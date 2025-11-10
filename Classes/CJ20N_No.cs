using Conexoes;
using DLM.vars;
using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DLM.sapgui
{
    public class CJ20N_No
    {
        public void Expand()
        {
            if(this.arvore !=null)
            {
            this.arvore.ExpandNode(this.key);
            }
        }
        public void Collapse()
        {
            if (this.arvore != null)
            {
                this.arvore.CollapseNode(this.key);
            }
        }
        [Browsable(false)]
        public System.Windows.Media.ImageSource imagem
        {
            get
            {
                switch (this.tipo)
                {
                    case CJ20N_Tipo.Pedido:
                return BufferImagem.folder_green;
                    case CJ20N_Tipo.Etapa:
                        return BufferImagem.folder;
                    case CJ20N_Tipo.SubEtapa:
                        return BufferImagem.folder_txt;
                    case CJ20N_Tipo.PEP:
                        return BufferImagem.folder_downloads;
                    case CJ20N_Tipo.Tarefa:
                        return BufferImagem.circulo_verde_16x16;
                    case CJ20N_Tipo.Desconhecido:
                        return BufferImagem.circulo_16x16_laranja;
                }
                return BufferImagem.circulo_16x16;
            }
        }

        public CJ20N_Tipo tipo
        {
            get
            {
                var s = this.pai;
                if(s!=null)
                {
                    if (!this.Nome.Contem(this.GetRaiz().Nome))
                    {
                        return CJ20N_Tipo.Tarefa;
                    }
                }

                var sts = this.Nome.Replace(" ", "").Replace("-","");
                if(sts.ESoNumero())
                {
                    return CJ20N_Tipo.Tarefa;
                }
                
                if(this.pep!="")
                {
                    return CJ20N_Tipo.PEP;
                }
                else if(this.subetapa!="")
                {
                    return CJ20N_Tipo.SubEtapa;
                }
                else if(this.etapa!="")
                {
                    return CJ20N_Tipo.Etapa;
                }
                else if(this.pedido!="")
                {
                    return CJ20N_Tipo.Pedido;
                }
                return CJ20N_Tipo.Desconhecido;
            }
        }
        public string pedido
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Pedido(this.Nome);
            }
        }
        public string pedido_completo
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Pedido(this.Nome,true);
            }
        }
        public string etapa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Etapa(this.Nome);
            }
        }
        public string etapa_completa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Etapa(this.Nome,true);
            }
        }
        public string subetapa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Subetapa(this.Nome);
            }
        }
        public string subetapa_completa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Subetapa(this.Nome,true);
            }
        }
        public string pep
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.PEP(this.Nome);
            }
        }
        public string pep_completo
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.PEP(this.Nome,true);
            }
        }
        public CJ20N_No GetNo(string fim)
        {
           if(this._filhos!=null)
            {
                return this._filhos.Find(x => x.Nome.ToUpper().EndsW(fim.ToUpper()));
            }
            return null;
        }
        public bool AddTarefa( string centro_de_trabalho = "1202",string divisao = "1202",string centro = "1202")
        {
            var s = this.Getfilhos(true).FindAll(x => !x.Nome.Contem(this.Nome)).ToList();
            if (s.Count>0)
            {
                return true;
            }
            this.SetNo();
            return this.SAP.CJ20N_CriarTarefa(this.Nome, this.descricao, centro_de_trabalho, divisao, centro, true);
        }

        public bool Atualizar( string novo_nome=null, string nova_descricao = null, string novo_centro =null, string nova_divisao=null, bool atualizar_tarefas = true)
        {
          return  this.SAP.CJ20N_EditarPEP(this, novo_nome, nova_descricao, novo_centro, nova_divisao, atualizar_tarefas);
        }
        public void AddEtapa(string nome, string descricao, bool planejado = true, bool contabil = true,bool faturamento = false,string empresa = "1100", string centro = "1104",string divisao = "1104")
        {
            
            var fls = this.Getfilhos(true);
            var pep = this.Nome + "." + nome.ToUpper();
            if(this.GetRaiz().nos.Find(x=> x.Nome.ToUpper() == pep.ToUpper())==null)
            {
                this.SetNo();
                var ss = this.SAP.CJ20N_CriarEtapa(this.Nome + "." + nome, descricao, planejado, contabil, faturamento, empresa, centro, divisao, true);
                if (ss)
                {
                    foreach (var s in this.Getfilhos(true))
                    {
                        this.GetRaiz().nos.Remove(s);
                    }
                    this._filhos = new List<CJ20N_No>();
                    this.Getfilhos(true);
                }
            }

        }

        public bool Apagar()
        {
            if(this.key.Replace(" ", "") == "") { return false; }
            return this.SAP.CJ20N_Apagar(this);
        }
        public SAP_Consulta_Macro SAP { get; set; }
        public List<CJ20N_No> nos { get; set; } = new List<CJ20N_No>();
        public CJ20N_No GetRaiz()
        {
            var s = this.pai;
            var s1 = this.pai;
            while (s != null)
            {
                s = s.pai;
                if (s != null)
                {
                    s1 = s;
                }
            }
            if(this.pai==null)
            {
                return this;
            }
            return s1;
        }
        public GuiTree SetNo(string num = null)
        {

            if (num == null)
            {
                num = this.key;
            }
            // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").topNode = "000002"
            arvore.TopNode = num;
            // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").selectedNode = "000002"
            arvore.SelectedNode = num;
            // session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").expandNode "000002"
            //arvore.ExpandNode(num);
            //session.findById("wnd[0]/shellcont/shellcont/shell/shellcont[0]/shell/shellcont[1]/shell").nodeContextMenu "000002"
            arvore.NodeContextMenu(num);

            return arvore;
        }

        public CJ20N_No GetNoProjeto(string key)
        {
            if(arvore!=null)
            {
                try
                {
                    var desc = arvore.GetNodeTextByKey(key);
                    var pep = arvore.GetItemText(key, "TECH_KEY");
                    string chave_pep = "";


                    CJ20N_No no = new CJ20N_No(pep, desc, key, chave_pep, arvore, this.SAP);
                    if (no.tipo == CJ20N_Tipo.Desconhecido | no.tipo == CJ20N_Tipo.Tarefa)
                    {
                        try
                        {
                            no.SetNo();
                            chave_pep = ((GuiTextField)this.SAP.SessaoSAP.FindById("wnd[0]/usr/subDETAIL_AREA:SAPLCNPB_M:1010/subVIEW_AREA:SAPLCONW:1001/tabsTABSTRIP_1000/tabpARBD/ssubSUBSCR_1000:SAPLCONW:1310/ctxtAFVGD-PROJN")).Text;

                        }
                        catch (Exception)
                        {

                        }
                    }
                    no.Nome = pep;
                    no.descricao = desc;
                    no.key = key;
                    no.chave_pep = chave_pep;
                    return no;
                }
                catch (Exception)
                {

                }
                
            }
            return null;
        }
        public CJ20N_No pai { get; set; }
        public override string ToString()
        {
            return "[" + this.tipo + "] "  + this.Nome + " - " + this.descricao;
        }
        public GuiTree arvore { get; private set; }
        public string Nome { get; set; } = "";
        public string key { get; set; } = "";
        public string chave_pep { get; set; } = "";
        public string descricao { get; set; } = "";
        private List<CJ20N_No> _filhos { get; set; } = new List<CJ20N_No>();
        public List<CJ20N_No> Getfilhos(bool reset = false)
        {
            if(this.tipo == CJ20N_Tipo.Desconhecido| this.tipo == CJ20N_Tipo.Tarefa)
            {
                return new List<CJ20N_No>();
            }
            if((_filhos.Count==0 | reset) && this.arvore!=null)
            {
                _filhos = new List<CJ20N_No>();
                this.SetNo();
                this.Expand();
                var lvl0 = arvore.GetAllNodeKeys();
                
                

                var keys = new List<string>();
                foreach(string s in lvl0)
                {
                    keys.Add(s);
                }

                keys = keys.OrderBy(x => x.Int()).ToList();



                foreach (var key in keys)
                {

                    if (key.Int() > this.key.Int())
                    {
                        var b = GetRaiz().nos.Find(x => x.key == key);

                       
                        if (b == null)
                        {
                            
                            string pai = this.arvore.GetParent(key);
                            if (pai != this.key)
                            {
                                continue;
                            }
                            else
                            {
                                b = GetNoProjeto(key);
                                b.pai = this;
                                this._filhos.Add(b);
                                GetRaiz().nos.Add(b);
                            }
                        }
                        else
                        {
                            //teóricamente não deveria vir aqui.
                            if(b.pai==this && this._filhos.Find(x=>x.Nome == b.Nome)==null)
                            {
                                this._filhos.Add(b);
                            }

                        }

                       

                        //if(this.tipo != CJ20N_Tipo.Pedido)
                        //{
                        //    var igual = this.pai.Getfilhos().Find(x => x.key == key);
                        //    if(igual != null)
                        //    {
                        //        continue;
                        //    }
                        //}
                        //if (b != null)
                        //{

                            

                        //    //precisei fazer essas validações pois a arvore no sap retorna toda a lista. mesmo setando um nó específico
                        //    if (
                        //        (this.tipo == CJ20N_Tipo.Etapa && b.tipo == CJ20N_Tipo.SubEtapa | b.tipo == CJ20N_Tipo.PEP) |
                        //        (this.tipo == CJ20N_Tipo.PEP && (b.tipo == CJ20N_Tipo.Desconhecido | b.tipo == CJ20N_Tipo.Tarefa)) |
                        //        (this.tipo == CJ20N_Tipo.Pedido && (b.tipo == CJ20N_Tipo.Etapa)) |
                        //        (this.tipo == CJ20N_Tipo.SubEtapa && (b.tipo == CJ20N_Tipo.PEP))
                        //        )
                        //    {
                        //        if ((b.tipo != CJ20N_Tipo.Desconhecido && b.tipo != CJ20N_Tipo.Tarefa && b.nome.Contains(this.nome)))
                        //        {
                        //            if (b.tipo == CJ20N_Tipo.PEP && this.tipo == CJ20N_Tipo.Etapa)
                        //            {
                        //                //tive que adicionar esse tratamento para que ele reconheça quando tem projetos que não tem sub-etapas.
                        //                //a principio ele vai antes entrar nas sub-etapas e depois nos PEPs, então o buffer de nós estará com alguma sub-etapa carregada.
                        //                if (this.GetRaiz().nos.FindAll(x => x.tipo == CJ20N_Tipo.SubEtapa).Count == 0)
                        //                {
                        //                    b.pai = this;
                        //                    _filhos.Add(b);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                if (b.nome.Contains(this.nome))
                        //                {
                        //                    b.pai = this;
                        //                    _filhos.Add(b);
                        //                }
                        //            }

                        //        }
                        //        else if ((b.tipo == CJ20N_Tipo.Tarefa | b.tipo == CJ20N_Tipo.Desconhecido) && this.tipo == CJ20N_Tipo.PEP)
                        //        {
                        //            if (b.chave_pep == this.nome)
                        //            {
                        //                b.pai = this;
                        //                _filhos.Add(b);
                        //            }
                        //        }
                        //        else

                        //        {
                        //            if (b.key != this.GetRaiz().key)
                        //            {
                        //                b.pai = this.GetRaiz();
                        //            }
                        //        }

                        //    }

                            
                        //}
                    }
                }
                //this.Collapse();
            }
            return _filhos;
        }
        public CJ20N_No(string nome, string descricao, string key, string chave_pep, GuiTree arvore, SAP_Consulta_Macro SessaoSAP)
        {
            this.arvore = arvore;
            this.Nome = nome;
            this.key = key;
            this.descricao = descricao;
            this.SAP = SessaoSAP;
            this.chave_pep = chave_pep;
            if (this.arvore != null)
            {
            }
        }
        public CJ20N_No(GuiTree arvore, SAP_Consulta_Macro SessaoSAP)
        {
            this.arvore = arvore;
            this.key = key;
            GetDados();
            this.SAP = SessaoSAP;
        }

        public void GetDados()
        {
            if(this.arvore==null)
            {
                return;
            }
            try
            {
                this.Nome = this.arvore.GetItemText(key, "TECH_KEY"); ;
                this.descricao = this.arvore.GetNodeTextByKey(key);
            }
            catch (Exception)
            {

            }

        }
    }
}
