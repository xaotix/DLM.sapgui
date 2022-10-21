using DLM.vars;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DLM.painel
{
  public  class Vars
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
                if(_Obras == null)
                {
                    _Obras = new Conexoes.MSAP_Obras();
                }
                return _Obras;
            }
        }
        public static string bingMapsKey { get { return "AoG37OwYKMoSSFEvqYWHfYTlseohKaXdeMWCw51MJ7vMvqdBxhq61r24C7HS1Hw1"; } }
        public static class Imagens
        {
            private static ImageSource _embarque_32x32_vermelho { get; set; }
            public static ImageSource embarque_32x32_vermelho
            {
                get
                {
                    if (_embarque_32x32_vermelho == null)
                    {
                        _embarque_32x32_vermelho = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.embarque_32x32_vermelho);
                    }
                    return _embarque_32x32_vermelho;
                }
            }

            private static ImageSource _engenharia_32x32_vermelho { get; set; }
            public static ImageSource engenharia_32x32_vermelho
            {
                get
                {
                    if (_engenharia_32x32_vermelho == null)
                    {
                        _engenharia_32x32_vermelho = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.engenharia_32x32_vermelho);
                    }
                    return _engenharia_32x32_vermelho;
                }
            }


            private static ImageSource _fabrica_32x32_vermelho { get; set; }
            public static ImageSource fabrica_32x32_vermelho
            {
                get
                {
                    if (_fabrica_32x32_vermelho == null)
                    {
                        _fabrica_32x32_vermelho = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.fabrica_32x32_vermelho);
                    }
                    return _fabrica_32x32_vermelho;
                }
            }


            private static ImageSource _montagem_32x32_vermelho { get; set; }
            public static ImageSource montagem_32x32_vermelho
            {
                get
                {
                    if (_montagem_32x32_vermelho == null)
                    {
                        _montagem_32x32_vermelho = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_vermelho);
                    }
                    return _montagem_32x32_vermelho;
                }
            }

            private static ImageSource _montagem_32x32_trancada { get; set; }
            public static ImageSource montagem_32x32_trancada
            {
                get
                {
                    if (_montagem_32x32_trancada == null)
                    {
                        _montagem_32x32_trancada = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_trancada);
                    }
                    return _montagem_32x32_trancada;
                }
            }

            private static ImageSource _montagem_32x32_cinza { get; set; }
            public static ImageSource montagem_32x32_cinza
            {
                get
                {
                    if (_montagem_32x32_cinza == null)
                    {
                        _montagem_32x32_cinza = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_cinza);
                    }
                    return _montagem_32x32_cinza;
                }
            }

            private static ImageSource _embarque_32x32_verde { get; set; }
            public static ImageSource embarque_32x32_verde
            {
                get
                {
                    if (_embarque_32x32_verde == null)
                    {
                        _embarque_32x32_verde = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.embarque_32x32_verde);
                    }
                    return _embarque_32x32_verde;
                }
            }

            private static ImageSource _engenharia_32x32_verde { get; set; }
            public static ImageSource engenharia_32x32_verde
            {
                get
                {
                    if (_engenharia_32x32_verde == null)
                    {
                        _engenharia_32x32_verde = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.engenharia_32x32_verde);
                    }
                    return _engenharia_32x32_verde;
                }
            }


            private static ImageSource _fabrica_32x32_verde { get; set; }
            public static ImageSource fabrica_32x32_verde
            {
                get
                {
                    if (_fabrica_32x32_verde == null)
                    {
                        _fabrica_32x32_verde = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.fabrica_32x32_verde);
                    }
                    return _fabrica_32x32_verde;
                }
            }

            private static ImageSource _montagem_32x32_verde { get; set; }
            public static ImageSource montagem_32x32_verde
            {
                get
                {
                    if (_montagem_32x32_verde == null)
                    {
                        _montagem_32x32_verde = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_verde);
                    }
                    return _montagem_32x32_verde;
                }
            }




            private static ImageSource _embarque_32x32_laranja { get; set; }
            public static ImageSource embarque_32x32_laranja
            {
                get
                {
                    if (_embarque_32x32_laranja == null)
                    {
                        _embarque_32x32_laranja = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.embarque_32x32_laranja);
                    }
                    return _embarque_32x32_laranja;
                }
            }

            private static ImageSource _engenharia_32x32_laranja { get; set; }
            public static ImageSource engenharia_32x32_laranja
            {
                get
                {
                    if (_engenharia_32x32_laranja == null)
                    {
                        _engenharia_32x32_laranja = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.engenharia_32x32_laranja);
                    }
                    return _engenharia_32x32_laranja;
                }
            }


            private static ImageSource _fabrica_32x32_laranja { get; set; }
            public static ImageSource fabrica_32x32_laranja
            {
                get
                {
                    if (_fabrica_32x32_laranja == null)
                    {
                        _fabrica_32x32_laranja = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.fabrica_32x32_laranja);
                    }
                    return _fabrica_32x32_laranja;
                }
            }


            private static ImageSource _montagem_32x32_laranja { get; set; }
            public static ImageSource montagem_32x32_laranja
            {
                get
                {
                    if (_montagem_32x32_laranja == null)
                    {
                        _montagem_32x32_laranja = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_laranja);
                    }
                    return _montagem_32x32_laranja;
                }
            }






            private static ImageSource _embarque_32x32_azul { get; set; }
            public static ImageSource embarque_32x32_azul
            {
                get
                {
                    if (_embarque_32x32_azul == null)
                    {
                        _embarque_32x32_azul = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.embarque_32x32_azul);
                    }
                    return _embarque_32x32_azul;
                }
            }

            private static ImageSource _engenharia_32x32_azul { get; set; }
            public static ImageSource engenharia_32x32_azul
            {
                get
                {
                    if (_engenharia_32x32_azul == null)
                    {
                        _engenharia_32x32_azul = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.engenharia_32x32_azul);
                    }
                    return _engenharia_32x32_azul;
                }
            }


            private static ImageSource _fabrica_32x32_azul { get; set; }
            public static ImageSource fabrica_32x32_azul
            {
                get
                {
                    if (_fabrica_32x32_azul == null)
                    {
                        _fabrica_32x32_azul = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.fabrica_32x32_azul);
                    }
                    return _fabrica_32x32_azul;
                }
            }


            private static ImageSource _montagem_32x32_azul { get; set; }
            public static ImageSource montagem_32x32_azul
            {
                get
                {
                    if (_montagem_32x32_azul == null)
                    {
                        _montagem_32x32_azul = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32_azul);
                    }
                    return _montagem_32x32_azul;
                }
            }




            private static ImageSource _embarque_32x32 { get; set; }
            public static ImageSource embarque_32x32
            {
                get
                {
                    if (_embarque_32x32 == null)
                    {
                        _embarque_32x32 = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.embarque_32x32);
                    }
                    return _embarque_32x32;
                }
            }

            private static ImageSource _engenharia_32x32 { get; set; }
            public static ImageSource engenharia_32x32
            {
                get
                {
                    if (_engenharia_32x32 == null)
                    {
                        _engenharia_32x32 = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.engenharia_32x32);
                    }
                    return _engenharia_32x32;
                }
            }


            private static ImageSource _fabrica_32x32 { get; set; }
            public static ImageSource fabrica_32x32
            {
                get
                {
                    if (_fabrica_32x32 == null)
                    {
                        _fabrica_32x32 = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.fabrica_32x32);
                    }
                    return _fabrica_32x32;
                }
            }


            private static ImageSource _montagem_32x32 { get; set; }
            public static ImageSource montagem_32x32
            {
                get
                {
                    if (_montagem_32x32 == null)
                    {
                        _montagem_32x32 = Conexoes.Utilz.getImageSource(DLM.sapgui.Properties.Resources.montagem_32x32);
                    }
                    return _montagem_32x32;
                }
            }
        }

        public class Datas
        {
            public static int GetSemana(DateTime time)
            {
                DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
                if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                {
                    time = time.AddDays(3);
                }

                return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }

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
                int semana = GetSemana(Data);
                primeiro = GetPrimeiroDiaDaSemana(Data.Year, semana, CultureInfo.CurrentCulture);
                ultimo = primeiro.AddDays(7);
            }
        }

    }
}
