using Conexoes;
using DLM.painel;
using DLM.sap;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace DLM.sapgui
{
    public class ConexaoSAP : IDisposable
    {
        private bool _disposed = false;

        public string Contrato
        {
            get
            {
                if (this.Codigo.LenghtStr() > 6)
                {
                    return this.Codigo.Substring(3, 6);
                }
                return "";
            }
        }
        public string Codigo { get; private set; } = "";
        public string Pedido { get; private set; } = "";

        public CN47N GETPEPENG(string PEPFABRICA)
        {
            var T = this.CN47N.Find(x => x.PEP == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".EN").Upper());
            if (T != null)
            {
                return T;
            }
            return new DLM.sapgui.CN47N();
        }
        public CN47N GETPEPMONT(string PEPFABRICA)
        {
            var lista = this.CN47N.Find(x => x.PEP == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".MO").Upper());
            if (lista != null)
            {
                return lista;
            }
            return new CN47N();
        }
        public List<CN47N> GETPEPSLOG(string PEPFABRICA)
        {
            var lista = this.CN47N.FindAll(x => x.PEP.Contem(Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".L"));
            return lista.ToList();
        }
        public string Descricao { get; set; } = "";

        public List<ZPP0100> ZPP0100 { get; set; } = new List<ZPP0100>();
        public List<ZPMP> ZPMP { get; set; } = new List<ZPMP>();
        public List<CN47N> CN47N { get; set; } = new List<CN47N>();
        public List<PEP_Planejamento> PEP_PLanejamento { get; set; } = new List<PEP_Planejamento>();
        public ConexaoSAP(string pedido)
        {
            this.Pedido = pedido;
            this.Codigo = pedido + "*";
        }
        public ConexaoSAP()
        {

        }
        public SAP_Avanco Avanco { get; set; }
        public bool ConsultaSAP(bool resultado = true, bool sincronizar = true, bool criar_cache = true)
        {
            if (this.Codigo.LenghtStr() < 10)
            {
                return false;
            }

            this.ZPP0100 = new List<ZPP0100>();
            this.ZPMP = new List<ZPMP>();
            this.PEP_PLanejamento = new List<PEP_Planejamento>();

            Consultas.AddMensagemStatus(this.Pedido, $"Consulta SAP Pedido -> {Pedido}");
            var ped = DLM.SAP.GetPedido(this.Pedido);
            if (ped == null)
            {
                return false;
            }
            Consultas.AddMensagemStatus(this.Pedido, $"Consulta SAP Encontrada -> {ped.ToString()}, iniciando a consulta do avanço...");
            if (this.Avanco == null)
                this.Avanco = DLM.SAP.ZGPP_GET_AVANCO(this.Pedido, true, true, true, false, true, true, resultado, resultado);

            if (sincronizar)
            {
                if (resultado)
                {
                    Consultas.AddMensagemStatus(this.Pedido, $"Consultando resultado da obra...");
                    this.Avanco.GetTabelasResultado();
                }

                Consultas.AddMensagemStatus(this.Pedido, $"Consultando peps cadastrados...");
                var peps_ped = ped.GetPeps();
                if (peps_ped?.Count > 0)
                {
                    foreach (var pep in peps_ped)
                    {
                        this.CN47N.Add(new CN47N
                        {
                            Data_Fim_Base = pep.DT_B_FIM,
                            Data_Inicio_Base = pep.DT_B_INI,
                            Fim_Previsto = pep.DT_P_FIM,
                            Inicio_Previsto = pep.DT_P_INI,
                            Texto_Operacao = pep.Descricao,
                            Status = string.Join(" ", pep.Status_Sistema.GetValores("BR_STAT")),
                            PEP = pep.PEP
                        });
                    }


                    Consultas.AddMensagemStatus(this.Pedido, $"Vinculando cargas com peças...");
                    if (this.Avanco[ZGPP_GET_AVANCO_TABS.PECAS].Count > 0)
                    {
                        this.ZPMP = Avanco[ZGPP_GET_AVANCO_TABS.PECAS].Select(x => new sapgui.ZPMP(x, true)).ToList();
                        this.ZPP0100 = Avanco[ZGPP_GET_AVANCO_TABS.CARGAS].Select(x => new sapgui.ZPP0100(x, true)).ToList();

                        if (this.ZPP0100.Count > 0)
                        {
                            var zppLookup = this.ZPP0100.ToLookup(x => new { x.POSNR, x.Material });

                            foreach (var p in this.ZPMP)
                            {
                                foreach (var l in zppLookup[new { p.POSNR, p.Material }])
                                {
                                    l.PEP = p.PEP;
                                    p.Descricao = l.Descricao;
                                }
                            }


                            int registrosRemovidos = this.ZPP0100.RemoveAll(x => x.PEP.LenghtStr() == 0);
                            if (registrosRemovidos > 0)
                            {
                                DLM.log.Log($"{this.Pedido} -> Contém {registrosRemovidos} registros sem PEP respectivo de ZPMP", "Painel de Obras.Log");
                            }
                        }
                    }

                    var peps_prod = new HashSet<string>(this.ZPMP.Select(x => x.PEP));
                    peps_prod.UnionWith(this.CN47N.Where(x => Conexoes.Utilz.PEP.Get.PEP(x.PEP).StartsW("F")).Select(x => x.PEP));
                    peps_prod.UnionWith(this.ZPP0100.Select(x => x.PEP));

                    var peps_prod_ordenado = peps_prod.OrderBy(x => x).ToList();

                    var zpmpByPep = this.ZPMP.ToLookup(x => x.PEP);
                    var zppByPep = this.ZPP0100.ToLookup(x => x.PEP);
                    var cn47nByPep = this.CN47N.GroupBy(x => x.PEP).ToDictionary(g => g.Key, g => g.FirstOrDefault());

                    foreach (var pep in peps_prod_ordenado)
                    {
                        this.PEP_PLanejamento.Add(new PEP_Planejamento(
                            pep,
                            zpmpByPep[pep].ToList(),
                            zppByPep[pep].ToList(),
                            cn47nByPep.ContainsKey(pep) ? cn47nByPep[pep] : null,
                            this
                        ));
                    }
                }

                /// - ajustes finais e banco de dados
                var db = DBases.GetDB();
                var dbComum = Cfg.Init.db_comum;

                Consultas.AddMensagemStatus(this.Pedido, $"Apagando dados existentes...");
                db.Apagar("pep", $"%{Pedido}%", dbComum, Cfg.Init.tb_pep_planejamento);
                db.Apagar("pep", $"%{Pedido}%", dbComum, Cfg.Init.tb_zpmp_producao);
                db.Apagar("Elemento_PEP", $"%{Pedido}%", dbComum, Cfg.Init.tb_zpp0100_embarques);
                db.Apagar("pep", $"%{Pedido}%", dbComum, Cfg.Init.tb_cn47n);

                if (this.PEP_PLanejamento.Count > 0)
                {
                    Consultas.AddMensagemStatus(this.Pedido, $"Cadastrando [db={dbComum}]...");
                    var peps = Funcoes.converter(this.PEP_PLanejamento);
                    db.Cadastro(peps.Select(x => x.GetLinha()).ToList(), dbComum, Cfg.Init.tb_pep_planejamento);

                    if (this.ZPMP.Count > 0)
                        db.Cadastro(this.ZPMP.Select(x => x.GetLinha()).ToList(), dbComum, Cfg.Init.tb_zpmp_producao);

                    if (this.ZPP0100.Count > 0)
                        db.Cadastro(this.ZPP0100.Select(x => x.GetLinha()).ToList(), dbComum, Cfg.Init.tb_zpp0100_embarques);

                    if (this.CN47N.Count > 0)
                        db.Cadastro(this.CN47N.Select(x => x.GetLinha()).ToList(), dbComum, Cfg.Init.tb_cn47n);
                }

                if (this.PEP_PLanejamento.Count > 0 && criar_cache)
                {
                    Consultas.AddMensagemStatus(this.Pedido, $"Criando cache...");
                    Consultas.CriarCache(this.Pedido.Replace("*", "").Replace("%", ""));
                }
            }
            Consultas.AddMensagemStatus(this.Pedido, $"Finalizado.");

            return this.Avanco.etapas > 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.ZPP0100?.Clear();
                    this.ZPP0100 = null;

                    this.ZPMP?.Clear();
                    this.ZPMP = null;

                    this.CN47N?.Clear();
                    this.CN47N = null;

                    this.PEP_PLanejamento?.Clear();
                    this.PEP_PLanejamento = null;

                    this.Avanco = null;
                }
                _disposed = true;
            }
        }
    }
}
