using Conexoes;
using DLM.painel;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DLM.painel.Colunas;

namespace DLM.sapgui
{
    public class ConexaoSAP
    {
        public bool ZSD0031N(string pedido)
        {
            string arquivo = pedido.Replace(".", "") + "_" + Cfg.Init.SAP_ZSD0031NARQ;
            var con = Consulta.ZSD0031N(pedido, Cfg.Init.GetDestinoSAP_Excel(), arquivo);
            if (con)
            {
                if (File.Exists(Cfg.Init.GetDestinoSAP_Excel() + arquivo))
                {
                    var folha = CargaExcel.ZSD0031N(Cfg.Init.GetDestinoSAP_Excel() + arquivo);
                    folha.PEP = pedido;
                    if (folha.Carregado)
                    {
                        DLM.painel.Consultas.Salvar(folha, pedido);
                    }
                }
            }
            return con;
        }
        public static string GravarTitulos(List<string> codigos_pedidos)
        {
            var db = DBases.GetDB().Clonar();
            var w = Conexoes.Utilz.Wait(codigos_pedidos.Count,"Gravando titulos...");

            foreach (var Pedido in codigos_pedidos)
            {
                try
                {
                    var consulta = DLM.sap.RfcsSAP.ConsultarPedido(Pedido);
                    if (consulta.Count == 0)
                    {
                        continue;
                    }
                    var lista = consulta.Select(x => x.Select(y => y.GetValue().ToString()).ToList()).ToList();

                    db.Apagar("CHAVE", $"%{Pedido.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_titulos_planejamento, false);


                    List<string> linhas = new List<string>();
                    foreach (var ll in lista)
                    {
                        linhas.Add("('" + ll[1].Replace("'", "") + "','" + ll[2].Replace("'", "") + "')");
                    }
                    var sublista = DLM.painel.Consultas.quebrar_lista(linhas, 100);
                    foreach (var sub in sublista)
                    {
                        var SUBCOMANDO = $"INSERT INTO {Cfg.Init.db_comum}.{Cfg.Init.tb_titulos_planejamento}" +
                                        $"(CHAVE, DESCRICAO)" +
                                        $" VALUES ";

                        SUBCOMANDO = SUBCOMANDO + string.Join(",", sub);
                        db.Comando(SUBCOMANDO);
                    }
                    w.somaProgresso();
                }
                catch (Exception ex)
                {
                    //return ex.Message;

                }
            }
            w.Close();
            return "";
        }



        public void GravarPep_Planejamento()
        {
            var peps = Funcoes.converter(this.PEPsConsultaSAP);
            DLM.painel.Consultas.Apagar_peps(this.Contrato);

            DBases.GetDB().Cadastro(peps.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
        }
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

        private List<DLM.painel.PLAN_PEP> _PEPs { get; set; }
        public List<DLM.painel.PLAN_PEP> PEPs
        {
            get
            {
                if(_PEPs == null)
                {
                    _PEPs = CarregarPEPS();
                }
                return _PEPs;
            }
        }

        public List<DLM.painel.PLAN_PEP> CarregarPEPS()
        {
            return DLM.painel.Consultas.GetPeps(new List<string> { this.Codigo });
        }


        public CN47N_Datas GETPEPFAB(string PEPFABRICA)
        {
            var T = this.Datas.Find(x => x.PEP.Codigo.ToUpper() == PEPFABRICA);
            if (T != null)
            {
                return T;
            }
            return new CN47N_Datas();
        }

        public CN47N_Datas GETPEPENG(string PEPFABRICA)
        {
            var T = this.Datas.Find(x => x.PEP.Codigo.ToUpper() == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".EN").ToUpper());
            if (T!=null)
            {
                return T;
            }
            return new DLM.sapgui.CN47N_Datas();
        }
        public CN47N_Datas GETPEPMONT(string PEPFABRICA)
        {
            var T = this.Datas.Find(x => x.PEP.Codigo.ToUpper() == (Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA,true) + ".MO").ToUpper());
            if (T != null)
            {
                return T;
            }
            return new CN47N_Datas();
        }
        public List<CN47N_Datas> GETPEPSLOG(string PEPFABRICA)
        {
            var T = this.Datas.FindAll(x => x.PEP.Codigo.ToUpper().Contains(Conexoes.Utilz.PEP.Get.Subetapa(PEPFABRICA, true) + ".L"));
            return T.ToList() ;
        }
        public string Descricao { get; set; } = "";
        public double Peso_Total
        {
            get
            {
                return Producao.Sum(x => Conexoes.Utilz.Double(x.peso_necessario));
            }
        }


        public Consulta Consulta { get; private set; } = new Consulta();
        public string Codigo { get; private set; } = "";
        public List<ZPP0066N> Logistica { get; set; } = new List<ZPP0066N>();
        public List<ZPP0100> Embarque { get; set; } = new List<ZPP0100>();

        public List<CJI3> cji3 { get; set; } = new List<CJI3>();
        public List<ZPP0112> ZPP0112 { get; set; } = new List<ZPP0112>();
        public List<FAGLL03> fagll03 { get; set; } = new List<FAGLL03>();

        public List<ZPMP> Producao { get; set; } = new List<ZPMP>();
        public List<CN47N_Datas> Datas { get; set; } = new List<CN47N_Datas>();
        public List<PEPConsultaSAP> PEPsConsultaSAP { get; set; } = new List<PEPConsultaSAP>();
        public ConexaoSAP(string Pedido)
        {
            this.Codigo = Pedido + "*";
            //Pesquisar();

        }

        public ConexaoSAP(DLM.painel.PLAN_PEDIDO PED)
        {
            this.Codigo = PED.pedido;
            this._PEPs = PED.GetPeps();
        }
        public ConexaoSAP()
        {

        }


        public void LimparMateriais(bool zpmp, bool zpp0066n, bool zpp0100)
        {
            if(this.Codigo.Length<5)
            {
                return;
            }
            if(zpmp)
            {
                DBases.GetDB().Apagar("pep", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao, false);
            }
            if(zpp0066n)
            {
                DBases.GetDB().Apagar("pep", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0066n_logistica, false);
            }
            if (zpp0100)
            {
                DBases.GetDB().Apagar("Elemento_PEP", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques, false);
            }
         
            DBases.GetDB().Apagar("pep", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n, false);

        }

        public bool ConsultaSAP( bool CN47N = true, bool ZPMP = true, bool ZPP0066N = false, bool ZPP0100 = true)
        {
            if (this.Codigo.Length < 10)
            {
                return false;
            }
            if (!DLM.painel.Consultas.MatarExcel(false)) { return false; }
            
            var prod = false;
            var data = false;
            if (ZPMP)
            {
                prod = this.ZPMP();
            }


            if (CN47N)
            {
                data = SAP_Datas();
            }


            if (ZPP0066N | ZPP0100)
            {
                var log = this.ZPP0100( true, ZPP0066N, ZPP0100);

                if (this.Codigo.StartsWith("*") && this.Logistica.Count > 0)
                {
                    this.Codigo = this.Logistica[0].PEP.Codigo.Substring(0, 13);
                }
            }


            if (prod  && data)
            {
                GetPEPs();
                DLM.painel.Consultas.MatarExcel(false);
            }
            else if(ZPMP && CN47N)
            {
                return false;
            }



            return true;

        }

        public void GravarMateriais()
        {

            if (this.Codigo.Length > 10 && (this.Producao.Count > 0 | this.Logistica.Count > 0 | this.Embarque.Count > 0))
            {
                var count = this.Producao.Count + this.Logistica.Count + this.Embarque.Count;

                LimparMateriais(this.Producao.Count > 0, this.Logistica.Count > 0, this.Embarque.Count > 0);

                if (this.Producao.Count > 0)
                {
                    var linhas = this.Producao.Select(x => x.GetLinha()).ToList();
                    DBases.GetDB().Cadastro(linhas, Cfg.Init.db_comum, "zpmp_producao");
                }

                if (this.Logistica.Count > 0)
                {
                    var linhas = this.Logistica.Select(x => x.GetLinha()).ToList();
                    DBases.GetDB().Cadastro(linhas, Cfg.Init.db_comum, "zpp0066n_logistica");
                }

                if (this.Embarque.Count > 0)
                {
                    var linhas = this.Embarque.Select(x => x.GetLinha()).ToList();
                    DBases.GetDB().Cadastro(linhas, Cfg.Init.db_comum, "zpp0100_embarques");
                }

                if(this.Datas.Count>0)
                {
                    DBases.GetDB().Cadastro(this.Datas.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, "cn47n");
                }
            }
        }

        public List<FAGLL03> GetFAGLL03(bool salvar)
        {
            bool con = false;
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<FAGLL03>(); }
            if (this.Codigo.Length < 3) { return new List<FAGLL03>(); }
            var arquivo = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_FAGLL03ARQ;
          var peps =  DBases.GetDB().Consulta($"SELECT pr.pep as pep from {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento} as pr where pr.pep like '%{Codigo.Replace("*", "")}% '").Linhas.Select(x=>x.Get("pep").ToString()).ToList();



            var sub_lista = peps.quebrar_lista(200);

            if(salvar && peps.Count>0)
            {
                DBases.GetDB().Apagar("Pedido", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_fagll03, false);
            }

            this.fagll03.Clear();
            var w = Conexoes.Utilz.Wait(sub_lista.Count, "FAGLL03 - 1/2 - Consultando..."); 
         
            for (int i = 0; i < sub_lista.Count; i++)
            {
                con = Consulta.FAGLL03(sub_lista[i], Cfg.Init.GetDestinoSAP_Excel(), i + "_" + arquivo);
                if (con)
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    var CARGA = CargaExcel.FAGLL03(Cfg.Init.GetDestinoSAP_Excel() + i + "_" + arquivo, this.Codigo.Replace("*","").Replace("%",""));
                    this.fagll03.AddRange(CARGA);
                }
                w.somaProgresso();
            }


            if (salvar)
            {
                w.SetProgresso(0, this.fagll03.Count, "FAGLL03 - 2/2 - Salvando...");
                DBases.GetDB().Cadastro(this.fagll03.Select(x=>x.GetLinha()).ToList(), Cfg.Init.db_comum, "fagll03");
              
            }

            w.Close();

            return this.fagll03;
        }

        public List<CJI3> GetCJI3(bool salvar)
        {
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<CJI3>(); }
            if (this.Codigo.Length < 3) { return new List<CJI3>(); }
            var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_CJI3ARQ;

            if (Consulta.CJI3(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq0100))
            {
                DLM.painel.Consultas.MatarExcel(false);
                this.cji3 = CargaExcel.CJI3(Cfg.Init.GetDestinoSAP_Excel() + arq0100);
                if (salvar)
                {
                    DBases.GetDB().Apagar("Elemento_PEP", $"%{Codigo.Replace("*", "")}%", Cfg.Init.db_comum, Cfg.Init.tb_cji3, false);
                    var w = Conexoes.Utilz.Wait(this.cji3.Count, "Salvando...");                     
                    DBases.GetDB().Cadastro(this.cji3.Select(x=>x.GetLinha()).ToList(), Cfg.Init.db_comum, "cji3");
                    w.Close();
                }
            }
            return this.cji3;
        }
        public List<ZPP0112> GetZPP0112(long min, long max, bool salvar, string pedido = "")
        {
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<ZPP0112>(); }
            //if (this.Codigo.Length < 3) { return new List<ZPP0112>(); }
            var ARQ = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP0112ARQ;

            if (Consulta.ZPP0112(Cfg.Init.GetDestinoSAP_Excel(), ARQ, min, max))
            {
                DLM.painel.Consultas.MatarExcel(false);
                this.ZPP0112 = CargaExcel.ZPP0112(Cfg.Init.GetDestinoSAP_Excel() + ARQ);
                if (salvar)
                {
                    DBases.GetDB().Apagar("Elemento_PEP", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0112, false);
                    //DBases.GetDBMySQL().Clonar().Comando($"delete from {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0112} where {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0112}.Elemento_PEP like '%{pedido}%'");
                    var w = Conexoes.Utilz.Wait(this.ZPP0112.Count, "Salvando..."); 
                    w.Show();
                    DBases.GetDB().Cadastro(this.ZPP0112.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_zpp0112);
                    w.Close();
                }
            }
            return this.ZPP0112;
        }

        public bool ZPP0100( bool sem_perfil, bool ZPP0066N, bool ZPP0100)
        {
            this.Logistica = new List<ZPP0066N>();
            this.Embarque = new List<ZPP0100>();

            var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP066NARQ;
            var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPP0100ARQ;

            if (ZPP0066N)
            {
                if(sem_perfil)
                {
                    if (Consulta.ZPP0066N_SemPerfil(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq))
                    {
                        DLM.painel.Consultas.MatarExcel(false);
                        this.Logistica = CargaExcel.ZPP0066N(Cfg.Init.GetDestinoSAP_Excel() + arq, true);
                    }
                }
                else
                {
                    if (Consulta.ZPP0066N(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq))
                    {
                        DLM.painel.Consultas.MatarExcel(false);
                        this.Logistica = CargaExcel.ZPP0066N(Cfg.Init.GetDestinoSAP_Excel() + arq, false);
                    }
                }
            }

            if (ZPP0100)
            {
                if (Consulta.ZPP0100(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq0100))
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    this.Embarque = CargaExcel.ZPP0100(Cfg.Init.GetDestinoSAP_Excel() + arq0100);
                }
            }


            if (this.Logistica.Count > 0 | this.Embarque.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool ZPMP()
        {
             var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_ZPMPARQ;
            if (Consulta.ZPMP(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(),  arq))
            {
                this.Producao = CargaExcel.ZPMP(Cfg.Init.GetDestinoSAP_Excel() + arq);
                return true;
            }
            return false;
        }

        public bool SAP_Datas()
        {
            var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Cfg.Init.SAP_CN47NARQ;
            if (Consulta.CN47N(this.Codigo, Cfg.Init.GetDestinoSAP_Excel(), arq))
            {
                this.Datas = CargaExcel.CN47N(Cfg.Init.GetDestinoSAP_Excel() + arq);
                return true;
            }
            return false;
        }




        public bool MatarExcel(bool confirmar = false)
        {
            return DLM.painel.Consultas.MatarExcel(confirmar);
        }

        private void GetPEPs()
        {
            this.PEPsConsultaSAP = new List<PEPConsultaSAP>();

            var peps_producao = this.Producao.Select(x => x.PEP.Codigo).Distinct().ToList();
            peps_producao.AddRange(this.Producao.Select(x => x.PEP.Codigo).Distinct().ToList());
            peps_producao = peps_producao.Distinct().ToList().OrderBy(x => x).ToList();
            foreach (var pep in peps_producao)
            {
                PEPsConsultaSAP.Add(new PEPConsultaSAP(
                    pep,
                    this.Producao.FindAll(x => x.PEP.Codigo == pep).ToList(),
                    this.Embarque.FindAll(x=>x.PEP.Codigo == pep).ToList(),
                    this.Datas.Find(x => x.PEP.Codigo == pep),
                    this
                    )
                    );
            }
        }
    }
}
