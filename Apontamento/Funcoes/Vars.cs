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
        public class Colunas
        {
            public class ESQUEMAS
            {
                public static int PINTURA = 1;
                public static int LOCAL = 2;
                public static int QTD_CARAC = 3;
                public static int DESCRICAO_ESQUEMA = 4;
                public static int CODIGO_ESQUEMA = 5;
                public static int COR_1_DM = 6;
                public static int TIPO_PIN_1_DM = 7;
                public static int ID_TINTA = 8;
                public static int COD_1_DM = 9;
                public static int DILUENTE_1_DM = 10;
                public static int DESCR_DILUENTE_1_DM = 11;
                public static int MICRAS_1_DM = 12;
                public static int COR_2_DM = 13;
                public static int TIPO_PIN_2_DM = 14;
                public static int ID_TINTA_2 = 15;
                public static int COD_2_DM = 16;
                public static int DILUENTE_2_DM = 17;
                public static int DESCR_DILUENTE_2_DM = 18;
                public static int MICRAS_2_DM = 19;
                public static int COR_3_DM = 20;
                public static int TIPO_PIN_3_DM = 21;
                public static int ID_TINTA_3 = 22;
                public static int COD_3_DM = 23;
                public static int DILUENTE_3_DM = 24;
                public static int DESCR_DILUENTE_3_DM = 25;
                public static int MICRAS_3_DM = 26;
                public static int TOTAL_COLUNAS = 26;
            }
        }
        public static string Raiz = System.Windows.Forms.Application.StartupPath + @"\";
        public static string SetupUser = Raiz + "setup.user.cfg";
        public static string TEMPLATE_SAIDA = Raiz + "TEMPLATE_PLANEJAMENTO.xlsx";
        public static string TEMPLATE_DATAS_FABRICA = Raiz + "TEMPLATE_DATAS_FABRICA.xlsx";
        public static string TEMPLATE_AVANCO_FABRICA = Raiz + "template_avanco_fabrica.xlsx";
        
        public static string template_cronograma_resumo = Raiz + "template_cronograma_resumo.xlsx";
        public static string template_relatorio_economico = Raiz + "template_relatorio_economico.xlsx";
        public static string TEMPLATE_SAIDA_PECAS = Raiz + "TEMPLATE_PECAS.xlsx";
        public static string TEMPLATE_SAIDA_PECAS_RESUMO = Raiz + "template_pecas_resumo.xlsx";
        public static string TEMPLATE_EMBARQUES = Raiz + "template_embarques.xlsx";
        public static string TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA = Raiz + "template_pecas_resumo_consolidada.xlsx";



        private  static List<Descricao_PEP> _Descricoes_PEP { get; set; }
        public static List<Descricao_PEP> Descricoes_PEP { get
            {
                if(_Descricoes_PEP==null)
                {
                    _Descricoes_PEP = new List<Descricao_PEP>();
                    var lista_log = Conexoes.DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_de_para_orc_pla);
                    foreach(var s in lista_log.Linhas)
                    {
                        _Descricoes_PEP.Add(new Descricao_PEP(s));
                    }
                }
                return _Descricoes_PEP;
            }
        }


        public static string args { get; set; } = "";
        public static string SAP_PDF1 = @"\\PAVMSFS04\SAP_PDF_Desenhos\";
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
