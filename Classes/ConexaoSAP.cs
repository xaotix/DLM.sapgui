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
            var T = this.CN47N.Find(x => x.PEP == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".MO").ToUpper());
            if (T != null)
            {
                return T;
            }
            return new CN47N();
        }
        public List<CN47N> GETPEPSLOG(string PEPFABRICA)
        {
            var T = this.CN47N.FindAll(x => x.PEP.Contains(Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".L"));
            return T.ToList();
        }
        public string Descricao { get; set; } = "";

        public SAP_Consulta_Macro Consulta { get; private set; } = new SAP_Consulta_Macro();
        public List<ZPP0100> ZPP0100 { get; set; } = new List<ZPP0100>();
        public List<ZPP0112> ZPP0112 { get; set; } = new List<ZPP0112>();
        public List<ZPMP> ZPMP { get; set; } = new List<ZPMP>();
        public List<CN47N> CN47N { get; set; } = new List<CN47N>();
        public List<PEP_Planejamento> PEP_PLanejamento { get; set; } = new List<PEP_Planejamento>();
        public ConexaoSAP(string Pedido)
        {
            this.Codigo = Pedido + "*";
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

            //var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP0100ARQ;
            //var arq_zpmp = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPMPARQ_SISTEMA;
            //var arq_cn47n = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_CN47NARQ;

            var sap = DLM.SAP.GetContratos().Find(x => x.Contrato == this.Contrato);
            if (sap != null)
            {
                foreach (var ped in sap.GetPedidos())
                {
                    if (!ped.PEP.Contains(".P"))
                    {
                        continue;
                    }
                    var peps = ped.GetPeps();
                    if (peps != null)
                    {
                        foreach (var pep in peps)
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
                }

                var w = Conexoes.Utilz.Wait(200, this.Codigo);
                w.somaProgresso();

                var avanco_sap = DLM.SAP.Get_Avanco(this.Codigo,true);
                if (avanco_sap.Tab_Pecas.Count > 0)
                {
                    this.ZPMP = avanco_sap.Tab_Pecas.Linhas.Select(x => new sapgui.ZPMP(x, true)).ToList();
                    this.ZPP0100 = avanco_sap.Tab_Cargas.Linhas.Select(x => new sapgui.ZPP0100(x, true)).ToList();
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
                        if(pp.Count > 0)
                        {
                            DLM.log.Log($"{this.Contrato} -> Contém {pp.Count} registros sem PEP respectivo de ZPMP",   "Painel de Obras.Log");
                        }
                    }
                }

                //monta o PEP_PLANEJAMENTO
                if (this.CN47N.Count > 0)
                {
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
                    DLM.painel.Consultas.MatarExcel(false);
                }

                //ajustes finais
                DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
                DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);
                DBases.GetDB().Apagar("Elemento_PEP", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
                DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n);

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




        public List<ZPP0112> GetZPP0112(long min, long max, bool salvar, string pedido = "")
        {
            DLM.painel.Consultas.MatarExcel(false);
            var arq_zpp0112 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP0112ARQ;

            if (Consulta.ZPP0112(Cfg.Init.GetDestinoSAP_Excel(), arq_zpp0112, min, max))
            {
                DLM.painel.Consultas.MatarExcel(false);
                this.ZPP0112 = CargaExcel.ZPP0112(Cfg.Init.GetDestinoSAP_Excel() + arq_zpp0112);
                if (salvar)
                {
                    DBases.GetDB().Apagar("Elemento_PEP", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0112);
                    DBases.GetDB().Cadastro(this.ZPP0112.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_zpp0112);
                }
            }
            return this.ZPP0112;
        }
    }
}
