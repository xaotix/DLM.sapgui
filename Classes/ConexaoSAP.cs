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
                if(this.Codigo.Length>6)
                {
                    return this.Codigo.Substring(3, 6);
                }
                return "";
            }
        }
        public string Codigo { get; private set; } = "";

        public CN47N GETPEPENG(string PEPFABRICA)
        {
            var T = this.CN47N.Find(x => x.PEP.Codigo.ToUpper() == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".EN").ToUpper());
            if (T!=null)
            {
                return T;
            }
            return new DLM.sapgui.CN47N();
        }
        public CN47N GETPEPMONT(string PEPFABRICA)
        {
            var T = this.CN47N.Find(x => x.PEP.Codigo.ToUpper() == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA,true) + ".MO").ToUpper());
            if (T != null)
            {
                return T;
            }
            return new CN47N();
        }
        public List<CN47N> GETPEPSLOG(string PEPFABRICA)
        {
            var T = this.CN47N.FindAll(x => x.PEP.Codigo.ToUpper().Contains(Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".L"));
            return T.ToList() ;
        }
        public string Descricao { get; set; } = "";

        public List<PLAN_CONTRATO> Titulos { get; set; } = new List<PLAN_CONTRATO>();

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
            this.Titulos = new List<PLAN_CONTRATO>();

            var tabela_zpmp = new db.Tabela();
            var tabela_zpp0100 = new db.Tabela();
            var tabela_cn47n = new DLM.db.Tabela();



            var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP0100ARQ;
            var arq_zpmp = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPMPARQ_SISTEMA;
            var arq_cn47n = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_CN47NARQ;



            //cronograma
            if (Consulta.CN47N(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq_cn47n))
            {
                this.CN47N = CargaExcel.CN47N(Cfg.Init.GetDestinoSAP_Excel() + arq_cn47n, out tabela_cn47n);
            }
            
            //fábrica / engenharia
            if (Consulta.ZPMP(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq_zpmp))
            {
                this.ZPMP.AddRange(CargaExcel.ZPMP(Cfg.Init.GetDestinoSAP_Excel() + arq_zpmp, out tabela_zpmp));
            }

            //logística
            if (Consulta.ZPP0100(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq0100))
            {
                this.ZPP0100 = CargaExcel.ZPP0100(Cfg.Init.GetDestinoSAP_Excel() + arq0100, out tabela_zpp0100);
            }


            //monta o PEP_PLANEJAMENTO
            if (this.ZPMP.Count>0  && this.CN47N.Count>0)
            {
                var peps_producao = this.ZPMP.Select(x => x.PEP.Codigo).Distinct().ToList();
                peps_producao.AddRange(this.ZPMP.Select(x => x.PEP.Codigo).Distinct().ToList());
                peps_producao = peps_producao.Distinct().ToList().OrderBy(x => x).ToList();

                foreach (var pep in peps_producao)
                {
                    PEP_PLanejamento.Add(new PEP_Planejamento(
                        pep,
                        this.ZPMP.FindAll(x => x.PEP.Codigo == pep).ToList(),
                        this.ZPP0100.FindAll(x => x.PEP.Codigo == pep).ToList(),
                        this.CN47N.Find(x => x.PEP.Codigo == pep),
                        this
                        )
                        );
                }
                DLM.painel.Consultas.MatarExcel(false);
            }

            //ajustes finais
            if (tabela_zpmp.Count > 0)
            {
                this.Descricao = tabela_zpmp[0][(int)TAB_ZPMP.DENOMINACAO].Valor;
                var pdes = tabela_zpmp.Linhas.GroupBy(x => Conexoes.Utilz.PEP.Get.Pedido(x[(int)TAB_ZPMP.ELEMENTO_PEP].Valor,true)).Select(x => x.First()).ToList();
                var pedidos = pdes
                    .Select(x => $"{Conexoes.Utilz.PEP.Get.Pedido(x[(int)TAB_ZPMP.ELEMENTO_PEP].Valor,true)};{x[(int)TAB_ZPMP.DENOMINACAO].Valor}").Distinct().ToList().Select(x=>x.Split(';').ToList()).ToList();
                foreach(var ped in pedidos)
                {
                    PLAN_CONTRATO titulo = new PLAN_CONTRATO();
                    titulo.Contrato = ped[0];
                    titulo.Descricao = ped[1];
                    this.Titulos.Add(titulo);
                    this.Titulos = this.Titulos.FindAll(x => x.Contrato.Length > 0);
                }
            }


            DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
            DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
            DBases.GetDB().Apagar("pep", $"%{Contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n);
            DBases.GetDB().Apagar("contrato", $"%{Contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);

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
                if (this.Titulos.Count > 0)
                {
                    DBases.GetDB().Cadastro(this.Titulos.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
                }
            }
            return this.PEP_PLanejamento.Count>0;
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
