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
using System.Windows;

namespace DLM.sapgui
{
   public partial class Funcoes
    {
        public static bool GravarZContratos(List<string> pedidos)
        {
            pedidos = pedidos.FindAll(x => x.Replace(" ", "") != "");
            DLM.sapgui.Consulta sap = new Consulta();
            var w = Conexoes.Utilz.Wait(pedidos.Count, "Procurando notas fiscais...");
            var destino = Conexoes.Utilz.CriarPasta(Vars.Raiz, "SAP");
            var CON = false;
            if (Directory.Exists(destino))
            {
                foreach (var ST in pedidos)
                {
                    string arnome = ST.Replace(".", "").Replace("*", "") + Vars.ZCONTRATOSARQ;
                    CON = sap.ZCONTRATOS(ST, destino, ST.Replace(".", "").Replace("*", "") + Vars.ZCONTRATOSARQ);
                    if (CON)
                    {
                        DBases.GetDB().Apagar("Elemento_PEP", $"%{ST}%", Cfg.Init.db_comum, Cfg.Init.tb_zcontratos_notas_fiscais, true);
                        DLM.painel.Consultas.MatarExcel(false);
                        List<ZCONTRATOS> notas =  CargaExcel.ZCONTRATO(destino + arnome);
                       var ok = Conexoes.DBases.GetDB().Cadastro(notas.Select(x=>x.GetLinha()).ToList(), Cfg.Init.db_comum, Cfg.Init.tb_zcontratos_notas_fiscais);
                    }
                    else
                    {
                        w.Close();
                        return CON;
                    }

                w.somaProgresso();
                }
            }
            else
            {
                w.Close();

                return false;
            }

            w.Close();
            return CON;
        }
        public static void RodaScript(List<string> Script, string Destino)
        {

            if (Script.Count() > 0)
            {
                try
                {
                    if (File.Exists(Vars.SCRIPT_IMPRESSAO_tmp))
                    {
                        File.Delete(Vars.SCRIPT_IMPRESSAO_tmp);

                    }
                    Biblioteca_Daniel.Arquivo_Pasta.Buffer_Texto.gravar_arquivo(Vars.SCRIPT_IMPRESSAO_tmp, Script.ToList());
                    Process scriptProc = new Process();
                    scriptProc.StartInfo.FileName = Conexoes.Utilz.getNome(Vars.SCRIPT_IMPRESSAO_tmp) + ".vbs";
                    scriptProc.StartInfo.WorkingDirectory = Conexoes.Utilz.getPasta(Vars.SCRIPT_IMPRESSAO_tmp); //<---very important 
                                                                                                                       //scriptProc.StartInfo.Arguments = "//B //Nologo vbscript.vbs";
                    scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; //prevent console window from popping up
                    scriptProc.Start();
                    scriptProc.WaitForExit(); // <-- Optional if you want program running until your script exit
                    scriptProc.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Operação abortada\n" + ex.Message + "\n" + ex.StackTrace);
                    //MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        public static List<CN47N_Datas> GetCronograma(string Pedido)
        {
            var Datas = new List<CN47N_Datas>();
            var t = DLM.sap.RfcsSAP.ConsultarPedido(Pedido);
            foreach (var s in t)
            {
                Datas.Add(new CN47N_Datas(s));
            }
            return Datas;
        }
        private  static List<PLAN_PEDIDO> _pedidos { get; set; }
        public static List<PLAN_PEDIDO> GetPedidos()
        {
            if(_pedidos==null)
            {
                _pedidos = new List<PLAN_PEDIDO>();
                var ss = DLM.sap.RfcsSAP.ListarObras(true);
               foreach(var s in ss)
                {
                    PLAN_PEDIDO pp = new PLAN_PEDIDO(s);
                    _pedidos.Add(pp);
                }
            }
            return _pedidos;
        }
        public static List<PLAN_PEP> converter(List<PEPConsultaSAP> origem/*, bool consultar_existentes = false*/)
        {
            var PEPS_DUMP = origem.Select(x =>
            new PLAN_PEP()
            {
                //banco = Conexoes.DBases.GetDB(),

                PEP = x.Codigo,
                pep_engenharia = x.Engenharia.PEP.Codigo,
                peso_planejado = x.Peso_Planejado,
                peso_produzido = x.Peso_Produzido,
                peso_embarcado = x.Peso_Embarcado,
                status_eng = x.Engenharia.Status,
                engenharia_liberacao = x.Engenharia_Liberacao,
                engenharia_cronograma = x.Engenharia_Cronograma,
                engenharia_cronograma_inicio = x.Engenharia_Cronograma_Inicio,
                fabrica_cronograma = x.Fabrica_Cronograma,
                fabrica_cronograma_inicio = x.Fabrica_Cronograma_Inicio,
                logistica_cronograma = x.Logistica_Cronograma,
                logistica_cronograma_inicio = x.Logistica_Cronograma_Inicio,
                montagem_cronograma = x.Montagem_Cronograma,
                montagem_cronograma_inicio = x.Montagem_Cronograma_Inicio,

                eini = x.eng_base_ini,
                efim = x.eng_base_fim,
                fini = x.fab_base_ini,
                ffim = x.fab_base_fim,
                lini = x.log_base_ini,
                lfim = x.log_base_fim,
                mini = x.mon_base_ini,
                mfim = x.mon_base_fim,

                observacoes = x.Observacoes,
                status = x.Fabrica.Status,
                /*14/11/18*/

            }).ToList();
            List<PLAN_PEP> RETORNO = new List<PLAN_PEP>();
            RETORNO.AddRange(PEPS_DUMP);

            

            return RETORNO;
        }

        public static List<DLM.sapgui.Lancamento> Agrupar(List<DLM.sapgui.Lancamento> lancamentos, bool separar_por_tipo = true)
        {
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();
            if (separar_por_tipo)
            {
                var meses = lancamentos.GroupBy(x => x.Chave).Select(x => x.First()).ToList();

                foreach (var mes in meses)
                {
                    var subs = lancamentos.FindAll(x => x.Chave == mes.Chave).ToList();
                    retorno.Add(new DLM.sapgui.Lancamento(subs));
                }
            }
            else
            {
                var meses = lancamentos.GroupBy(x => x.data).Select(x => x.First()).ToList();

                foreach (var mes in meses)
                {
                    var subs = lancamentos.FindAll(x => x.data == mes.data).ToList();
                    retorno.Add(new DLM.sapgui.Lancamento(subs));
                }
            }

            return retorno.OrderBy(x => x.ToString()).ToList();
        }
    }
}
