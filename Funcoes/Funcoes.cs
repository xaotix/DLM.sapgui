using Conexoes;
using DLM.painel;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DLM.sapgui
{
   public partial class Funcoes
    {

        public static void RodaScript(List<string> Script)
        {

            if (Script.Count() > 0)
            {
                try
                {
                    var ARQ_TMP = $"{Cfg.Init.DIR_SCRIPTS}DLMScript_{Conexoes.Utilz.RandomString(2)}.vbs";
                    denovo:
                    if (ARQ_TMP.Exists())
                    {
                       if(!ARQ_TMP.Delete(false))
                        {
                            ARQ_TMP = $"{Cfg.Init.DIR_SCRIPTS}DLMScript_{Conexoes.Utilz.RandomString(2)}.vbs";
                            goto denovo;
                        }
                    }
                    
                    Conexoes.Utilz.Arquivo.Gravar(ARQ_TMP, Script.ToList());
                    var scriptProc = new Process();
                    scriptProc.StartInfo.FileName = ARQ_TMP;
                    scriptProc.StartInfo.WorkingDirectory = Utilz.getPasta(Cfg.Init.SAP_SCRIPT_IMPRESSAO_tmp); 

                    scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; 
                    scriptProc.Start();
                    scriptProc.WaitForExit(); 
                    scriptProc.Close();
                }
                catch (Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                    return;
                }
            }
        }
        public static List<CN47N> GetCronograma(string Pedido)
        {
            var Datas = new List<CN47N>();
            var t = DLM.sap.RfcsSAP.ConsultarPedido(Pedido);
            foreach (var s in t)
            {
                Datas.Add(new CN47N(s));
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
        public static List<PLAN_PEP> converter(List<PEP_Planejamento> origem/*, bool consultar_existentes = false*/)
        {
            var PEPS_DUMP = origem.Select(x =>
            new PLAN_PEP()
            {
                //banco = DBases.GetDBMySQL(),

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
