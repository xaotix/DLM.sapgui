using Conexoes;
using DLM.vars;
using System;
using System.Globalization;
using System.Windows.Media;

namespace DLM.painel
{
    public class Vars
    {

        public static string SetupUser = $"{Cfg.Init.DIR_APP}setup.user.cfg";
        public static string TEMPLATE_SAIDA = $"{Cfg.Init.DIR_APP}TEMPLATE_PLANEJAMENTO.xlsx";
        public static string TEMPLATE_DATAS_FABRICA = $"{Cfg.Init.DIR_APP}TEMPLATE_DATAS_FABRICA.xlsx";
        public static string TEMPLATE_AVANCO_FABRICA = $"{Cfg.Init.DIR_APP}template_avanco_fabrica.xlsx";

        public static string template_cronograma_resumo = $"{Cfg.Init.DIR_APP}template_cronograma_resumo.xlsx";
        public static string template_relatorio_economico = $"{Cfg.Init.DIR_APP}template_relatorio_economico.xlsx";
        public static string TEMPLATE_SAIDA_PECAS = $"{Cfg.Init.DIR_APP}TEMPLATE_PECAS.xlsx";
        public static string TEMPLATE_SAIDA_PECAS_RESUMO = $"{Cfg.Init.DIR_APP}template_pecas_resumo.xlsx";
        public static string TEMPLATE_EMBARQUES = $"{Cfg.Init.DIR_APP}template_embarques.xlsx";
        public static string TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA = $"{Cfg.Init.DIR_APP}template_pecas_resumo_consolidada.xlsx";







        public static string args { get; set; } = "";
        private static Conexoes.MSAP_Obras _Obras { get; set; }
        public static Conexoes.MSAP_Obras Obras
        {
            get
            {
                if (_Obras == null)
                {
                    _Obras = new Conexoes.MSAP_Obras();
                }
                return _Obras;
            }
        }
        public static string bingMapsKey { get { return "AoG37OwYKMoSSFEvqYWHfYTlseohKaXdeMWCw51MJ7vMvqdBxhq61r24C7HS1Hw1"; } }

        public class Datas
        {
            public static DateTime GetPrimeiroDiaDaSemana(int ano, int semana, System.Globalization.CultureInfo ci)
            {
                DateTime iniciojaneiro = new DateTime(ano, 1, 1);
                int offsetdias = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)iniciojaneiro.DayOfWeek;
                DateTime primeirodiadasemana = iniciojaneiro.AddDays(offsetdias);
                int firstWeek = ci.Calendar.GetWeekOfYear(iniciojaneiro, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
                if ((firstWeek <= 1 || firstWeek >= 52) && offsetdias >= -3)
                {
                    semana -= 1;
                }
                return primeirodiadasemana.AddDays(semana * 7);
            }

            public static void GetPrimeiroEUltimoDiaDaSemana(DateTime Data, out DateTime primeiro, out DateTime ultimo)
            {
                int semana = Data.Week();
                primeiro = GetPrimeiroDiaDaSemana(Data.Year, semana, CultureInfo.CurrentCulture);
                ultimo = primeiro.AddDays(7);
            }
        }

    }
}
