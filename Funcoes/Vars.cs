using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    public class Vars
    {

        public static int PEDIDO_TAM { get; set; } = 13;
        public static string ZSD0031NARQ { get; set; } = "ZSD0031.xml";
        public static string ZPMPARQ { get; set; } = "ZPMP.xlsx";
        public static string ZPPCOOISNARQ { get; set; } = "ZPPCOOISN.xlsx";
        public static string ZCONTRATOSARQ { get; set; } = "ZCONTRATOS.xlsx";
        public static string ZPP0100ARQ { get; set; } = "ZPP0100.xlsx";
        public static string CJI3ARQ { get; set; } = "CJI3.xlsx";
        public static string ZPP0112ARQ { get; set; } = "ZPP0112.xlsx";
        public static string FAGLL03ARQ { get; set; } = "FAGLL03.xlsx";
        public static string ZPP066NARQ { get; set; } = "ZPP0066N.xlsx";
        public static string CN47NARQ { get; set; } = "CN47N.xlsx";
        public static string Raiz { get; set; } = System.Windows.Forms.Application.StartupPath + @"\";
        public static string SetupUser { get; set; } = Raiz + "setup.user.cfg";
        public static string SCRIPT_IMPRESSAO { get; set; } = Raiz + "IMPRIMIR.VBS";
        public static string SCRIPT_IMPRESSAO_FAGLL03 { get; set; } = Raiz + "IMPRIMIR_FAGLL03.VBS";
        public static string SCRIPT_IMPRESSAO2 { get; set; } = Raiz + "IMPRIMIR2.VBS";
        public static string SCRIPT_IMPRESSAO_ZPMP { get; set; } = Raiz + "IMPRIMIR_ZPMP.VBS";
        public static string SCRIPT_IMPRESSAO_tmp { get; set; } = Raiz + "Script1.vbs";
        public static string TEMPLATE_SAIDA { get; set; } = Raiz + "TEMPLATE_PLANEJAMENTO.xlsx";

    }
}
