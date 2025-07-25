﻿using Conexoes;
using DLM.sap;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace DLM.painel
{
    public class PLAN_PECA
    {
        private Bobina _bobina { get; set; }
        private ImageSource _imagem { get; set; } = null;
        private string _desenho { get; set; } = "";
        private string _esq_de_pintura { get; set; } = "";
        private string _arquivo { get; set; }

        public Tipo_Embarque Tipo_Embarque { get; set; } = Tipo_Embarque.ZPP0100;
        public void SetStatusByZPP0100(List<DLM.db.Linha> linhas)
        {
            var lista_fim = linhas.FindAll(x => x["Material"].Valor == this.material.ToString());
            if (lista_fim.Count == 0)
            {
                lista_fim = linhas.FindAll(x => x["Tamanho_dimensao"].Valor == this.marca);
            }

            if (lista_fim.Count == 0)
            {
                lista_fim = linhas.FindAll(x => x["Tamanho_dimensao"].Valor.Contains(this.marca));
            }

            if (lista_fim.Count == 0)
            {

            }

            this.qtd_embarcada = lista_fim.FindAll(x => x["St_Conf_"].Valor.ToUpper() == Cfg.Init.ZPP0100_CARGA_CONFIRMADA).Sum(x => x["qtd_embarque"].Double());
            var marca = lista_fim.Select(x => x["Tamanho_dimensao"].Valor).Distinct().ToList().FindAll(x => x.Replace(" ", "") != "");

            if (marca.Count > 0)
            {
                var mm = marca[0];
                if (this.material.ToString() == this.desenho | this.desenho == "")
                {
                    this.desenho = mm;
                }
            }

            if (lista_fim.Count > 0)
            {
                var centro = lista_fim[0]["CentroProducao"].Int();
                if (centro > 0)
                {
                    this.centro = centro.ToString();
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
                if (
                    this.TIPO_DE_PINTURA.ToUpper().StartsW("SEM") |
                    this.TIPO_DE_PINTURA.ToUpper().Contains("GALVANIZADO") |
                    this.esq_de_pintura.Replace("0", "") == "" |
                    this.TIPO_DE_PINTURA == ""
                    )
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
        public ImageSource imagem
        {
            get
            {
                if (_imagem == null)
                {
                    if (Tipo == Tipo_Material.Real)
                    {
                        _imagem = BufferImagem.R_AZUL;
                    }
                    else if (Tipo == Tipo_Material.Consolidado)
                    {

                        _imagem = BufferImagem.C_VERDE;
                    }
                    else if (Tipo == Tipo_Material.Orçamento)
                    {

                        _imagem = BufferImagem.O_VERDE;
                    }
                    else
                    {

                        _imagem = BufferImagem.dialog_error;
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

            var retorno = new db.Linha();
            retorno.AddRange(valores);
            retorno.Add("material", this.material);
            retorno.Add("pep", this.PEP);
            return retorno.Celulas;
        }

        public string DENOMINDSTAND { get; private set; } = "";
        public string DESENHO_1 { get; private set; } = "";
        public string TIPO_DE_PINTURA { get; private set; } = "";
        public string ULTIMO_STATUS { get; private set; } = "";

        public DateTime? inicio { get; set; }
        public DateTime? ultima_edicao { get; private set; }
        public DateTime? fim { get; set; }
        public string unidade => Conexoes.Utilz.PEP.Get.Unidade_Fabril(this.PEP);
        public string subetapa => Conexoes.Utilz.PEP.Get.Subetapa(this.PEP, false);
        public string etapa => Conexoes.Utilz.PEP.Get.Etapa(this.PEP, false);
        public string contrato => Conexoes.Utilz.PEP.Get.Contrato(this.PEP, false);
        public string pedido => Conexoes.Utilz.PEP.Get.Pedido(this.PEP, false);
        public string pedido_completo => Conexoes.Utilz.PEP.Get.Pedido(this.PEP, true);
        public override string ToString()
        {
            return this.PEP + "/" + this.desenho + "/" + this.material + " - [N: " + qtd_necessaria + "]" + "[F: " + qtd_produzida + "[E: " + qtd_embarcada + "]";
        }

        public double qtd_necessaria { get; private set; } = 0;
        public double espessura { get; private set; } = 0;
        public double qtd_embarcada { get; private set; } = 0;
        public double qtd_produzida { get; private set; } = 0;
        public double peso_unitario { get; private set; } = 0;
        private List<PLAN_PECA_LOG> _logistica { get; set; }
        public List<PLAN_PECA_LOG> logistica
        {
            get
            {
                if (_logistica == null)
                {
                    _logistica = new List<PLAN_PECA_LOG>();

                    var consulta = DBases.GetDB().Consulta($"SELECT *  FROM {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0066n_logistica} as pr where pr.pep ='{PEP}' and pr.material = '{material}'");
                    foreach (var linha in consulta.Linhas)
                    {
                        this._logistica.Add(new PLAN_PECA_LOG(this, linha));
                    }
                }
                return _logistica;

            }
        }

        public string marca
        {
            get
            {
                if (desenho != "") return desenho;


                return this.material.ToString();
            }
        }
        public string desenho
        {
            get
            {
                if (_desenho != "") { return _desenho; }
                ;
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
                var s = (peso_necessario - peso_produzido).Round(2);
                if (s > 0)
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
                if (total_fabricado == 0) { return 0; }
                double vv = 100;
                return (total_fabricado / vv).Round(2);
            }
        }
        public double embarcado_porcentagem
        {
            get
            {
                if (total_embarcado == 0) { return 0; }
                double vv = 100;
                return (total_embarcado / vv).Round(2);
            }
        }
        public double total_fabricado
        {
            get
            {

                if (this.qtd_produzida > 0 && this.qtd_necessaria > 0)
                {
                    var s = ((double)(this.qtd_produzida / this.qtd_necessaria) * 100).Round(2);
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
                if (this.qtd_embarcada > 0 && this.qtd_necessaria > 0)
                {
                    var s = ((double)(this.qtd_embarcada / this.qtd_necessaria) * 100).Round(2);
                    if (s > 0)
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
                else return this.espessura.String(2) + (this.tipo_aco != "" ? " - " + this.tipo_aco : "");
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
                var vlr = value.Int().ToString();
                if (vlr.Replace("0", "") != "") { _codigo_materia_prima_sap = vlr; }
            }
        }

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
                var nbob = DBases.GetBancoRM().GetBobina(this.codigo_materia_prima_sap);
                Buffer.Bobinas.Add(nbob);
                this._bobina = nbob;
            }
        }

        public void SetLogistica(List<PLAN_PECA_LOG> logs)
        {
            this._logistica = logs;
            foreach (var s in this._logistica)
            {
                s.peca = this;
            }
        }

        public string status_sistema_pep { get; private set; } = "";
        public string centro { get; private set; } = "";
        public string Complexidade { get; set; } = "";


        public PLAN_PECA(DLM.db.Linha linha, bool orcamento = false)
        {
            if (!orcamento)
            {
                this.Tipo = Tipo_Material.Real;
                this.material = linha["material"].Valor;
                this.PEP = linha["pep"].Valor;
                if (!this.PEP.Contains("."))
                {
                    this.PEP = Conexoes.Utilz.PEP.Ajustar(this.PEP);
                }

                this.texto_breve = linha["texto_breve"].Valor;

                this.peso_unitario = linha["peso_unitario"].Double();
                this.peso_necessario = linha["peso_necessario"].Double();
                this.peso_produzido = linha["peso_produzido"].Double(6);
                this.peso_embarcado = linha["peso_embarcado"].Double();

                this.qtd_necessaria = linha["qtd_necessaria"].Double();
                this.qtd_produzida = linha["qtd_produzida"].Double();
                this.qtd_embarcada = linha["qtd_embarcada"].Double();

                this.grupo_mercadoria = linha["grupo_mercadoria"].Valor;
                this.desenho = linha["desenho"].Valor;

                this.DENOMINDSTAND = linha["DENOMINDSTAND"].Valor;

                this.inicio = linha["data_inicio"].DataNull();
                this.fim = linha["data_fim"].DataNull();
                this.DESENHO_1 = linha["desenho_1"].Valor;
                this.TIPO_DE_PINTURA = linha["tipo_de_pintura"].Valor;

                /*04/04/2019 - novas caracterísricas*/
                this.corte_largura = linha["corte_largura"].Double();
                this.comprimento = linha["comprimento"].Double();
                this.esq_de_pintura = linha["esq_de_pintura"].Valor;
                this.superficie = linha["superficie"].Double();
                this.furacoes = linha["furacoes"].Int();
                this.espessura = linha["espessura"].Double();
                this.tipo_aco = linha["tipo_aco"].Valor;
                this.codigo_materia_prima_sap = linha["codigo_materia_prima_sap"].Valor;

                if (this.desenho == "" | this.desenho == this.material)
                {
                    this.desenho = linha["marca"].Valor;
                }


                /*porcentagens*/

                this.ultima_edicao = linha["ultima_edicao"].Data();
                this.pep_cooisn = linha["pep_cooisn"].Valor;
                this.centro = linha["centro"].Valor;

                var centro = linha["centro_producao"].Valor;
                if (centro != "")
                {
                    this.centro = centro;
                }

                if (this.qtd_embarcada > 0)
                {
                    this.ULTIMO_STATUS = "PARCIALMENTE EXPEDIDO";
                }
                else if (this.qtd_necessaria > this.qtd_produzida)
                {
                    this.ULTIMO_STATUS = linha["ULTIMO_STATUS"].Valor;
                }
                else if (this.qtd_necessaria > this.qtd_embarcada)
                {
                    this.ULTIMO_STATUS = "FABRICADO";
                }
                else if (this.qtd_embarcada >= this.qtd_necessaria)
                {
                    this.ULTIMO_STATUS = "EMBARCADO";
                }
            }
            /*05/04/19*/
            else
            {
                this.Tipo = Tipo_Material.Orçamento;
                this.comprimento = linha["comp"].Double();
                this.corte_largura = linha["corte"].Double();
                this.desenho = linha["marca"].Valor;
                this.DESENHO_1 = linha["marca"].Valor;
                this.espessura = linha["esp"].Double();
                this.esq_de_pintura = linha["esquema"].Valor;
                this.furacoes = linha["furos"].Int();
                this.grupo_mercadoria = linha["grupo_mercadoria"].Valor;
                this.material = linha["marca"].Valor;
                this.PEP = linha["pep"].Valor;
                this.pep_cooisn = linha["pep"].Valor;
                this.qtd_necessaria = linha["quantidade"].Double();
                this.peso_unitario = linha["peso"].Double();
                this.peso_necessario = linha["peso"].Double() * this.qtd_necessaria;
                this.status_sistema_pep = linha["marca"].Valor;
                this.superficie = linha["superficie"].Double();
                this.texto_breve = linha["descricao"].Valor;
                this.tipo_aco = linha["tipo_aco"].Valor;
                this.TIPO_DE_PINTURA = linha["tratamento"].Valor;
                this.ultima_edicao = linha["ultima_edicao"].Data();
                this.centro = linha["unidade_fabril"].Valor;
                this.codigo_materia_prima_sap = linha["materia_prima"].Valor;
            }
        }


        //private void getComplexidade()
        //{
        //    if (DENOMINDSTAND.Replace(" ", "").StartsW("SS") | DENOMINDSTAND.Replace(" ", "").EndsWith("SS"))
        //    {
        //        Complexidade = "Super Simples";
        //    }
        //    else if (DENOMINDSTAND.Replace(" ", "").StartsW("S") | DENOMINDSTAND.Replace(" ", "").EndsWith("S"))
        //    {
        //        Complexidade = "Simples";
        //    }
        //    else if (DENOMINDSTAND.Replace(" ", "").StartsW("M") | DENOMINDSTAND.Replace(" ", "").EndsWith("M"))
        //    {
        //        Complexidade = "Média";
        //    }
        //    else if (DENOMINDSTAND.Replace(" ", "").StartsW("C") | DENOMINDSTAND.Replace(" ", "").EndsWith("C"))
        //    {
        //        Complexidade = "Complexa";
        //    }
        //    else if (DENOMINDSTAND.Replace(" ", "").StartsW("H") | DENOMINDSTAND.Replace(" ", "").EndsWith("H"))
        //    {
        //        Complexidade = "Hiper Complexa";
        //    }
        //    if (this.grupo_mercadoria.Contains("PARAF"))
        //    {
        //        this.grupo_mercadoria = "PARAFUSO";
        //    }
        //    else if (this.grupo_mercadoria.Contains("PORCA"))
        //    {
        //        this.grupo_mercadoria = "PARAFUSO";
        //    }
        //    else if (this.grupo_mercadoria.Contains("ARRU"))
        //    {
        //        this.grupo_mercadoria = "PARAFUSO";
        //    }
        //    else if (this.material.ToString().StartsW("10"))
        //    {
        //        this.grupo_mercadoria = "ALMOX NÃO FATURÁVEL";
        //    }
        //    else if (this.material.ToString().StartsW("11"))
        //    {
        //        this.grupo_mercadoria = "ALMOX";
        //    }
        //}
        public PLAN_PECA()
        {
        }

        public PLAN_PECA(PLAN_PECA_LOG ps)
        {
            this.centro = ps.centro;
            this.PEP = ps.pep;
            this.material = ps.material;
            this.desenho = ps.desenho;
            this.texto_breve = ps.descricao;
        }
    }
}
