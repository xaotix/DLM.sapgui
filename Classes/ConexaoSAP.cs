using Conexoes;
using DLM.painel;
using DLM.vars;
using System.Collections.Generic;
using System.Linq;

namespace DLM.sapgui
{
    public class ConexaoSAP
    {
        public string Contrato
        {
            get
            {
                if (this.Codigo.Length > 6)
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
            var T = this.CN47N.Find(x => x.PEP == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".EN").ToUpper());
            if (T != null)
            {
                return T;
            }
            return new DLM.sapgui.CN47N();
        }
        public CN47N GETPEPMONT(string PEPFABRICA)
        {
            var lista = this.CN47N.Find(x => x.PEP == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".MO").ToUpper());
            if (lista != null)
            {
                return lista;
            }
            return new CN47N();
        }
        public List<CN47N> GETPEPSLOG(string PEPFABRICA)
        {
            var lista = this.CN47N.FindAll(x => x.PEP.Contains(Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".L"));
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
            //Pesquisar();

        }
        public ConexaoSAP()
        {

        }

        public bool ConsultaSAP()
        {
            if (this.Codigo.Length < 10)
            {
                return false;
            }
            this.ZPP0100 = new List<ZPP0100>();
            this.ZPMP = new List<ZPMP>();
            this.PEP_PLanejamento = new List<PEP_Planejamento>();

            var tabela_zpmp = new db.Tabela();
            var tabela_zpp0100 = new db.Tabela();
            var tabela_cn47n = new DLM.db.Tabela();

            var ped = DLM.SAP.GetPedido(this.Pedido);
            if (ped != null)
            {
                var peps_ped = ped.GetPeps();
                if (peps_ped != null)
                {
                    foreach (var pep in peps_ped)
                    {
                        var ncn = new CN47N();
                        ncn.Data_Fim_Base = pep.DT_B_FIM;
                        ncn.Data_Inicio_Base = pep.DT_B_INI;
                        ncn.Fim_Previsto = pep.DT_P_FIM;
                        ncn.Inicio_Previsto = pep.DT_P_INI;
                        ncn.Texto_Operacao = pep.Descricao;
                        ncn.Status = string.Join(" ", pep.Status_Sistema.GetValores("BR_STAT"));
                        ncn.PEP = pep.PEP;
                        this.CN47N.Add(ncn);
                    }
                }

                //monta o PEP_PLANEJAMENTO
                if (this.CN47N.Count > 0)
                {
                    //novo mapeamento
                    //todo = eliminar sistema atual, substituindo-o por este
                    var avanco_sap = DLM.SAP.Get_Avanco(this.Pedido, true, true, true, false, true, true);
                    if (avanco_sap.Tabelas["PECAS"].Count > 0)
                    {
                        this.ZPMP = avanco_sap.Tabelas["PECAS"].Select(x => new sapgui.ZPMP(x, true)).ToList();
                        this.ZPP0100 = avanco_sap.Tabelas["CARGAS"].Select(x => new sapgui.ZPP0100(x, true)).ToList();
                        if (this.ZPP0100.Count > 0)
                        {
                            foreach (var p in this.ZPMP)
                            {
                                var log = this.ZPP0100.FindAll(x => x.POSNR == p.POSNR && x.Material == p.Material);
                                foreach (var l in log)
                                {
                                    l.PEP = p.PEP;
                                    p.Descricao = l.Descricao;
                                }
                            }
                            var pp = this.ZPP0100.FindAll(x => x.PEP.Length == 0).ToList();
                            foreach (var p in pp)
                            {
                                this.ZPP0100.Remove(p);
                            }
                            if (pp.Count > 0)
                            {
                                DLM.log.Log($"{this.Pedido} -> Contém {pp.Count} registros sem PEP respectivo de ZPMP", "Painel de Obras.Log");
                            }
                        }
                    }

                    var peps_prod = this.ZPMP.Select(x => x.PEP).Distinct().ToList();
                    peps_prod.AddRange(this.ZPMP.Select(x => x.PEP).Distinct().ToList());
                    peps_prod.AddRange(this.CN47N.FindAll(x => Conexoes.Utilz.PEP.Get.PEP(x.PEP).StartsW("F")).Select(x => x.PEP));
                    peps_prod.AddRange(this.ZPP0100.Select(x => x.PEP));
                    peps_prod = peps_prod.Distinct().ToList().OrderBy(x => x).ToList();

                    foreach (var pep in peps_prod)
                    {
                        PEP_PLanejamento.Add(new PEP_Planejamento(
                            pep,
                            this.ZPMP.FindAll(x => x.PEP == pep).ToList(),
                            this.ZPP0100.FindAll(x => x.PEP == pep).ToList(),
                            this.CN47N.Find(x => x.PEP == pep),
                            this
                            )
                            );
                    }
                }

                //ajustes finais
                DBases.GetDB().Apagar("pep", $"%{Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
                DBases.GetDB().Apagar("pep", $"%{Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);
                DBases.GetDB().Apagar("Elemento_PEP", $"%{Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
                DBases.GetDB().Apagar("pep", $"%{Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n);

                if (this.PEP_PLanejamento.Count > 0)
                {
                    var count = this.ZPMP.Count + this.ZPP0100.Count;

                    if (this.PEP_PLanejamento.Count > 0)
                    {
                        var peps = Funcoes.converter(this.PEP_PLanejamento);
                        DBases.GetDB().Cadastro(peps.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
                    }
                    if (this.ZPMP.Count > 0)
                    {
                        DBases.GetDB().Cadastro(this.ZPMP.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);
                    }

                    if (this.ZPP0100.Count > 0)
                    {
                        DBases.GetDB().Cadastro(this.ZPP0100.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
                    }

                    if (this.CN47N.Count > 0)
                    {
                        DBases.GetDB().Cadastro(this.CN47N.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_cn47n);
                    }
                }
            }
            return this.PEP_PLanejamento.Count > 0;
        }
    }
}
