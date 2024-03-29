﻿using Conexoes;
using DLM.sap;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DLM.painel
{
    public class PLAN_PECA
    {
        public Tipo_Embarque Tipo_Embarque { get; set; } = Tipo_Embarque.ZPP0100;
       public void SetStatusByZPP0100(List<DLM.db.Linha> linhas)
        {
            var lista = linhas.FindAll(x => x["Material"].Valor == this.material.ToString());

            this.qtd_embarcada = lista.FindAll(x=>x["St_Conf_"].Valor.ToUpper() == Cfg.Init.ZPP0100_CARGA_CONFIRMADA).Sum(x => x.Get("Qtd_Embarque").Double());
            var marca = lista.Select(x => x["Tamanho_dimensao"].Valor).Distinct().ToList().FindAll(x=>x.Replace(" ","")!="");

            if(marca.Count>0)
            {
                var mm = marca[0];
                if(this.material.ToString() == this.desenho | this.desenho=="")
                {
                    this.desenho = mm;
                }
            }
         
            this.Tipo_Embarque = Tipo_Embarque.ZPP0100;
          
        }

        public Tipo_Material Tipo { get; set; } = Tipo_Material.Real;
        public SAP_ESQ_PIN Esquema
        {
            get
            {
                var DUMMY = new SAP_ESQ_PIN() { ESQUEMA_DESCR = "", PINTURA = this.TIPO_DE_PINTURA };
                if (this.TIPO_DE_PINTURA.ToUpper().Contains("SEM") | this.TIPO_DE_PINTURA.ToUpper().Contains("GALVANIZADO") | this.esq_de_pintura.Replace("0", "") == "" | this.TIPO_DE_PINTURA == "")
                {
                    return DUMMY;
                }
                var t = DBases.GetEsquemas().Find(x => x.ESQUEMA_COD == this.esq_de_pintura);
                if (t != null)
                {
                    return t;
                }
                else
                {
                    DUMMY.ESQUEMA_DESCR = "Falta Cadastro";
                    DUMMY.ESQUEMA_COD = this.esq_de_pintura;
                    return DUMMY;
                }

            }
        }
        private ImageSource _imagem { get; set; } = null;
        public ImageSource imagem
        {
            get
            {
                if (_imagem == null)
                {
                    if (Tipo == Tipo_Material.Real)
                    {
                        //_imagem =  Conexoes.BufferImagem.R_VERDE;
                        if (File.Exists(arquivo))
                        {
                            _imagem = Conexoes.BufferImagem.R_VERDE;
                        }
                        else
                        {
                            _imagem = Conexoes.BufferImagem.R_PRETO;
                        }
                    }
                    else if (Tipo == Tipo_Material.Consolidado)
                    {

                        _imagem = Conexoes.BufferImagem.C_VERDE;
                    }
                    else if (Tipo == Tipo_Material.Orçamento)
                    {

                        _imagem = Conexoes.BufferImagem.O_VERDE;
                    }
                    else
                    {

                        _imagem = Conexoes.BufferImagem.dialog_error;
                    }
                  
                }
                return _imagem;
            }
        }
        public List<DLM.db.Celula> AddPep_E_Material(List<DLM.db.Celula> valores)
        {
            if (this.material == "")
            {
                return new List<DLM.db.Celula>();
            }

            var retorno = new List<DLM.db.Celula>();
            retorno.AddRange(valores);
            retorno.Add(new DLM.db.Celula("material", this.material));
            retorno.Add(new DLM.db.Celula("pep", this.PEP));
            return retorno;
        }

        public DateTime ultima_edicao { get; private set; } = Cfg.Init.DataDummy();
        public string unidade
        {
            get
            {
                if (PEP.Length >= 24)
                {
                    return PEP.Substring(22, 2);
                }
                return "";
            }
        }
        private string _arquivo { get; set; }
        public string arquivo
        {
            get
            {
                if (_arquivo == null)
                {
                    if (this.DESENHO_1 != "")
                    {
                        this._arquivo = this.DESENHO_1;
                    }
                    else if (this.desenho != "")
                    {
                        var pasta = Cfg.Init.DIR_SAP_PDF_DESENHOS + contrato + pedido + "_" + etapa + subetapa + @"\";
                        if (!Directory.Exists(pasta))
                        {
                            var pastas = Biblioteca_Daniel.Arquivo_Pasta.Listar_Pastas(Cfg.Init.DIR_SAP_PDF_DESENHOS, contrato + pedido + "_" + etapa + subetapa + "*");
                            foreach (var p in pastas)
                            {
                                var arq = pasta + @"\" + desenho + ".pdf";
                                if (File.Exists(arq))
                                {
                                    this._arquivo = arq;

                                }
                            }


                        }
                        else
                        {
                            var arq2 = pasta + @"\" + desenho + ".pdf";
                            if (File.Exists(arq2))
                            {
                                this._arquivo = arq2;

                            }
                            else
                            {
                                this._arquivo = "";
                            }

                        }
                    }



                }
                return _arquivo;
            }
        }
        public string DENOMINDSTAND { get; private set; } = "";
        public string DESENHO_1 { get; private set; } = "";
        public string TIPO_DE_PINTURA { get; private set; } = "";
        public string ULTIMO_STATUS { get; private set; } = "";

        public DateTime? inicio { get; set; } = Cfg.Init.DataDummy();
        public DateTime? fim { get; set; } = Cfg.Init.DataDummy();
        public string subetapa
        {
            get
            {
                if (PEP.Length >= 21)
                {
                    return PEP.Substring(18, 3);
                }
                return "";
            }
        }
        public string etapa
        {
            get
            {
                if (PEP.Length >= 21)
                {
                    return PEP.Substring(14, 3);
                }
                return "";
            }
        }
        public string contrato
        {
            get
            {
                if (PEP.Length >= 12)
                {
                    return PEP.Substring(3, 6);
                }
                return "";
            }
        }
        public string pedido
        {
            get
            {
                if (PEP.Length >= 12)
                {
                    return PEP.Substring(10, 3);
                }
                return "";
            }
        }

        public string pedido_completo
        {
            get
            {
                if (PEP.Length >= 13)
                {
                    return PEP.Substring(0, 13) + (this.Tipo == Tipo_Material.Orçamento ? ".PGO" : "");
                }
                return "";
            }
        }
        public override string ToString()
        {
            return this.PEP + "/" + this.desenho + "/" + this.material + " - [N: " + qtd_necessaria + "]" + "[F: " + qtd_produzida + "[E: " + qtd_embarcada + "]";
        }

        public double qtd_necessaria { get; private set; } = 0;
        public double espessura { get; private set; } = 0;
        private double _qtd_embarcada { get; set; } = 0;
        public double qtd_embarcada { get; private set; } = 0;
        public double qtd_produzida { get; private set; } = 0;
        public double peso_unitario { get; private set; } = 0;
        private List<Logistica_Planejamento> _logistica { get; set; }
        public List<Logistica_Planejamento> logistica
        {
            get
            {
                if (_logistica == null)
                {
                    _logistica = new List<Logistica_Planejamento>();

                    var consulta = DBases.GetDB().Consulta($"SELECT *  FROM {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0066n_logistica} as pr where pr.pep ='{PEP}' and pr.material = '{material}'");
                    foreach (var linha in consulta.Linhas)
                    {
                        this._logistica.Add(new Logistica_Planejamento(this, linha));
                    }
                }
                return _logistica;

            }
            //set
            //{
            //    _logistica = value;

            //}
        }
        public void SetLogistica(List<Logistica_Planejamento> logs)
        {
            this._logistica = logs;
            foreach(var s in this._logistica)
            {
                s.peca = this;
            }
        }
        private string _desenho { get; set; } = "";
        public string marca
        {
            get
            {
                if(desenho!="") return desenho;


                return this.material.ToString();
            }
        }
        public string desenho
        {
            get
            {
                if (_desenho != "") { return _desenho; };
                return material.ToString();
            }
            set
            {
                _desenho = value;
            }
        }
        public string PEP { get; private set; } = "";
        public string pep_cooisn { get; private set; } = "";
        public string material { get; private set; } = "";
        public string texto_breve { get; private set; } = "";
        public string grupo_mercadoria { get; private set; } = "";
        public double peso_necessario { get; private set; } = 0;
        public double peso_a_produzir
        {
            get
            {
                var s = Math.Round(peso_necessario - peso_produzido,2);
               if(s>0)
                {
                    return s;
                }
                return 0;
            }
        }
        public double peso_produzido { get; private set; } = 0;
        public double peso_embarcado { get; private set; } = 0;

        public double fabricado_porcentagem
        {
            get
            {
                if(total_fabricado== 0) { return 0; }
                double vv = 100;
                return Math.Round(total_fabricado / vv, 2);
            }
        }
        public double embarcado_porcentagem
        {
            get
            {
                if (total_embarcado == 0) { return 0; }
                double vv = 100;
                return Math.Round(total_embarcado / vv, 2);
            }
        }
        public double total_fabricado
        {
            get
            {

                if (this.qtd_produzida > 0 && this.qtd_necessaria > 0)
                {
                    var s = Math.Round((double)(this.qtd_produzida / this.qtd_necessaria) * 100, 2);
                    if (s > 0)
                    {
                        return 100;
                    }
                    return s;
                }
                return 0;
            }
        }
        public double total_embarcado
        {
            get
            {
                if(this.qtd_embarcada>0 && this.qtd_necessaria>0)
                {
                var s = Math.Round((double)(this.qtd_embarcada / this.qtd_necessaria) * 100, 2);
                    if(s>0)
                    {
                        return 100;
                    }
                    return s;
                }
                return 0;
            }

        }

        public long qtd_mercadoria_entrada { get; private set; } = 0;

        public double corte_largura { get; private set; } = 0;
        public double comprimento { get; private set; } = 0;
        public int furacoes { get; private set; } = 0;
        public double superficie { get; private set; } = 0;
        private string _esq_de_pintura { get; set; } = "";
        public string esq_de_pintura
        {
            get
            {
                if (_esq_de_pintura.Replace("0", "").Replace(" ", "") == "")
                {
                    return "";
                }
                return _esq_de_pintura;
            }
            set
            {
                _esq_de_pintura = value;
            }
        }
        public string tipo_aco { get; private set; } = "";
        private string _codigo_materia_prima_sap { get; set; } = "";

        public string chave_material
        {
            get
            {
                if (codigo_materia_prima_sap.Replace("0", "").Replace(" ", "") != "")
                {
                    return codigo_materia_prima_sap;
                }
                else return this.espessura.String() + (this.tipo_aco != "" ? " - " + this.tipo_aco : "");
            }
        }

        public string codigo_materia_prima_sap
        {
            get
            {
                    return _codigo_materia_prima_sap;
     
            }
            set
            {
                var s = Conexoes.Utilz.Int(value).ToString();
                if (s.Replace("0", "") != "") { _codigo_materia_prima_sap = s; }

            }
        }

        private Bobina _bobina { get; set; }
        public Bobina bobina
        {
            get
            {
                GetBobina();
                return _bobina;

            }
            set { this._bobina = value; }
        }

        public void GetBobina()
        {
            if (this._bobina == null)
            {
                if (this.codigo_materia_prima_sap.Replace("0","") == "")
                {
                    this._bobina = new Bobina();

                }
                else
                {
                    var ts = Buffer.Bobinas.Find(x => x.SAP == this.codigo_materia_prima_sap);
                    if(ts==null)
                    {
                        var t = DBases.GetBancoRM().GetBobina(this.codigo_materia_prima_sap);
                        if (t != null)
                        {
                            Buffer.Bobinas.Add(t);
                            this._bobina = t;
                        }
                        else
                        {
                            Bobina b = new Bobina() { SAP = this.codigo_materia_prima_sap };
                            Buffer.Bobinas.Add(b);
                            this._bobina = b;

                        }
                    }
                  
                    else
                    {
                        this._bobina = ts;
                    }
                }
            }
        }

        public double saldo_peso_produzido { get; private set; } = 0;
        public string status_sistema_pep { get; private set; } = "";
        public string centro { get; private set; } = "";
        public string status_usuario_pep { get; private set; } = "";
        public string status_sistema_tarefa { get; private set; } = "";

        public string Complexidade { get; set; } = "";




        public PLAN_PECA(DLM.db.Linha peca, bool orcamento = false)
        {
            if (!orcamento)
            {
                this.Tipo = Tipo_Material.Real;
                this.material = peca.Get("material").Valor;
                this.PEP = peca.Get("pep").Valor;
                this.texto_breve = peca.Get("texto_breve").Valor;
                this.peso_unitario = peca.Get("peso_unitario").Double();
                this.peso_necessario = peca.Get("peso_necessario").Double();
                this.peso_produzido = peca.Get("peso_produzido").Double(6);
                this.qtd_necessaria = peca.Get("qtd_necessaria").Double();
                this.qtd_produzida = peca.Get("qtd_produzida").Double();
                this.qtd_embarcada = peca.Get("qtd_embarcada").Double();
                this.grupo_mercadoria = peca.Get("grupo_mercadoria").Valor;
                this.desenho = peca.Get("desenho").Valor;

                this.DENOMINDSTAND = peca.Get("DENOMINDSTAND").Valor;
               
                this.inicio = peca.Get("DATA_INICIO").Data();
                this.fim = peca.Get("DATA_FIM").Data();
                this.DESENHO_1 = peca.Get("DESENHO_1").Valor;
                this.TIPO_DE_PINTURA = peca.Get("TIPO_DE_PINTURA").Valor;

                /*04/04/2019 - novas caracterísricas*/
                this.corte_largura = Utilz.Double(peca.Get("CORTE_LARGURA").Valor);
                this.comprimento = Utilz.Double(peca.Get("COMPRIMENTO").Valor);
                this.esq_de_pintura = peca.Get("ESQ_DE_PINTURA").Valor;
                this.superficie = Utilz.Double(peca.Get("SUPERFICIE").Valor);
                this.furacoes = Utilz.Int(peca.Get("FURACOES").Valor);
                this.espessura = Utilz.Double(peca.Get("ESPESSURA").Valor);
                this.tipo_aco = peca.Get("TIPO_ACO").Valor;
                this.codigo_materia_prima_sap = peca.Get("CODIGO_MATERIA_PRIMA_SAP").Valor;

                if (this.desenho == "" | this.desenho == this.material)
                {
                    this.desenho = peca.Get("MARCA").Valor;
                }

                this.peso_embarcado = this.qtd_embarcada * peso_unitario;

                /*porcentagens*/

                this.ultima_edicao = peca["ultima_edicao"].Data();
                this.pep_cooisn = peca.Get("pep_cooisn").Valor;
                this.centro = peca.Get("centro").Valor;

                var s = peca.Get("centro_producao").Valor;
                if (s != "")
                {
                    this.centro = s;
                }

                if(this.qtd_embarcada>0)
                {
                    this.ULTIMO_STATUS = "PARCIALMENTE EXPEDIDO";
                }
                else if (this.qtd_necessaria > this.qtd_produzida)
                {
                    this.ULTIMO_STATUS = peca.Get("ULTIMO_STATUS").Valor;
                }
                else if(this.qtd_necessaria> this.qtd_embarcada)
                {
                    this.ULTIMO_STATUS = "FABRICADO";
                }
                else if(this.qtd_embarcada>=this.qtd_necessaria)
                {
                    this.ULTIMO_STATUS = "EMBARCADO";
                }
            

                getComplexidade();
            }
            /*05/04/19*/
            else
            {
                this.Tipo = Tipo_Material.Orçamento;
                this.comprimento = peca.Get("comp").Double();
                this.corte_largura = peca.Get("corte").Double();
                this.desenho = peca.Get("marca").Valor;
                this.DESENHO_1 = peca.Get("marca").Valor;
                this.espessura = peca.Get("esp").Double();
                this.esq_de_pintura = peca.Get("esquema").Valor;
                this.furacoes = peca.Get("furos").Int();
                this.grupo_mercadoria = peca.Get("grupo_mercadoria").Valor;
                this.material = peca.Get("marca").Valor;
                this.PEP = peca.Get("pep").Valor;
                this.pep_cooisn = peca.Get("pep").Valor;
                this.qtd_necessaria = peca.Get("quantidade").Double();
                this.peso_unitario = peca.Get("peso").Double();
                this.peso_necessario = peca.Get("peso").Double() * this.qtd_necessaria;
                this.status_sistema_pep = peca.Get("marca").Valor;
                this.superficie = peca.Get("superficie").Double();
                this.texto_breve = peca.Get("descricao").Valor;
                this.tipo_aco = peca.Get("tipo_aco").Valor;
                this.TIPO_DE_PINTURA = peca.Get("tratamento").Valor;
                this.ultima_edicao = peca["ultima_edicao"].Data();
                this.centro = peca.Get("unidade_fabril").Valor;
                this.codigo_materia_prima_sap = peca.Get("materia_prima").Valor;

            }
        }
        public PLAN_PECA(DLM.db.Linha peca, List<DLM.db.Linha> logistica)
        {
            this.Tipo = Tipo_Material.Real;
            this.PEP = peca.Get("pep").Valor;
            this.material = peca.Get("material").Valor;
            this.texto_breve = peca.Get("texto_breve").Valor;
            this.grupo_mercadoria = peca.Get("grupo_mercadoria").Valor;
            this.peso_necessario = peca.Get("peso_necessario").Double(6);
            this.peso_produzido = peca.Get("peso_produzido").Double(6);
            this.qtd_necessaria = peca.Get("qtd_necessaria").Double();
            this.qtd_mercadoria_entrada = peca.Get("qtd_mercadoria_entrada").Long();
            this.saldo_peso_produzido = peca.Get("saldo_peso_produzido").Double();
            this.status_sistema_pep = peca.Get("status_sistema_pep").Valor;
            this.status_usuario_pep = peca.Get("status_usuario_pep").Valor;
            this.status_sistema_tarefa = peca.Get("status_sistema_tarefa").Valor;
            foreach (var t in logistica)
            {
                this.logistica.Add(new Logistica_Planejamento(this, t));
            }
            if (logistica.Count > 0)
            {
                this.desenho = logistica[0].Get("desenho").Valor;
            }

            this.peso_unitario = (peso_necessario / qtd_necessaria);
            this.qtd_embarcada = this.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
            this.peso_embarcado = qtd_embarcada * this.peso_unitario;
            this.qtd_produzida = (int)Math.Round(peso_produzido / peso_unitario);

            this.ultima_edicao = peca["ultima_edicao"].Data();

            this.DENOMINDSTAND = peca.Get("DENOMINDSTAND").Valor;
            this.DESENHO_1 = peca.Get("DESENHO_1").Valor;
            this.TIPO_DE_PINTURA = peca.Get("TIPO_DE_PINTURA").Valor;







            this.inicio = peca.Get("DATA_INICIO").Data();
            this.fim = peca.Get("DATA_FIM").Data();

            this.ULTIMO_STATUS = peca.Get("ULTIMO_STATUS").Valor;

            this.pep_cooisn = peca.Get("pep_cooisn").Valor;
            this.centro = peca.Get("centro").Valor;
            var s = peca.Get("centro_producao").Valor;
            if (s != "")
            {
                this.centro = s;
            }

            getComplexidade();

        }

        private void getComplexidade()
        {
            if (DENOMINDSTAND.Replace(" ", "").StartsWith("SS") | DENOMINDSTAND.Replace(" ", "").EndsWith("SS"))
            {
                Complexidade = "Super Simples";
            }
            else if (DENOMINDSTAND.Replace(" ", "").StartsWith("S") | DENOMINDSTAND.Replace(" ", "").EndsWith("S"))
            {
                Complexidade = "Simples";
            }
            else if (DENOMINDSTAND.Replace(" ", "").StartsWith("M") | DENOMINDSTAND.Replace(" ", "").EndsWith("M"))
            {
                Complexidade = "Média";
            }
            else if (DENOMINDSTAND.Replace(" ", "").StartsWith("C") | DENOMINDSTAND.Replace(" ", "").EndsWith("C"))
            {
                Complexidade = "Complexa";
            }
            else if (DENOMINDSTAND.Replace(" ", "").StartsWith("H") | DENOMINDSTAND.Replace(" ", "").EndsWith("H"))
            {
                Complexidade = "Hiper Complexa";
            }
            if (this.grupo_mercadoria.Contains("PARAF"))
            {
                this.grupo_mercadoria = "PARAFUSO";
            }
            else if (this.grupo_mercadoria.Contains("PORCA"))
            {
                this.grupo_mercadoria = "PARAFUSO";
            }
            else if (this.grupo_mercadoria.Contains("ARRU"))
            {
                this.grupo_mercadoria = "PARAFUSO";
            }
            else if (this.material.ToString().StartsWith("10"))
            {
                this.grupo_mercadoria = "ALMOX NÃO FATURÁVEL";
            }
            else if (this.material.ToString().StartsWith("11"))
            {
                this.grupo_mercadoria = "ALMOX";
            }
           
        }

        public PLAN_PECA()
        {

        }

        public PLAN_PECA(Logistica_Planejamento ps)
        {
            this.centro = ps.centro;
            this.PEP = ps.pep;
            this.material = ps.material;
            this.desenho = ps.desenho;
            this.texto_breve = ps.descricao;
            
        }
    }


}
