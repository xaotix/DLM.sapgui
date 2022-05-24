﻿using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Media;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DLM.painel
{
    public class PLAN_PEP :PLAN_BASE
    {
        public string fabrica
        {
            get
            {
                if(this.pep.Length>2)
                {
                    return this.pep.Substring(this.pep.Length - 2, 2);
                }
                return "";
            }
        }
        public override string ToString()
        {
            return pep;
        }
        public long id { get; set; } = -1;
        public long id_status_pep { get; set; } = -1;
        public long id_status_pedido { get; set; } = -1;
        public string pep_engenharia { get; set; } = "";
        public string status_eng { get; set; } = "";
        public string status { get; set; } = "";
        public string status_log { get; set; } = "";
        public string data_eng
        {
            get
            {
             if(_data_eng!="")
                {
                    return _data_eng;
                }
             else if(status_eng.Contains("CONF"))
                {
                    return "LIBERADO";
                }
                else if (status_eng.Contains("NÃO"))
                {
                    return status_eng;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _data_eng = value;
            }
        }
        private string _data_eng { get; set; } = "";
        public string data_fab
        {
            get
            {
                if (_data_fab != "")
                {
                    return _data_fab;
                }
                else if(porcentagem_produzida>99.8)
                {
                    return "FINALIZADO";
                }
                else if(pep.Contains(".FO"))
                {
                    return "TELHA EM OBRA";
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _data_fab = value;
            }
        }
        private string _data_fab    { get; set; } = "";
        public string observacoes
        {
            get
            {
                if(_observacoes=="")
                {
                    GetObservacoes();
                }
                return _observacoes;
            }
            set
            {
                _observacoes = value;
            }
        }
        private void GetObservacoes()
        {
            if(this.resumo_pecas.etapa_bloqueada)
            {
                this._observacoes = "ETAPA BLOQUEADA";
            }
            else if (this.porcentagem_produzida > 99 && this.porcentagem_embarcada > 99)
            {
                this._observacoes = "TOTALMENTE EMBARCADA";
            }
            else if (this.porcentagem_produzida > 0 && this.porcentagem_embarcada == 0)
            {
                this._observacoes = "EM PRODUÇÃO";
            }
            else if (this.porcentagem_produzida == 0 && this.porcentagem_embarcada == 0)
            {
                this._observacoes = "À PRODUZIR";
            }
            else if (this.porcentagem_produzida == 100 && this.porcentagem_embarcada > 0)
            {
                this._observacoes = "ETAPA PARCIALMENTE EMBARCADA";
            }
            else if (this.porcentagem_produzida == 100 && this.porcentagem_embarcada == 100)
            {
                this._observacoes = "ETAPA COMPLETA NO PÁTIO";
            }
        }
        private string _observacoes { get; set; } = "";
        public double peso_a_embarcar
        {
            get
            {
               var t =  peso_planejado - peso_embarcado;
                if (t > 0)
                {
                    return t;
                }
                return 0;
            }
        }
        public double falta_produzir
        {
            get
            {
                var t = peso_planejado - peso_produzido;
                if(t>0)
                {
                    return t;
                }
                return 0;
            }
        }
        private double _porcentagem_produzida = -1;
        public double porcentagem_produzida
        {
            get
            {
  if(_porcentagem_produzida<0)
                {
                    double resultado = 0;
                    var t0 = peso_produzido;
                    var t1 = peso_planejado;
                    if (t0 > 0 && t1 > 0)
                    {
                        resultado = t0 / t1;
                        if (resultado > 1)
                        {
                            resultado = 1;
                        }
                        if(resultado < 0)
                        {
                            resultado = 0;
                        }
                    }
                    _porcentagem_produzida = resultado * 100;
                   
                }
                return _porcentagem_produzida;
            }
        }
        private double _porcentagem_embarcada { get; set; } = -1;
        public double porcentagem_embarcada
        {
            get
            {
              if(_porcentagem_embarcada==-1)
                {
                    double resultado = 0;
                    var t0 = peso_embarcado;
                    var t1 = peso_planejado;
                    if (t0 > 0 && t1 > 0)
                    {
                        resultado = t0 / t1;
                        if (resultado > 1)
                        {
                            resultado = 1;
                        }
                        if (resultado < 0)
                        {
                            resultado = 0;
                        }
                    }
                    _porcentagem_embarcada = resultado * 100;
                }
                return _porcentagem_embarcada;
            }
        }
        public string Pedido
        {
            get
            {
                if(pep.Length>13)
                {
                    return pep.Substring(0, 13);
                }
                return "";
            }
        }
        public string Contrato
        {
            get
            {
                if (pep.Length > 9)
                {
                    return pep.Substring(3, 6);
                }
                return "";
            }
        }

        public string Contrato_Completo
        {
            get
            {
                if (pep.Length > 9)
                {
                    return pep.Substring(0, 9);
                }
                return "";
            }
        }
        public void Ler(bool carrega_id = true)
        {
            this.data_eng = L.Get("data_eng").ToString();
            this.data_fab = L.Get("data_fab").ToString();

            this.engenharia_liberacao = L.Get("engenharia_liberacao").Data();

            this.engenharia_cronograma = L.Get("engenharia_cronograma").Data();
            this.fabrica_cronograma = L.Get("fabrica_cronograma").Data();
            this.logistica_cronograma = L.Get("logistica_cronograma").Data();
            this.montagem_cronograma = L.Get("montagem_cronograma").Data();

            this.engenharia_cronograma_inicio = L.Get("engenharia_cronograma_inicio").Data();
            this.fabrica_cronograma_inicio = L.Get("fabrica_cronograma_inicio").Data();
            this.logistica_cronograma_inicio = L.Get("logistica_cronograma_inicio").Data();
            this.montagem_cronograma_inicio = L.Get("montagem_cronograma_inicio").Data();

            if (carrega_id)
            {

            this.id = L["id"].Int();
            }
            this.id_status_pedido = L.Get("id_status_pedido").Int();
            this.id_status_pep = L.Get("id_status_pedido").Int();

            this.observacoes = L["observacoes"].Valor;
            this.pep = L.Get("pep").ToString();
            this.pep_engenharia = L.Get("pep_engenharia").ToString();

            this.peso_embarcado = L.Get("peso_embarcado").Double(6);
            this.peso_planejado = L.Get("peso_planejado").Double(6);
            this.peso_produzido = L.Get("peso_produzido").Double(6);

            this.ultima_consulta_sap = L.Get("ultima_consulta_sap").Data();

            this.status_eng = L.Get("status_eng").ToString();
            this.status = L.Get("status").ToString();


            this.Titulo = new Titulo_Planejamento() { CHAVE = this.pep, DESCRICAO = this.descricao };

        }
        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha l = new DLM.db.Linha();
            var data_min = Conexoes.Utilz.Calendario.DataDummy();

            l.Add("id_status_pep", id_status_pep);
            l.Add("id_status_pedido", id_status_pedido);
            l.Add("pep", pep);
            l.Add("pep_engenharia", pep_engenharia);

            l.Add("peso_planejado", Math.Round(peso_planejado, 3));
            l.Add("peso_produzido", Math.Round(peso_produzido, 3));
            l.Add("peso_embarcado", Math.Round(peso_embarcado, 3));

            if (engenharia_liberacao > data_min)
            {
                l.Add("engenharia_liberacao", engenharia_liberacao);
            }
            if (engenharia_cronograma > data_min)
            {
                l.Add("engenharia_cronograma", engenharia_cronograma);
            }
            if (fabrica_cronograma > data_min)
            {
                l.Add("fabrica_cronograma", fabrica_cronograma);
            }

            if (status_eng!="")
            {
            l.Add("status_eng", status_eng);
            }
            if(status!="")
            {
            l.Add("status", status);
            }

            if(data_eng!="")
            {
            l.Add("data_eng", data_eng);

            }
            if(data_fab!="")
            {
            l.Add("data_fab", data_fab);
            }

            if (observacoes!="")
            {
                l.Add("observacoes", observacoes);
            }


            if (logistica_cronograma > data_min)
            {

                l.Add("logistica_cronograma", logistica_cronograma);
            }
            if (montagem_cronograma > data_min)
            {

                l.Add("montagem_cronograma", montagem_cronograma);
            }


            //l.Add("descricao", descricao);

            if (engenharia_cronograma_inicio > data_min)
            {

                l.Add("engenharia_cronograma_inicio", engenharia_cronograma_inicio);
            }
            if (fabrica_cronograma_inicio > data_min)
            {
                l.Add("fabrica_cronograma_inicio", fabrica_cronograma_inicio);

            }
            if (logistica_cronograma_inicio > data_min)
            {
                l.Add("logistica_cronograma_inicio", logistica_cronograma_inicio);

            }
            if (montagem_cronograma_inicio > data_min)
            {
                l.Add("montagem_cronograma_inicio", montagem_cronograma_inicio);
            }

            if (eini > data_min)
            {
                l.Add("eng_base_ini", eini);
            }

            if (efim > data_min)
            {
                l.Add("eng_base_fim", efim);
            }

            if (fini > data_min)
            {
                l.Add("fab_base_ini", fini);
            }

            if (ffim > data_min)
            {
                l.Add("fab_base_fim", ffim);
            }

            if (lini > data_min)
            {
                l.Add("log_base_ini", lini);
            }

            if (lfim > data_min)
            {
                l.Add("log_base_fim", lfim);
            }

            if (mini > data_min)
            {
                l.Add("mon_base_ini", mini);
            }

            if (mfim > data_min)
            {
                l.Add("mon_base_fim", mfim);
            }

            l.Add("ultima_consulta_sap", DateTime.Now);

            return l;
        }
        public PLAN_PEP(DLM.db.Linha L)
        {
            //this.banco = banco;
            this.L = L;
            Ler(true);
        }
        public PLAN_PEP(List<PLAN_PECA> pecas, string observacoes, string descricao)
        {
            this.Set(pecas);
            this.observacoes = observacoes;
            if(pecas.Count>0)
            {
                this.pep = pecas[0].pep;
            }
            //this.banco = Conexoes.DBases.GetDB();
        }

        public PLAN_PEP()
        {

        }
    }
}