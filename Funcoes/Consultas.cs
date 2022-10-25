using Conexoes;
using DLM.vars;
using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DLM.sapgui
{
    //created a class for the SAP app, connection, and session objects as well as for common methods. 
    public class Consulta
    {
        public GuiSession NovaJanela()
        {
            //session.findById("wnd[0]/tbar[0]/okcd").text = "/o"
            //session.findById("wnd[0]").sendVKey 0
            Enter();
            //session.findById("wnd[1]/tbar[0]/btn[5]").press
            ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[5]")).Press();

            var secoes = GetSecoes();
            if(secoes.Count>0)
            {
                return secoes.Last();
            }
            return null;
        }
        public void Maximizar()
        {
            var jan = GetJanela();
            if(jan!=null)
            {
                jan.Maximize();
            }
        }
        public string TituloJanela()
        {
            var s = GetJanela();
            if(s!=null)
            {
                return s.Text;
            }
            return "";
        }
        public GuiFrameWindow GetJanela()
        {
            if (!this.Carregar_sap()) { return null; }
            try
            {
                return ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]"));
            }
            catch (Exception)
            {
            }
            return null;
        }
        public static bool Autologin { get; set; } = false;
        public GuiSession SessaoSAP
        {
            get
            {
                var secoes = GetSecoes();
                if (secoes.Count > 0)
                {
                    return secoes.Last();
                }
                return null;
            }
        }
        public static GuiApplication SapGuiApp { get; set; }
        public static GuiConnection SapConnection { get; set; }
        private void openSap(string env)
        {
            try
            {
                DLM.sapgui.Consulta.SapGuiApp = new GuiApplication();


                string connectString = null;
                if (env.ToUpper().Equals("DEFAULT"))
                {
                    connectString = "1.0 Test ERP (DEFAULT)";
                }
                else
                {
                    connectString = env;
                }
                DLM.sapgui.Consulta.SapConnection = DLM.sapgui.Consulta.SapGuiApp.OpenConnection(connectString, Sync: true); //creates connection
                //this.SessaoSAP = (GuiSession)SAPGUI_Medabil.Consulta.SapConnection.Sessions.Item(0); //creates the Gui session off the connection you made
            }
            catch (Exception)
            {

            }

        }

        private void Login(string myclient = "800", string mylogin = "ma1516", string mypass = "", string mylang = "PT")
        {
            GuiTextField client = (GuiTextField)this.SessaoSAP.ActiveWindow.FindByName("RSYST-MANDT", "GuiTextField");
            GuiTextField login = (GuiTextField)this.SessaoSAP.ActiveWindow.FindByName("RSYST-BNAME", "GuiTextField");
            GuiTextField pass = (GuiTextField)this.SessaoSAP.ActiveWindow.FindByName("RSYST-BCODE", "GuiPasswordField");
            GuiTextField language = (GuiTextField)this.SessaoSAP.ActiveWindow.FindByName("RSYST-LANGU", "GuiTextField");

            client.SetFocus();
            client.Text = myclient;
            login.SetFocus();
            login.Text = mylogin;
            pass.SetFocus();
            pass.Text = mypass;
            language.SetFocus();
            language.Text = mylang;

            //Press the green checkmark button which is about the same as the enter key 
            GuiButton btn = (GuiButton)SessaoSAP.FindById("/app/con[0]/ses[0]/wnd[0]/tbar[0]/btn[0]");
            btn.SetFocus();
            btn.Press();

        }
        public void Logar(string ma = "", string senha = "")

        {
            Conexoes.Utilz.Matar("saplogon.exe");
            Conexoes.Utilz.Matar("ConsultaAvanco.exe");
            Conexoes.Utilz.Matar("saplogon");
            this.openSap("PRODUCAO");
            this.Login("800", ma, senha, "PT");
        }
        
        /*ESSE CARA TENTA ABRIR UMA INSTÂNCIA PENDURADA NA GUI DO SAP*/
        public bool Carregar_sap()
        {
            try
            {
                if(SessaoSAP!=null)
                {
                   if (SessaoSAP.IsActive)
                    {
                    return true;
                    }
                }
                if(DLM.sapgui.Consulta.Autologin)
                {
                Logar();
                }
                //if(SessaoSAP==null)
                //{
                //    var secoes = GetSecoes();
                //    if(secoes.Count>0)
                //    {
                //        SessaoSAP = secoes.Last();
                //    }
                //}


                return SessaoSAP != null;
            }
            catch (Exception )
            {

                return false;  
            }


        }
        public List<GuiSession> GetSecoes()
        {
            List<GuiSession> retorno = new List<GuiSession>();
            var s = GetConexao();
            if(s==null)
            {
                return new List<GuiSession>();
            }
            foreach(GuiSession gui in s.Sessions)
            {
                retorno.Add(gui);
            }

            return retorno;
        }
        private GuiConnection GetConexao()
        {
            try
            {
                SapROTWr.CSapROTWrapper sapROT = new SapROTWr.CSapROTWrapper();
                object objSapGui = sapROT.GetROTEntry("SAPGUI");

                object objEngine = objSapGui.GetType().InvokeMember("GetScriptingEngine", System.Reflection.BindingFlags.InvokeMethod, null, objSapGui, null);

                for (int x = 0; x < (objEngine as GuiApplication).Children.Count; x++)
                {
                    GuiConnection sapConnection = ((objEngine as GuiApplication).Children.ElementAt(x) as GuiConnection);
                    return sapConnection;
                }
            }
            catch (Exception)
            {

            }


            return null;
        }

        /*ESSE CARA DÁ O CRONOGRAMA*/
        public bool CN47N(string Pedido, string destino, string ARQUIVO, bool msgs = false)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("CN47N");
                    Retornar();

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtCN_PROJN-LOW")).Text = Pedido + "*";

                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_DISVAR")).Text = @"/SISTEMA"; //ANTERIOR = ATAPLAN
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();
                    //((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO);
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/cntlALVCONTAINER/shellcont/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }
                    this.SessaoSAP.EndTransaction();

                    return File.Exists(destino + ARQUIVO);


                }
                else
                {
                    if (!msgs)
                    {
                        MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!msgs)
                {
                    MessageBox.Show("Não foi possível criar o arquivo\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                return false;
            }

        }

        /*ABRE O PROJETO*/
        public GuiTree SetNo(string num)
        {
            

            return null;
        }
        public void Esc()
        {
            try
            {
                ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).SendVKey(12);

            }
            catch (Exception)
            {
            }
        }
        public void Enter()
        {
            try
            {
                ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).SendVKey(00);

            }
            catch (Exception)
            {
            }
        }
        public void SendVKey(int key)
        {
            try
            {
                ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).SendVKey(key);

            }
            catch (Exception)
            {
            }
        }
        public void Sair()
        {
            //session.findById("wnd[0]/tbar[0]/btn[15]").press
            try
            {
                ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[0]/btn[15]")).Press();
            }
            catch (Exception)
            {

            }

        }
        public CJ20N_No CJ20NGetProjeto(string Projeto,bool esconder_erros = false)
        {
           var w = Conexoes.Utilz.Wait(5, "Abrindo projeto...." + Projeto);
            this.CJ20N_Sair(false);
            w.somaProgresso();
            try
            {

                if (this.Carregar_sap())
                {
                    // session.findById("wnd[0]/tbar[0]/okcd").text = "cj20n"
                    this.SessaoSAP.StartTransaction("CJ20N");
                    // session.findById("wnd[0]").sendVKey 0
                    ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).SendVKey(00);
                    // session.findById("wnd[0]/mbar/menu[0]/menu[1]").select
                    ((GuiMenu)this.SessaoSAP.FindById("wnd[0]/mbar/menu[0]/menu[1]")).Select();
                    // session.findById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-PROJ_EXT").text = ""
                    //definição do projeto
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-PROJ_EXT")).Text = Projeto;
                    // session.findById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-PRPS_EXT").text = "20-103941.p00.001"
                    //elemento pep
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-PRPS_EXT")).Text = "";
                    // session.findById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-AUFNR").text = ""
                    //diagrama de rede
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtCNPB_W_ADD_OBJ_DYN-AUFNR")).Text = "";
                    // session.findById("wnd[1]").sendVKey 0
                    ((GuiFrameWindow)this.SessaoSAP.FindById("wnd[1]")).SendVKey(00);

                    //session.findById("wnd[0]").maximize
                    //((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).Maximize();

                    try
                    {
                        //habilita a edição
                        // session.findById("wnd[0]/tbar[1]/btn[13]").press
                        ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[13]")).Press();

                        var s = ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3990/txtPROJ-POST1"));
                        var ts = s.Text;
                        s.Text = "TESTE";
                        s.Text = ts;

                    }
                    catch (Exception)
                    {
                        //manda o shift + f1 pra ver se consegue
                        SendVKey(13);
                        try
                        {
                            var s = ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3990/txtPROJ-POST1"));
                            var ts = s.Text;
                            s.Text = "TESTE";
                            s.Text = ts;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Não foi possível habilitar a edição do projeto. Depois que terminar de carregar lembre-se de ativar no SAP.\nNão feche a janela do programa.");
                        }
                    }




                    try
                    {
                        //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").headerContextMenu "TECH_KEY"
                        ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell")).HeaderContextMenu("TECH_KEY");

                        //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectContextMenuItem "HIER_COL_KEY"
                        // HIER_COL_TEXT = identificação / denominação
                        // HIER_COL_KEY = denominação / identificação
                        ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell")).SelectContextMenuItem("HIER_COL_TEXT");
                    }
                    catch (Exception)
                    {

                    }



                    w.somaProgresso();
                    string key = "000002";
                    CJ20N_No Raiz = GetRaiz(key);

                    w.Close();
                    return Raiz;

                }
                else
                {
                    if (!esconder_erros)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    w.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (!esconder_erros)
                {
                    MessageBox.Show("Não foi possível consultar o projeto\n" + ex.Message + "\n" + ex.StackTrace, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                w.Close();
                return null;
            }
        }

        public void Desbloqueia_Secao()
        {
            this.SessaoSAP.UnlockSessionUI();
        }

        public void Bloquear_Secao()
        {
            this.SessaoSAP.LockSessionUI();
        }

        public CJ20N_No GetRaiz(string key)
        {
            GuiTree tt = null;

            try
            {
                tt = ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell"));

                // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").topNode = "000002"
                tt.TopNode = key;
                // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectedNode = "000002"
                tt.SelectedNode = key;
                // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").expandNode "000002"
                //tt.ExpandNode(key);
                //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").nodeContextMenu "000002"
                tt.NodeContextMenu(key);

                string pep = "";
                string desc = "";
                string chave_pep = "";

                desc = tt.GetNodeTextByKey(key);
                pep = tt.GetItemText(key, "TECH_KEY");

                CJ20N_No Raiz = new CJ20N_No(pep, desc, key, chave_pep, tt, this);
                if (Raiz.tipo == CJ20N_Tipo.Desconhecido | Raiz.tipo == CJ20N_Tipo.Tarefa)
                {
                    try
                    {
                        Raiz.chave_pep = ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-PROJN")).Text;

                    }
                    catch (Exception)
                    {

                    }
                }

                return Raiz;

            }
            catch (Exception)
            {

            }

            return null;

        }

        public void CJ20N_Explode_Arvore()
        {
            try
            {
                //tive que fazer essa gambiarra pq não dava pra explodir por C#
                List<string> lista = new List<string>();
                lista.Add("If Not IsObject(application) Then");
                lista.Add("   Set SapGuiAuto  = GetObject(\"SAPGUI\")");
                lista.Add("   Set application = SapGuiAuto.GetScriptingEngine");
                lista.Add("End If");
                lista.Add("If Not IsObject(connection) Then");
                lista.Add("   Set connection = application.Children(0)");
                lista.Add("End If");
                lista.Add("If Not IsObject(session) Then");
                lista.Add("   Set session    = connection.Children(0)");
                lista.Add("End If");
                lista.Add("If IsObject(WScript) Then");
                lista.Add("   WScript.ConnectObject session,     \"on\"");
                lista.Add("   WScript.ConnectObject application, \"on\"");
                lista.Add("End If");
                // lista.Add("session.findById(\"wnd[0]\").resizeWorkingPane 189,33,false");
                lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell\").selectedNode = \"000002\"");
                lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell\").pressButton \"EBLM\"");
                lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell\").pressButton \"ABLM\"");

                Funcoes.RodaScript(lista, Cfg.Init.DIR_APPDATA + @"\Script_explode_arvore.vbs");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectedNode = "000002"
                //var dsd = this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell").pressButton "EBLM"
                //((GuiToolbarControl)this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell")).PressButton("EBLM");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell").pressButton "ABLM"
                //((GuiToolbarControl)this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell")).PressButton("ABLM");
            }
            catch (Exception)
            {


            }
        }

        public void CJ20N_Collapse_Arvore()
        {
            try
            {
                //tive que fazer essa gambiarra pq não dava pra explodir por C#
                List<string> lista = new List<string>();
                lista.Add("If Not IsObject(application) Then");
                lista.Add("   Set SapGuiAuto  = GetObject(\"SAPGUI\")");
                lista.Add("   Set application = SapGuiAuto.GetScriptingEngine");
                lista.Add("End If");
                lista.Add("If Not IsObject(connection) Then");
                lista.Add("   Set connection = application.Children(0)");
                lista.Add("End If");
                lista.Add("If Not IsObject(session) Then");
                lista.Add("   Set session    = connection.Children(0)");
                lista.Add("End If");
                lista.Add("If IsObject(WScript) Then");
                lista.Add("   WScript.ConnectObject session,     \"on\"");
                lista.Add("   WScript.ConnectObject application, \"on\"");
                lista.Add("End If");
                // lista.Add("session.findById(\"wnd[0]\").resizeWorkingPane 189,33,false");
                lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell\").selectedNode = \"000002\"");
                //lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell\").pressButton \"EBLM\"");
                lista.Add("session.findById(\"wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell\").pressButton \"ABLM\"");

                Funcoes.RodaScript(lista, Cfg.Init.DIR_APPDATA + @"\Script_explode_arvore.vbs");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectedNode = "000002"
                //var dsd = this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell").pressButton "EBLM"
                //((GuiToolbarControl)this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell")).PressButton("EBLM");
                ////session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell").pressButton "ABLM"
                //((GuiToolbarControl)this.SessaoSAP.FindById("wwnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[0]/shell")).PressButton("ABLM");
            }
            catch (Exception)
            {


            }
        }

        public bool CJ20N_Sair(bool salvar,bool esconder_erros = true)
        {
            if(salvar)
            {
                CJ20N_Salvar(false);
            }
            try
            {

                if (this.Carregar_sap())
                {

                    // session.findById("wnd[0]/mbar/menu[0]/menu[7]").select
                    ((GuiMenu)this.SessaoSAP.FindById("wnd[0]/mbar/menu[0]/menu[7]")).Select();

                    try
                    {
                        //clica em não se precisar
                    //session.findById("wnd[1]/usr/btnSPOP-OPTION2").press
                        ((GuiButton)this.SessaoSAP.FindById("wnd[1]/usr/btnSPOP-OPTION2")).Press();

                    }
                    catch (Exception)
                    {

                    }

                    return true;


                }
                else
                {
                    if (!esconder_erros)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!esconder_erros)
                {
                    MessageBox.Show("Não foi possível consultar o projeto\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                return false;
            }

        }
        public bool CJ20N_Salvar(bool esconder_erros = false)
        {

            try
            {

                if (this.Carregar_sap())
                {

                    //'salvar
                    //session.findById("wnd[0]/tbar[0]/btn[11]").press
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[0]/btn[11]")).Press();
                    //'esse cara dá pau se nao tem edição
                    //session.findById("wnd[1]/tbar[0]/btn[0]").press
                    try
                    {
                        var s = ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]"));
                        if (s != null)
                        {
                            s.Press();
                            //session.findById("wnd[1]/usr/btnSPOP-OPTION1").press
                            var s1 = ((GuiButton)this.SessaoSAP.FindById("wnd[1]/usr/btnSPOP-OPTION1"));
                            if (s1 != null)
                            {
                                s1.Press();
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                 



                    return true;


                }
                else
                {
                    if (!esconder_erros)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!esconder_erros)
                {
                    MessageBox.Show("Não foi possível consultar o projeto\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                return false;
            }

        }
        public bool CJ20N_Apagar(CJ20N_No no)
        {
           var tt = no.SetNo();
            if(tt!=null)
            {
                //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectContextMenuItem "DELE"
                ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell")).SelectContextMenuItem("DELE");
                //session.findById("wnd[1]/usr/btnSPOP-OPTION1").press
                ((GuiButton)this.SessaoSAP.FindById("wnd[1]/usr/btnSPOP-OPTION1")).Press();
            }
            //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").nodeContextMenu "000030"




            //Esc();

            return true;
        }
        public bool CJ20N_CriarTarefa(string pep,string descricao, string centro_de_trabalho = "1202", string divisao = "1202", string centro = "1202",  bool esconder_erros = false, string planejador_mrp = "PS0")
        {

            try
            {

                if (this.Carregar_sap())
                {

                    //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectContextMenuItem "CREATE_ACT_W"
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell")).SelectContextMenuItem("CREATE_ACT_W");


                    try
                    {
                        //session.findById("wnd[1]/usr/sub:SAPLSPO4:0300/ctxtSVALD-VALUE[0,21]").text = "PS0"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/sub:SAPLSPO4:0300/ctxtSVALD-VALUE[0,21]")).Text = planejador_mrp;
                        //session.findById("wnd[1]/tbar[0]/btn[0]").press
                        ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();
                    }
                    catch (Exception)
                    {
                    }

                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD").select
                    ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD")).Select();


                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-PROJN").text = "20-103941.P00.222.20A.F2"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-PROJN")).Text = pep;

                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCONW:0110/txtAFVGM-LTXA1").text = "Det. de Chumb."
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCONW:0110/txtAFVGM-LTXA1")).Text = descricao;
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-WERKS").text = "1202"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-WERKS")).Text = centro_de_trabalho;
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD").select
                    ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD")).Select();
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-GSBER").text = "1202"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-GSBER")).Text = divisao;
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-WERKS").text = "1202"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-WERKS")).Text = centro;
                    //session.findById("wnd[0]").sendVKey 0
                    Enter();
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD").select
                    ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD")).Select();



                    return true;


                }
                else
                {
                    if (!esconder_erros)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!esconder_erros)
                {
                    MessageBox.Show("Não foi possível consultar o projeto\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Esc();

                }
                return false;
            }

        }
        public bool CJ20N_EditarPEP(CJ20N_No no,
            string novo_nome = null, string nova_descricao = null, string centro =null, string divisao = null, bool atualizar_tarefa = true, bool escondermsgs = false)
        {

            try
            {

                if (this.Carregar_sap())
                {
                    //tem que pegar os filhos antes de renomear, se não ele perde o vínculo
                    var filhos = no.Getfilhos();
                    
                    var tt = no.SetNo();

                    if (tt == null) { return false; }

                   
                    if(novo_nome!=null)
                    {
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/ctxtPRPS-POSID").text = "10-104159.P00.004.25A.F4"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/ctxtPRPS-POSID")).Text = novo_nome;
                    }
     
                    if(nova_descricao!=null)
                    {
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/txtPRPS-POST1").text = "Fáb. Medabar em CHA"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/txtPRPS-POST1")).Text = nova_descricao;
                    }

                    try
                    {
                        //Aba atribuições
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA").select
                        ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA")).Select();




                        if(centro!=null)
                        {
                            try
                            {
                                //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-WERKS").text = "1204"
                                ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-WERKS")).Text = centro;

                                try
                                {
                                    // CP - NB - SR (CALENDÁRIO FÁBRICA)
                                    string cal = "NB";
                                    if (centro == Cfg.Init.CENTRO_SER)
                                    {
                                        cal = "SR";
                                    }
                                    else if (centro == Cfg.Init.CENTRO_CHA)
                                    {
                                        cal = "CP";
                                    }
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-FABKL").text = "CP"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-FABKL")).Text = cal;

                                }
                                catch (Exception)
                                {

                                }
                            }
                            catch (Exception)
                            {

                            }
                        }


                        if (divisao != null)
                        {
                            try
                            {
                                //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PGSBR").text = "1204"
                                ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PGSBR")).Text = divisao;

                            }
                            catch (Exception)
                            {

                            }
                        }

                       

                        //session.findById("wnd[0]").sendVKey 0
                        Enter();

                        if(atualizar_tarefa)
                        {
                            //tarefa
                            foreach (var f in filhos)
                            {

                                var tr = f.SetNo();
                                if (tr == null)
                                {
                                    continue;
                                }
                                try
                                {
                                    if (nova_descricao != null)
                                    {
                                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCONW:0110/txtAFVGM-LTXA1").text = "Fáb. Medabar em CHA"
                                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCONW:0110/txtAFVGM-LTXA1")).Text = nova_descricao;
                                    }

                                    if (centro != null)
                                    {
                                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-WERKS").text = "1204"
                                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-WERKS")).Text = centro;
                                    }

                                }
                                catch (Exception)
                                {

                                }

                                if(novo_nome!=null)
                                {
                                    try
                                    {

                                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-PROJN")).Text = novo_nome;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }



                                try
                                {
                                    //Aba atribuições da tarefa
                                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD").select
                                    ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD")).Select();

                                    if (divisao != null)
                                    {
                                        try
                                        {
                                            //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-GSBER").text = "1204"
                                            ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-GSBER")).Text = divisao;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }

                                    if (centro != null)
                                    {
                                        try
                                        {
                                            //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-WERKS").text = "1204"
                                            ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpZUOD/ssubSUBSCR_1000:SAPLCONW:1314/ctxtAFVGD-WERKS")).Text = centro;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }

                                }
                                catch (Exception)
                                {

                                }



                                //session.findById("wnd[0]").sendVKey 0
                            }
                        }

  






                    }
                    catch (Exception)
                    {

                    }


                    Enter();
                    Esc();
                    return true;


                }
                else
                {
                    if (!escondermsgs)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!escondermsgs)
                {
                    MessageBox.Show("Não foi possível consultar o projeto\n" + ex.Message + "\n" + ex.StackTrace, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                Esc();

                return false;
            }

        }
        public bool CJ20N_CriarEtapa(string nome, string descricao, bool planejado = true, bool contabil = true, bool faturamento = false, string empresa = "1100", string centro = "1104", string divisao = "1104", bool msgs = false)
        {

            try
            {

                if (this.Carregar_sap())
                {
                    //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectContextMenuItem "CREATE_WBS"
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell")).SelectContextMenuItem("CREATE_WBS");
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/ctxtPRPS-POSID").text = "20-103941.P00.022"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/ctxtPRPS-POSID")).Text = nome;
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/txtPRPS-POST1").text = "Desc. da Etapa"
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subIDENTIFICATION:SAPLCJWB:3991/txtPRPS-POST1")).Text = descricao;
                    Enter();


                    try
                    {
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-PLAKZ").selected = true
                        ((SAPFEWSELib.GuiCheckBox)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-PLAKZ")).Selected = planejado;
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-BELKZ").selected = false
                        ((SAPFEWSELib.GuiCheckBox)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-BELKZ")).Selected = contabil;
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-FAKKZ").selected = false
                        ((SAPFEWSELib.GuiCheckBox)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND/ssubSUBSCR1:SAPLCJWB:1210/chkPRPS-FAKKZ")).Selected = faturamento;
                    }
                    catch (Exception)
                    {

                    }



                    try
                    {
                        //Aba atribuições
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA").select
                        ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA")).Select();
                        try
                        {
                            //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PBUKR").text = "1000"
                            ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PBUKR")).Text = empresa;
                        }
                        catch (Exception)
                        {

                        }


                        if (centro != null)
                        {
                            try
                            {
                                //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-WERKS").text = "1204"
                                ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-WERKS")).Text = centro;

                                try
                                {
                                    // CP - NB - SR (CALENDÁRIO FÁBRICA)
                                    string cal = "NB";
                                    if (centro == Cfg.Init.CENTRO_SER)
                                    {
                                        cal = "SR";
                                    }
                                    else if (centro == Cfg.Init.CENTRO_CHA)
                                    {
                                        cal = "CP";
                                    }
                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-FABKL").text = "CP"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-FABKL")).Text = cal;

                                }
                                catch (Exception)
                                {

                                }
                            }
                            catch (Exception)
                            {

                            }
                        }


    

                        //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PGSBR").text = "1002"
                        ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpORGA/ssubSUBSCR1:SAPLCJWB:1410/ctxtPRPS-PGSBR")).Text = divisao;
                    }
                    catch (Exception)
                    {

                    }

                    //aba Dados básicos
                    //session.findById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND").select
                    ((SAPFEWSELib.GuiTab)this.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCJWB:3999/tabsTABCJWB/tabpGRND")).Select();

                    //'manda o enter
                    //session.findById("wnd[0]").sendVKey 0
                    //'manda o esc
                    //session.findById("wnd[0]").sendVKey 12

                    Enter();
                    Esc();
                    return true;


                }
                else
                {
                    if (msgs)
                    {
                        MessageBox.Show("Não foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                Retornar();
                return false;
            }

        }
        public string GetTipo(string componente)
        {
           var s = this.SessaoSAP.FindById(componente);
            if(s!=null)
            {
                return s.Type.ToString();
            }
            return "";
        }

        /*ESSE CARA DÁ A FOLHA MARGEM*/
        public bool ZSD0031N(string Pedido, string destino, string ARQUIVO, bool msgs = false)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("zsd0031n");
                    Retornar();

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_PSP-LOW")).Text = Pedido;
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();

                    /*EXCEL*/
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[5]")).Press();
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = ARQUIVO;
                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[11]")).Press();

                    if (!File.Exists(destino + ARQUIVO))
                    {
                        ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO);
                        if (!File.Exists(destino + ARQUIVO))
                        {
                            var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/cntlALVCONTAINER/shellcont/shell", false);
                            ExportaExcelNativo(destino, ARQUIVO, ctrl);
                        }
                    }

                    this.SessaoSAP.EndTransaction();
                    //this.SessaoSAP.ActiveWindow.Close();

                    return File.Exists(destino + ARQUIVO);


                }
                else
                {
                    if (!msgs)
                    {
                        MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                               if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                return false;
            }

        }
        /*ESSE CARA DÁ O AVANÇO DE LOGÍSTICA*/
        public bool ZPP0066N(string Pedido, string destino, string ARQUIVO, bool msgs=false)
        {

            try
            {

                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPP0066N");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/txtS_PACK-LOW")).Text = Pedido + "*";

                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_LAYOUT")).Text = @"/SALDO_PT";
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[0]/btn[0]")).Press();
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO2);
                    if(!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell/shellcont/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();

                    return File.Exists(destino + ARQUIVO);


                }
                else
                {
                    if(msgs)
                    {

                    MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                return false;
            }

        }
        /*ESSE CARA DÁ O AVANÇO DE LOGÍSTICA COM AS NOTAS FISCAIS*/
        public bool ZPP0066N_SemPerfil(string Pedido, string destino, string ARQUIVO, bool msgs = false)
        {

            try
            {

                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPP0066N");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/txtS_PACK-LOW")).Text = Pedido + "*";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_LAYOUT")).Text = @"/PAINEL";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_LAYOUT")).Text = "";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_LAYOUT")).Text = @"/SISTEMA";
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[0]/btn[0]")).Press();
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO2);
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell/shellcont/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();

                    return File.Exists(destino + ARQUIVO);


                }
                else
                {
                    if (msgs)
                    {

                        MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }

                return false;
            }

        }
        /*ESSE CARA DÁ OS DADOS DO EMBARQUE ZPP0112*/
        public bool ZPP0112(string destino, string ARQUIVO, long min, long max)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPP0112");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/txtS_CARGA-LOW")).Text = min.ToString();
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/txtS_CARGA-HIGH")).Text = max.ToString();


                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO2);

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    //MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                //MessageBox.Show("Não foi possível criar o arquivo\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }

        }
        /*ESSE CARA DÁ A MOVIMENTAÇÃO DE TINTAS DO ALMOX PARA A FÁBRICA*/
        public DLM.db.Tabela MB51(DateTime de, DateTime ate)
        {

            DLM.db.Tabela retorno = new db.Tabela();
            DLM.painel.Consultas.MatarExcel(false);

            var dt_de = $"{de.Day}.{de.Month}.{de.Year}";
            var dt_ate = $"{ate.Day}.{ate.Month}.{ate.Year}";
            string nome = $"MB51_{dt_de}_a_{dt_ate}.xlsx";
            string arquivo = Cfg.Init.GetDestinoSAP_Excel() + nome;
            string destino = Conexoes.Utilz.getPasta(arquivo);

            if (arquivo.Existe())
            {
               if(!arquivo.Apagar())
                {
                    return retorno;
                }
            }
            if (this.Carregar_sap())
            {
                this.Bloquear_Secao();
                try
                {

                    this.SessaoSAP.StartTransaction("MB51");

                    /*
                       session.findById("wnd[0]/usr/ctxtWERKS-LOW").text = "1202"
                       session.findById("wnd[0]/usr/ctxtLGORT-LOW").text = "0010"
                       session.findById("wnd[0]/usr/ctxtBWART-LOW").text = "311"
                       session.findById("wnd[0]/usr/ctxtBUDAT-LOW").text = "01.07.2022"
                       session.findById("wnd[0]/usr/ctxtBUDAT-HIGH").text = "31.07.2022"
                       session.findById("wnd[0]/tbar[1]/btn[8]").press
                       session.findById("wnd[0]/tbar[1]/btn[48]").press
                     */
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtWERKS-LOW")).Text = Cfg.Init.CENTRO_NOB;
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtLGORT-LOW")).Text = "0010";
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtBWART-LOW")).Text = "311";
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtBUDAT-LOW")).Text = dt_de;
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtBUDAT-HIGH")).Text = dt_ate;



                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[48]")).Press();


                    /*
                     //Impressão
                     session.findById("wnd[0]/usr/cntlGRID1/shellcont/shell").contextMenu
                     session.findById("wnd[0]/usr/cntlGRID1/shellcont/shell").selectContextMenuItem "&XXL"
                     session.findById("wnd[1]/tbar[0]/btn[0]").press

                     session.findById("wnd[1]/usr/ctxtDY_PATH").text = "N:\000 - Dev. Software\2022\2022.08.01 - Trabalho Superfície Pintura\"
                     session.findById("wnd[1]/usr/ctxtDY_FILENAME").text = "mb51_2.xlsx"
                     session.findById("wnd[1]/tbar[0]/btn[0]").press
                     */

                    var ctrl = this.SessaoSAP.FindById("wnd[0]/usr/cntlGRID1/shellcont/shell");
                    var shellToolbarContextButton = ((GuiShell)ctrl);
                    var btnToolbarContextButton = shellToolbarContextButton as GuiGridView;
                    btnToolbarContextButton?.ContextMenu();
                    btnToolbarContextButton?.SelectContextMenuItem("&XXL");
                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();


                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = nome;
                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();

                    this.SessaoSAP.EndTransaction();

                    if (arquivo.Existe())
                    {
                        return Conexoes.Utilz.Excel.GetTabela(arquivo);
                    }
                }
                catch (Exception ex)
                {

                    Conexoes.Utilz.Alerta(ex);

                }
                this.Desbloqueia_Secao();
            }


            return retorno;
        }
        /*ESSE CARA DÁ A MOVIMENTAÇÃO DE ORDENS DE PRODUÇÃO DE UM RANGE DE DATAS*/
        public DLM.db.Tabela ZPPCOOISN(DateTime de, DateTime ate)
        {
            var dt_de = $"{de.Day}.{de.Month}.{de.Year}";
            var dt_ate = $"{ate.Day}.{ate.Month}.{ate.Year}";
            string nome = $"ZPPCOOISN_{dt_de}_a_{dt_ate}.xlsx";
            string arquivo = Cfg.Init.GetDestinoSAP_Excel() + nome;
            string destino = Conexoes.Utilz.getPasta(arquivo);

            DLM.db.Tabela retorno = new db.Tabela();

            DLM.painel.Consultas.MatarExcel(false);
            if (arquivo.Existe())
            {
                if (!arquivo.Apagar())
                {
                    return retorno;
                }
            }

            try
            {
                if (File.Exists(arquivo))
                {
                    File.Delete(arquivo);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPPCOOISN");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_PROJN-LOW")).Text ="";
                    /*
                        session.findById("wnd[0]/usr/ctxtS_BUDAT-LOW").text = "05.07.2022"
                        session.findById("wnd[0]/usr/ctxtS_BUDAT-HIGH").text = "06.07.2022"
                     */
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_BUDAT-LOW")).Text = dt_de;
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_BUDAT-HIGH")).Text = dt_ate;
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_VARI")).Text = "/SISTEMA";

                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, nome, Cfg.Init.SAP_SCRIPT_IMPRESSAO_ZPMP);
                    if (!File.Exists(destino + nome))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell", false);
                        ExportaExcelNativo(destino, nome, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();
                    return Conexoes.Utilz.Excel.GetTabela(arquivo);

                }

            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }
            return retorno;
        }



        public void Retornar()
        {
            try
            {
                var st = (this.SessaoSAP.FindById("wnd[0]/usr/btnSTARTBUTTON"));
                if (st != null)
                {
                    ((GuiButton)st).Press();
                }

            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
            }
            try
            {
                ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtTCNT-PROF_DB")).Text = "PS0000000001";
                ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
            }
        }

        /*ESSE CARA DÁ O AVANÇO DE LOGÍSTICA*/
        public bool ZPMP(string Pedido, string destino, string ARQUIVO, bool msgs = false)
        {
   
            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPMP");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_PSPID-LOW")).Text = Pedido + "*";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtPC_VARI")).Text = @"/SALDO_PT";


                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_POSID-LOW")).Text = "";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_WERKS-LOW")).Text = "";

                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();




                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO_ZPMP);
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    if(msgs)
                    {

                    MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                return false;
            }

        }


        /*ESSE CARA DÁ AS NOTAS FISCAIS DA OBRA*/
        public bool ZCONTRATOS(string Pedido, string destino, string ARQUIVO, bool msgs = false)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("zcontratos");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_PEP-LOW")).Text = Pedido;

                    ((GuiRadioButton)this.SessaoSAP.FindById("wnd[0]/usr/radP_NF")).Select();

                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();




                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO_ZPMP);
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    if (msgs)
                    {

                        MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (msgs)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                return false;
            }

        }

        /*ESSE CARA DÁ AS CARACTERÍSTICAS DAS PEÇAS*/
        public bool ZPPCOOISN(string Pedido, string destino, string ARQUIVO, bool msgs = false, string layout = "")
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPPCOOISN");

                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_PROJN-LOW")).Text = Pedido + "*";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_VARI")).Text = @"";


                    //session.findById("wnd[0]/usr/ctxtP_VARI").text = "/SISTEMA"
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_VARI")).Text = layout;



                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO_ZPMP);
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    if (msgs)
                    {

                        MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {

                if (msgs)
                {

                    Conexoes.Utilz.Alerta(ex);
                }
                else
                {
                    DLM.log.Log(ex);
                }
                return false;
            }

        }
       
        /*ESSE CARA DÁ O AVANÇO DE LOGÍSTICA NOVO*/
        public bool ZPP0100(string Pedido, string destino, string ARQUIVO)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    this.SessaoSAP.StartTransaction("ZPP0100");
             
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/usr/btn%#AUTOTEXT009")).Press();
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_POSID-LOW")).Text = Pedido;
                    //((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_DATA-LOW")).Text = antes.Day.ToString().PadLeft(2, '0') + "." + antes.Month.ToString().PadLeft(2, '0') + "." + antes.Year;
                    //((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_DATA-HIGH")).Text = agora.Day.ToString().PadLeft(2, '0') + "." + agora.Month.ToString().PadLeft(2, '0') + "." + agora.Year;
                    //09/04/2020 - removi as datas pq o zpp0100 nao precisa mais
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_DATA-LOW")).Text = "";
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtS_DATA-HIGH")).Text = "";
                    ((GuiTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_LAYOUT")).Text = "/COMPLETO";

                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();


                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO2);

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    //MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                //MessageBox.Show("Não foi possível criar o arquivo\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }

        }


        /*ESSE CARA DÁ OS CUSTOS DA OBRA*/
        public bool CJI3(string Pedido, string destino, string ARQUIVO)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    Retornar();

                    this.SessaoSAP.StartTransaction("CJI3");

                    
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtCN_PROJN-LOW")).Text = Pedido;
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/usr/btn%_CN_PSPNR_%_APP_%-VALU_PUSH")).Press();

                    try
                    {
                        /*esse cara limpa a lista*/

                        ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[16]")).Press();

                    }
                    catch (Exception ex)
                    {
                        DLM.log.Log(ex);
                    }

                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,0]")).Text = "*FM*";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,1]")).Text = "*DES*";
                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[8]")).Press();
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtR_BUDAT-LOW")).Text = DateTime.Now.AddYears(-3).ToShortDateString().Replace("/",".");
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtR_BUDAT-HIGH")).Text = DateTime.Now.AddMonths(1).ToShortDateString().Replace("/", ".");
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtP_DISVAR")).Text = "/PAINEL";
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();

                    try
                    {
                        /*GERANDO O EXCEL*/
                        ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[43]")).Press();
                        ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();
                        ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                        ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = ARQUIVO;
                        ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[11]")).Press();
                    }
                    catch (Exception ex)
                    {

                        DLM.log.Log(ex);
                    }
                   

                    /*SE NÃO CONSEGUIU GERAR O EXCEL*/
                    if(!File.Exists(destino + ARQUIVO))
                    {
                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO2);
                    }
                    if (!File.Exists(destino + ARQUIVO))
                    {
                        var ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/cntlALVCONTAINER/shellcont/shell", false);
                        ExportaExcelNativo(destino, ARQUIVO, ctrl);
                    }

                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    //MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                //MessageBox.Show("Não foi possível criar o arquivo\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }

        }

        public static DateTime antes { get; set; } = DateTime.Now.AddYears(-4);
        public static DateTime agora { get; set; } = DateTime.Now.AddYears(1);
        /*ESSE CARA DÁ OS CUSTOS DA OBRA*/
        public bool FAGLL03(List<string> peps, string destino, string ARQUIVO)
        {

            try
            {
                if (File.Exists(destino + ARQUIVO))
                {
                    File.Delete(destino + ARQUIVO);
                }
                if (this.Carregar_sap())
                {
                    Retornar();
                    Retornar();

                    this.SessaoSAP.StartTransaction("fagll03");


                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtSD_SAKNR-LOW")).Text = "4111001011";
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtSD_BUKRS-LOW")).Text = "1200";


                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[25]")).Press();
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell")).ExpandNode("         22");
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell")).SelectNode("         41");
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell")).TopNode ="          1";
                    ((GuiTree)this.SessaoSAP.FindById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell")).DoubleClickNode("         41");

                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/usr/btn%_%%DYN001_%_APP_%-VALU_PUSH")).Press();

                    //limpa a lista
                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[16]")).Press();
                   

                    for (int i = 0; i < peps.Count; i++)
                    {
                        ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1," + 1 + "]")).Text = peps[i];
                        ((SAPFEWSELib.ISapTableControlTarget)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE")).VerticalScrollbar.Position = i + 1;
                    }
                    //((GuiFrameWindow)this.SessaoSAP.FindById("wnd[1]")).SendVKey(8);
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,0]")).Text = "20-103826.P00.002.20A.F3";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,1]")).Text = "20-103826.P00.001.60B.F2";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,2]")).Text = "20-103826.P00.004.30B.F2";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,3]")).Text = "20-103795.P00.001.20A.F3";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,4]")).Text = "20-103795.P00.001.20B.F2";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,5]")).Text = "20-103795.P00.001.20B.F3";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,6]")).Text = "20-103795.P00.001.30A.F2";
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE/ctxtRSCSEL_255-SLOW_I[1,7]")).Text = "20-103795.P00.001.30A.F3";

                    ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[8]")).Press();
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[0]/btn[11]")).Press();

                    ((GuiRadioButton)this.SessaoSAP.FindById("wnd[0]/usr/radX_AISEL")).Select();

                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtSO_BUDAT-LOW")).Text = antes.Day.ToString().PadLeft(2, '0') + "." + antes.Month.ToString().PadLeft(2, '0') + "." + antes.Year;
                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtSO_BUDAT-HIGH")).Text = agora.Day.ToString().PadLeft(2, '0') + "." + agora.Month.ToString().PadLeft(2, '0') + "." + agora.Year;

                    ((GuiCTextField)this.SessaoSAP.FindById("wnd[0]/usr/ctxtPA_VARI")).Text = "/PAINEL";

                    /*rodar*/
                    ((GuiButton)this.SessaoSAP.FindById("wnd[0]/tbar[1]/btn[8]")).Press();



                    /*gerar excel*/
                    //((GuiMenu)this.SessaoSAP.FindById("wnd[0]/mbar/menu[0]/menu[3]/menu[1]")).Select();
                    //((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = ARQUIVO;
                    //((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[11]")).Press();

                    /*GERAR EXCEL TENTATIVA 2*/
                    //((GuiFrameWindow)this.SessaoSAP.FindById("wnd[0]")).SendVKey(16);
                    //((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                    //((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = ARQUIVO;
                    ////*tenta mandar um CTRL+S*/
                    //((GuiModalWindow)this.SessaoSAP.FindById("wnd[1]")).SendVKey(11);

                    //((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[11]")).Press();
                    /*GERAR EXCEL TENTATIVA 3*/
                    ExportarExcel(destino, ARQUIVO, Cfg.Init.SAP_SCRIPT_IMPRESSAO_FAGLL03);


                    this.SessaoSAP.EndTransaction();
                    return File.Exists(destino + ARQUIVO);

                }
                else
                {
                    //MessageBox.Show("Não foi possível criar o arquivo\nNão foi possível carregar o SAP. Verifique se está logado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                //MessageBox.Show("Não foi possível criar o arquivo\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }

        }

        /*cancelar: session.findById("wnd[0]/tbar[0]/btn[3]").press*/
        /*ESSE CARA TENTA EXPORTAR EM EXCEL A TELA QUE ESTÁ EM EXECUÇÃO NO SAP*/
        private void ExportarExcel(string destino, string NOME, string SCRIPT_ORIGEM)
        {

            var SCR = Biblioteca_Daniel.Arquivo_Pasta.Buffer_Texto.retorna_arquivo(SCRIPT_ORIGEM).Select(X => X.Replace("$NOME$", NOME).Replace("$TAM$", (NOME.Length-1).ToString()).Replace("$DESTINO$", destino)).ToList();
            Funcoes.RodaScript(SCR, destino);

        }

        private void ExportaExcelNativo(string destino, string NOME, object ctrl)
        {
            try
            {
                /*CN47N*/
                if (ctrl == null)
                {
                    ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/cntlALVCONTAINER/shellcont/shell", false);
                }
                /*ZPP066N*/
                if (ctrl == null)
                {
                    ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell/shellcont/shell", false);
                }
                /*ZPMP*/
                if (ctrl == null)
                {
                    ctrl = SessaoSAP.ActiveWindow.FindById("wnd[0]/usr/shell", false);
                }
                if (ctrl == null)
                {
                    //MessageBox.Show("Não conseguir exportar em excel o arquivo " + destino + "\nNão encontrei a chamada de menu do SAP para exportação. Contacte suporte.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int tentativas = 0;
                denovo:
                var shellToolbarContextButton = ((GuiShell)ctrl);
                var btnToolbarContextButton = shellToolbarContextButton as GuiGridView;
                btnToolbarContextButton?.ContextMenu();
                btnToolbarContextButton?.SelectContextMenuItem("&XXL");


                ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[0]")).Press();



                ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_PATH")).Text = destino;
                ((GuiCTextField)this.SessaoSAP.FindById("wnd[1]/usr/ctxtDY_FILENAME")).Text = NOME;
                ((GuiButton)this.SessaoSAP.FindById("wnd[1]/tbar[0]/btn[11]")).Press();


                var arquivo = destino + NOME;
                if(!arquivo.Existe() && tentativas == 0)
                {
                    ctrl = this.SessaoSAP.FindById("wnd[0]/usr/shell/shellcont/shell",false);
                    tentativas++;
                    goto denovo;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                //throw;
            }
        }

        public Consulta()
        {

        }
    }


}
