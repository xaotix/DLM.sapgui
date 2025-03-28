﻿using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.sapgui
{

    public class PEP_Planejamento : Notificar
    {
        public CN47N Engenharia { get; set; } = new CN47N();
        public CN47N Montagem { get; set; } = new CN47N();
        public CN47N Fabrica { get; set; } = new CN47N();
        public List<CN47N> LogisticaDatas { get; set; } = new List<CN47N>();
        public ConexaoSAP Pedido { get; set; } = new ConexaoSAP();

        public List<ZPP0100> Embarque { get; set; } = new List<ZPP0100>();
        public List<ZPMP> Producao { get; set; } = new List<ZPMP>();
        private long _id { get; set; } = -1;
        public long id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                NotifyPropertyChanged("id");
            }
        }

        private string _Codigo { get; set; } = "";
        public string Codigo
        {
            get
            {
                return _Codigo;
            }
            set
            {
                _Codigo = value;
                NotifyPropertyChanged("Codigo");
            }
        }

        private string _Observacoes { get; set; } = "";
        public string Observacoes
        {
            get
            {
                if(_Observacoes!="") return _Observacoes;
                GetObservacoes();
                return _Observacoes;
            }
            set
            {
                _Observacoes = value;
                NotifyPropertyChanged("Observacoes");
            }
        }
        private string _Denominacao { get; set; } = "";
        public string Denominacao
        {
            get
            {
                return _Denominacao;
            }
            set
            {
                _Denominacao = value;
                NotifyPropertyChanged("Denominacao");
            }
        }

        private string _Descricao { get; set; } = "";
        public string Descricao
        {
            get
            {
                return _Descricao;
            }
            set
            {
                _Descricao = value;
                NotifyPropertyChanged("Descricao");
            }
        }
        private double _Peso_Planejado { get; set; } = 0;
        public double Peso_Planejado
        {
            get
            {
                return _Peso_Planejado;
            }
            set
            {
                _Peso_Planejado = value;
                NotifyPropertyChanged("Peso_Planejado");
            }
        }
        private double _Peso_Produzido { get; set; } = 0;
        public double Peso_Produzido
        {
            get
            {
                return _Peso_Produzido;
            }
            set
            {
                _Peso_Produzido = value;
                NotifyPropertyChanged("Peso_Produzido");
            }
        }
        private double _Peso_Embarcado { get; set; } = 0;
        public double Peso_Embarcado
        {
            get
            {
                return _Peso_Embarcado;
            }
            set
            {
                _Peso_Embarcado = value;
                NotifyPropertyChanged("Peso_Embarcado");
            }
        }
        //private double _Peso_A_Embarcar { get; set; } = 0;
        public double Peso_A_Embarcar
        {
            get
            {
                //return _Peso_A_Embarcar;
                return this.Peso_Planejado - this.Peso_Embarcado;
            }
            //set
            //{
            //    _Peso_A_Embarcar = value;
            //    NotifyPropertyChanged("Peso_A_Embarcar");
            //}
        }
        //private double _Falta_Produzir { get; set; } = 0;
        public double Falta_Produzir
        {
            get
            {
                //return _Falta_Produzir;
                return Peso_Planejado - Peso_Produzido; 
            }
            //set
            //{
            //    _Falta_Produzir = value;
            //    NotifyPropertyChanged("Falta_Produzir");
            //}
        }

        public double Porcentagem_Produzida
        {
            get
            {
                var T= Peso_Produzido / Peso_Planejado;
                if (T > 1)
                {
                    return 1;
                }
                else return T;
            }
        }
        public double Porcentagem_Embarcada
        {
            get
            {
                var T = Peso_Embarcado / Peso_Planejado;
                if (T > 1)
                {
                    return 1;
                }
                else return T;
            }
        }

        private DateTime _Engenharia_Cronograma { get; set; } = Cfg.Init.DataDummy;

        public DateTime Engenharia_Cronograma
        {
            get
            {
                return _Engenharia_Cronograma;
            }
            set
            {
                _Engenharia_Cronograma = value;
                NotifyPropertyChanged("Engenharia_Cronograma");
            }
        }

        private DateTime _Engenharia_Cronograma_Inicio { get; set; } = Cfg.Init.DataDummy;

        public DateTime Engenharia_Cronograma_Inicio
        {
            get
            {
                return _Engenharia_Cronograma_Inicio;
            }
            set
            {
                _Engenharia_Cronograma_Inicio = value;
                NotifyPropertyChanged("Engenharia_Cronograma_Inicio");
            }
        }

        private DateTime _Engenharia_Liberacao { get; set; } = Cfg.Init.DataDummy;

        public DateTime Engenharia_Liberacao
        {
            get
            {
                return _Engenharia_Liberacao;
            }
            set
            {
                _Engenharia_Liberacao = value;
                NotifyPropertyChanged("Engenharia_Liberacao");
            }
        }

        private DateTime _Fabrica_Cronograma { get; set; } = Cfg.Init.DataDummy;

        public DateTime Fabrica_Cronograma
        {
            get
            {
                return _Fabrica_Cronograma;
            }
            set
            {
                _Fabrica_Cronograma = value;
                NotifyPropertyChanged("Fabrica_Cronograma");
            }
        }

        private DateTime _Fabrica_Cronograma_Inicio { get; set; } = Cfg.Init.DataDummy;

        public DateTime Fabrica_Cronograma_Inicio
        {
            get
            {
                return _Fabrica_Cronograma_Inicio;
            }
            set
            {
                _Fabrica_Cronograma_Inicio = value;
                NotifyPropertyChanged("Fabrica_Cronograma_Inicio");
            }
        }

        private DateTime _Logistica_Cronograma { get; set; } = Cfg.Init.DataDummy;

        public DateTime Logistica_Cronograma
        {
            get
            {
                return _Logistica_Cronograma;
            }
            set
            {
                _Logistica_Cronograma = value;
                NotifyPropertyChanged("Logistica_Cronograma");
            }
        }

        private DateTime _Logistica_Cronograma_Inicio { get; set; } = Cfg.Init.DataDummy;

        public DateTime Logistica_Cronograma_Inicio
        {
            get
            {
                return _Logistica_Cronograma_Inicio;
            }
            set
            {
                _Logistica_Cronograma_Inicio = value;
                NotifyPropertyChanged("Logistica_Cronograma_Inicio");
            }
        }

        private DateTime _Montagem_Cronograma { get; set; } = Cfg.Init.DataDummy;

        public DateTime Montagem_Cronograma
        {
            get
            {
                return _Montagem_Cronograma;
            }
            set
            {
                _Montagem_Cronograma = value;
                NotifyPropertyChanged("Montagem_Cronograma");
            }
        }

        private DateTime _Montagem_Cronograma_Inicio { get; set; } = Cfg.Init.DataDummy;

        public DateTime Montagem_Cronograma_Inicio
        {
            get
            {
                return _Montagem_Cronograma_Inicio;
            }
            set
            {
                _Montagem_Cronograma_Inicio = value;
                NotifyPropertyChanged("Montagem_Cronograma_Inicio");
            }
        }


        public DateTime eng_base_ini { get; set; } = Cfg.Init.DataDummy;
        public DateTime fab_base_ini { get; set; } = Cfg.Init.DataDummy;
        public DateTime log_base_ini { get; set; } = Cfg.Init.DataDummy;
        public DateTime mon_base_ini { get; set; } = Cfg.Init.DataDummy;

        public DateTime eng_base_fim { get; set; } = Cfg.Init.DataDummy;
        public DateTime fab_base_fim { get; set; } = Cfg.Init.DataDummy;
        public DateTime log_base_fim { get; set; } = Cfg.Init.DataDummy;
        public DateTime mon_base_fim { get; set; } = Cfg.Init.DataDummy;


        private double _Produzido { get; set; } = 0;
        public double Produzido
        {
            get
            {
                return _Produzido;
            }
            set
            {
                _Produzido = value;
                NotifyPropertyChanged("Produzido");
            }
        }

        private double _Embarcado { get; set; } = 0;
        public double Embarcado
        {
            get
            {
                return _Embarcado;
            }
            set
            {
                _Embarcado = value;
                NotifyPropertyChanged("Embarcado");
            }
        }

        public PEP_Planejamento()
        {

        }
        public PEP_Planejamento(string Codigo)
        {
            this.Codigo = Codigo;
            AjustaPEP();
        }

        private void AjustaPEP()
        {
            // 20/10/2020
            //TRATAMENTO QUE ALTERA QUALQUER LETRA DA UNIDADE FABRIL PRA 'F' 
            // PRA RESOLVER O PROBLEMA DE MERDA OS CADASTRO DE PEP NA 
            //LOGISTICA QUE FICAM FAZENDO TIGRADA DE BOTAR NOMES NÃO PADRÃO
            var PEP = CargaExcel.TratarPEP(this.Codigo);
            this.Codigo = PEP;
        }

        public PEP_Planejamento(string Codigo, List<ZPMP> Producao,List<ZPP0100> Embarque, CN47N Fabrica, ConexaoSAP Pedido)
        {
            this.Codigo = Codigo;
            this.AjustaPEP();
            this.Producao = Producao;

            this.Embarque = Embarque;
            this.Fabrica = Fabrica;
            if (this.Fabrica == null)
            {
                this.Fabrica = new CN47N();
            }
            this.Pedido = Pedido;

            this.Peso_Planejado = this.Producao.Sum(x => x.Peso_Nec);
            this.Peso_Produzido = this.Producao.Sum(x => x.Peso_Fab);


            this.Engenharia = Pedido.GETPEPENG(this.Codigo);

            /*14/11/18*/
            /*esses caras eu to pegando do cn47n*/
            this.Montagem = Pedido.GETPEPMONT(this.Codigo);

            LogisticaDatas = Pedido.GETPEPSLOG(this.Codigo);

            if (this.Engenharia != null)
            {
                this.Descricao = this.Engenharia.Texto_Operacao;

                this.Engenharia_Cronograma_Inicio = this.Engenharia.Data_Inicio_Base.GetValue();
                this.Engenharia_Cronograma = this.Engenharia.Data_Fim_Base.GetValue();

                this.eng_base_ini = this.Engenharia.Inicio_Previsto.GetValue();
                this.eng_base_fim = this.Engenharia.Fim_Previsto.GetValue();
            }
            if (this.Fabrica != null)
            {
                this.Fabrica_Cronograma_Inicio = this.Fabrica.Data_Inicio_Base.GetValue();
                this.Fabrica_Cronograma = this.Fabrica.Data_Fim_Base.GetValue();

                this.fab_base_ini = this.Fabrica.Inicio_Previsto.GetValue();
                this.fab_base_fim = this.Fabrica.Fim_Previsto.GetValue();
            }
            if (LogisticaDatas.Count > 0)
            {

                this.Logistica_Cronograma_Inicio = LogisticaDatas.Min(x => x.Data_Inicio_Base).GetValue();
                this.Logistica_Cronograma = LogisticaDatas.Max(x => x.Data_Fim_Base).GetValue();

                this.log_base_ini = LogisticaDatas.Max(x => x.Inicio_Previsto).GetValue();
                this.log_base_fim = LogisticaDatas.Max(x => x.Fim_Previsto).GetValue();
            }
            if (this.Montagem !=null)
            {
                this.Montagem_Cronograma_Inicio = Montagem.Data_Inicio_Base.GetValue();
                this.Montagem_Cronograma = Montagem.Data_Fim_Base.GetValue();

                this.mon_base_ini = this.Montagem.Inicio_Previsto.GetValue();
                this.mon_base_fim = this.Montagem.Fim_Previsto.GetValue();
            }     

            foreach (var t in this.Embarque.FindAll(x => x.Carregado))
            {
                var s = Producao.Find(x => x.Material == t.Material);
                if (s != null)
                {
                    this.Peso_Embarcado = this.Peso_Embarcado + (s.Peso_Nec / s.Qtd_Nec * t.Qtd_Carregada);
                }
            }

            this.Peso_Embarcado = this.Peso_Embarcado / 1000;
            this.Peso_Planejado = this.Peso_Planejado / 1000;
            this.Peso_Produzido = this.Peso_Produzido / 1000;

            GetObservacoes();
        }

        private void GetObservacoes()
        {
            if (this.Porcentagem_Produzida > 0.99 && this.Porcentagem_Embarcada > 0.99)
            {
                this._Observacoes = "TOTALMENTE EMBARCADA";
            }
            else if (this.Porcentagem_Produzida > 0 && this.Porcentagem_Embarcada == 0)
            {
                this._Observacoes = "EM PRODUÇÃO";
            }
            else if (this.Porcentagem_Produzida == 0 && this.Porcentagem_Embarcada == 0)
            {
                this._Observacoes = "À PRODUZIR";
            }
            else if (this.Porcentagem_Produzida == 1 && this.Porcentagem_Embarcada > 0)
            {
                this._Observacoes = "ETAPA PARCIALMENTE EMBARCADA";
            }
            else if (this.Porcentagem_Produzida == 1 && this.Porcentagem_Embarcada == 1)
            {
                this._Observacoes = "ETAPA COMPLETA NO PÁTIO";
            }
        }
    }
}
