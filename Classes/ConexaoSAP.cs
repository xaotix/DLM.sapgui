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
            string arquivo = pedido.Replace(".", "") + "_" + Vars.ZSD0031NARQ;
            var con = Consulta.ZSD0031N(pedido, this.Destino, arquivo);
            if (con)
            {
                if (File.Exists(this.Destino + arquivo))
                {
                    var folha = CargaExcel.ZSD0031N(this.Destino + arquivo);
                    folha.PEP = pedido;
                    if (folha.Carregado)
                    {
                        DLM.painel.Consultas.Salvar(folha, pedido);
                    }
                }
            }
            return con;
        }
        public string GravarTitulos()
        {
            var db = Conexoes.DBases.GetDB().Clonar();
            if(this.Contrato=="")
            {
                return "Contrato em branco";
            }
           foreach (var Pedido in DLM.painel.Consultas.GetPedidos(new List<string> { this.Contrato }))
            {
                try
                {
                    var t = DLM.sap.RfcsSAP.ConsultarPedido(Pedido.pedido);
                    if (t.Count == 0)
                    {
                        continue;
                    }
                    var lista = t.Select(x => x.Select(y => y.GetValue().ToString()).ToList()).ToList();

                    db.Comando($"delete from " +
                        $"{Cfg.Init.db_comum}.titulos_planejamento where " +
                        $"{Cfg.Init.db_comum}.titulos_planejamento.CHAVE like '$C$%'".Replace("$C$", Pedido.pedido));

                    List<string> linhas = new List<string>();
                    foreach (var ll in lista)
                    {
                        linhas.Add("('" + ll[1].Replace("'","") + "','" + ll[2].Replace("'", "") + "')");
                    }
                    var sublista = DLM.painel.Consultas.quebrar_lista(linhas, 300);
                    foreach (var sub in sublista)
                    {
                        var SUBCOMANDO = $"INSERT INTO {Cfg.Init.db_comum}.titulos_planejamento " +
                                        $"(CHAVE, DESCRICAO)" +
                                        $" VALUES ";

                        SUBCOMANDO = SUBCOMANDO + string.Join(",", sub);
                        db.Comando(SUBCOMANDO);
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;

                }
            }
            return "";
        }
        public void GravarPep_Planejamento()
        {
            var peps = Funcoes.converter(this.PEPsConsultaSAP);
            DLM.painel.Consultas.Apagar_peps(this.Contrato);

            Conexoes.DBases.GetDB().Cadastro(peps.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, "pep_planejamento");
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

        private List<DLM.painel.PEP_PLAN> _PEPs { get; set; }
        public List<DLM.painel.PEP_PLAN> PEPs
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

        public List<DLM.painel.PEP_PLAN> CarregarPEPS()
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

        public string Destino
        {
            get
            {
                if(_Destino==null)
                {
                    _Destino = Conexoes.Utilz.CriarPasta(Vars.Raiz, "SAP");
                    if(Conexoes.DBases.GetUserAtual().ma!="")
                    {
                    _Destino = Conexoes.Utilz.CriarPasta(Vars.Raiz, Conexoes.DBases.GetUserAtual().ma);
                    }
                    _Destino = Conexoes.Utilz.CriarPasta(_Destino, DateTime.Now.ToShortDateString().Replace(@"/","-").Replace(@"\","-"));
                }
                return _Destino;
            }
        }

        private string _Destino { get; set; }
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
            Conexoes.DBases.GetDB().Comando($"delete from " +
                $"{Cfg.Init.db_comum}.zpmp_producao where " +
                $"{Cfg.Init.db_comum}.zpmp_producao.pep like '" + this.Codigo.Replace("*", "") + "%'");

            }
            if(zpp0066n)
            {

            Conexoes.DBases.GetDB().Comando($"delete from " +
                $"{Cfg.Init.db_comum}.zpp0066n_logistica where " +
                $"{Cfg.Init.db_comum}.zpp0066n_logistica.pep like '" + this.Codigo.Replace("*", "") + "%'");
            }
            if (zpp0100)
            {
                Conexoes.DBases.GetDB().Comando($"delete from " +
                    $"{Cfg.Init.db_comum}.zpp0100_embarques where " +
                    $"{Cfg.Init.db_comum}.zpp0100_embarques.Elemento_PEP like '" + this.Codigo.Replace("*", "") + "%'");
            }

            Conexoes.DBases.GetDB().Comando($"delete from " +
               $"{Cfg.Init.db_comum}.cn47n where " +
               $"{Cfg.Init.db_comum}.cn47n.pep like '" + this.Codigo.Replace("*", "") + "%'");
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
                prod = SAP_Producao();
            }


            if (CN47N)
            {
                data = SAP_Datas();
            }


            if (ZPP0066N | ZPP0100)
            {
                var log = SAP_Logistica( true, ZPP0066N, ZPP0100);

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
            var con = Conexoes.DBases.GetDB().Clonar();

            if (this.Codigo.Length > 10 && (this.Producao.Count > 0 | this.Logistica.Count > 0 | this.Embarque.Count > 0))
            {
                var count = this.Producao.Count + this.Logistica.Count + this.Embarque.Count;

                LimparMateriais(this.Producao.Count > 0, this.Logistica.Count > 0, this.Embarque.Count > 0);

                if (this.Producao.Count > 0)
                {
                    var linhas = this.Producao.Select(x => x.GetLinha()).ToList();
                   var ok = con.Cadastro(linhas, Cfg.Init.db_comum, "zpmp_producao");
                }

                if (this.Logistica.Count > 0)
                {
                    var linhas = this.Logistica.Select(x => x.GetLinha()).ToList();
                    var ok = con.Cadastro(linhas, Cfg.Init.db_comum, "zpp0066n_logistica");
                }

                if (this.Embarque.Count > 0)
                {
                    var linhas = this.Embarque.Select(x => x.GetLinha()).ToList();
                    var ok = con.Cadastro(linhas, Cfg.Init.db_comum, "zpp0100_embarques");
                }

                if(this.Datas.Count>0)
                {
                    con.Cadastro(this.Datas.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, "cn47n");
                }
            }
        }

        public List<FAGLL03> GetFAGLL03(bool salvar)
        {
            bool con = false;
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<FAGLL03>(); }
            if (this.Codigo.Length < 3) { return new List<FAGLL03>(); }
            var arquivo = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.FAGLL03ARQ;
          var peps =  Conexoes.DBases.GetDB().Consulta($"SELECT pr.pep as pep from {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento} as pr where pr.pep like '%{Codigo.Replace("*", "")}% '").Linhas.Select(x=>x.Get("pep").ToString()).ToList();



            var sub_lista = peps.quebrar_lista(200);

            if(salvar && peps.Count>0)
            {
                Conexoes.DBases.GetDB().Comando($"delete from " +
                    $"{Cfg.Init.db_comum}.fagll03 where " +
                    $"{Cfg.Init.db_comum}.fagll03.Pedido like '%$C$%'".Replace("$C$", Codigo.Replace("*", "")));

            }

            this.fagll03.Clear();
            var w = Conexoes.Utilz.Wait(sub_lista.Count, "FAGLL03 - 1/2 - Consultando..."); 
         
            for (int i = 0; i < sub_lista.Count; i++)
            {
                con = Consulta.FAGLL03(sub_lista[i], Destino, i + "_" + arquivo);
                if (con)
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    var CARGA = CargaExcel.FAGLL03(this.Destino + i + "_" + arquivo, this.Codigo.Replace("*","").Replace("%",""));
                    this.fagll03.AddRange(CARGA);
                }
                w.somaProgresso();
            }


            if (salvar)
            {
                w.SetProgresso(0, this.fagll03.Count, "FAGLL03 - 2/2 - Salvando...");
                var ok = Conexoes.DBases.GetDB().Cadastro(this.fagll03.Select(x=>x.GetLinha()).ToList(), Cfg.Init.db_comum, "fagll03");
              
            }

            w.Close();

            return this.fagll03;
        }

        public List<CJI3> GetCJI3(bool salvar)
        {
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<CJI3>(); }
            if (this.Codigo.Length < 3) { return new List<CJI3>(); }
            var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.CJI3ARQ;

            if (Consulta.CJI3(this.Codigo, Destino, arq0100))
            {
                DLM.painel.Consultas.MatarExcel(false);
                this.cji3 = CargaExcel.CJI3(this.Destino + arq0100);
                if (salvar)
                {
                    Conexoes.DBases.GetDB().Comando($"delete from " +
                        $"{Cfg.Init.db_comum}.cji3 where " +
                        $"{Cfg.Init.db_comum}.cji3.Elemento_PEP like '%$C$%'".Replace("$C$", Codigo.Replace("*","")));
                    var w = Conexoes.Utilz.Wait(this.cji3.Count, "Salvando...");                     
                    var ok = Conexoes.DBases.GetDB().Cadastro(this.cji3.Select(x=>x.GetLinha()).ToList(), Cfg.Init.db_comum, "cji3");
                    w.Close();
                }
            }
            return this.cji3;
        }
        public List<ZPP0112> GetZPP0112(long min, long max, bool salvar, string pedido = "")
        {
            if (!DLM.painel.Consultas.MatarExcel(false)) { return new List<ZPP0112>(); }
            //if (this.Codigo.Length < 3) { return new List<ZPP0112>(); }
            var ARQ = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.ZPP0112ARQ;

            if (Consulta.ZPP0112(Destino, ARQ, min, max))
            {
                DLM.painel.Consultas.MatarExcel(false);
                this.ZPP0112 = CargaExcel.ZPP0112(this.Destino + ARQ);
                if (salvar)
                {
                    Conexoes.DBases.GetDB().Clonar().Comando($"delete from {Cfg.Init.db_comum}.zpp0112 where {Cfg.Init.db_comum}.zpp0112.Elemento_PEP like '%{pedido}% '");
                    var w = Conexoes.Utilz.Wait(this.ZPP0112.Count, "Salvando..."); 
                    w.Show();
                    var ok = Conexoes.DBases.GetDB().Cadastro(this.ZPP0112.Select(x => x.GetLinha()).ToList(), Cfg.Init.db_comum, "zpp0112");
                    w.Close();
                }
            }
            return this.ZPP0112;
        }

        public bool SAP_Logistica( bool sem_perfil, bool ZPP0066N, bool ZPP0100)
        {
            this.Logistica = new List<ZPP0066N>();
            this.Embarque = new List<ZPP0100>();

            var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.ZPP066NARQ;
            var arq0100 = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.ZPP0100ARQ;

            if (ZPP0066N)
            {
                if(sem_perfil)
                {
                    if (Consulta.ZPP0066N_SemPerfil(this.Codigo, Destino, arq))
                    {
                        DLM.painel.Consultas.MatarExcel(false);
                        this.Logistica = CargaExcel.ZPP0066N(this.Destino + arq, true);
                    }
                }
                else
                {
                    if (Consulta.ZPP0066N(this.Codigo, Destino, arq))
                    {
                        DLM.painel.Consultas.MatarExcel(false);
                        this.Logistica = CargaExcel.ZPP0066N(this.Destino + arq, false);
                    }
                }
            }

            if (ZPP0100)
            {
                if (Consulta.ZPP0100(this.Codigo, Destino, arq0100))
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    this.Embarque = CargaExcel.ZPP0100(this.Destino + arq0100);
                }
            }


            if (this.Logistica.Count > 0 | this.Embarque.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool SAP_Producao()
        {
             var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.ZPMPARQ;
            if (Consulta.ZPMP(this.Codigo, Destino,  arq))
            {
                this.Producao = CargaExcel.ZPMP(this.Destino + arq);
                return true;
            }
            return false;
        }

        public bool SAP_Datas()
        {
            var arq = this.Codigo.Replace("*", "").Replace("%", "") + "_" + Vars.CN47NARQ;
            if (Consulta.CN47N(this.Codigo, this.Destino, arq))
            {
                this.Datas = CargaExcel.CN47N(this.Destino + arq);
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
