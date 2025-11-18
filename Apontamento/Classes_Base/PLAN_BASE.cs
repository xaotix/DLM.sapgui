using Conexoes;
using DLM.db;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace DLM.painel
{
    public class PLAN_BASE
    {
        public void SetBase(DLM.db.Linha linha)
        {
            this.eini =        linha["eini"].DataNull();
            this.efim =        linha["efim"].DataNull();
            this.fini =        linha["fini"].DataNull();
            this.ffim =        linha["ffim"].DataNull();
            this.lini =        linha["lini"].DataNull();
            this.lfim =        linha["lfim"].DataNull();
            this.mini =        linha["mini"].DataNull();
            this.mfim =        linha["mfim"].DataNull();
            this.eng_base_st = linha["es"].Double();
            this.fab_base_st = linha["fs"].Double();
            this.log_base_st = linha["ls"].Double();
            this.mon_base_st = linha["ms"].Double();
        }
        public string descricao { get; set; } = "";
        private List<PLAN_ETAPA> _etapas { get; set; } = new List<PLAN_ETAPA>();
        private List<PLAN_PEP> _peps { get; set; } = new List<PLAN_PEP>();
        private List<PLAN_SUB_ETAPA> _subetapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        private List<PLAN_PEDIDO> _pedidos { get; set; } = new List<PLAN_PEDIDO>();
        private List<PLAN_PECA> _pecas { get; set; } = new List<PLAN_PECA>();

        public List<PLAN_PEDIDO> GetPedidos()
        {
            if (_pedidos.Count == 0 && this.PEP.LenghtStr() > 3)
            {
                _pedidos = DLM.painel.Consultas.GetPedidos(new List<string> { this.contrato });

                if (carregou_pecas)
                {
                    foreach (var p in _pedidos)
                    {
                        p.Set(_pecas);
                    }
                }
            }
            return _pedidos;
        }
        public List<PLAN_ETAPA> Getetapas()
        {
            if (_etapas.Count == 0 && this.PEP.LenghtStr() > 3)
            {
                _etapas = Consultas.GetEtapas(new List<string> { this.PEP });

                if (carregou_pecas)
                {
                    foreach (var e in _etapas)
                    {
                        e.Set(_pecas);
                    }
                }


            }
            return _etapas.OrderBy(x => x.etapa).ToList();
        }
        public List<PLAN_SUB_ETAPA> GetSubEtapas()
        {
            if (_subetapas.Count == 0 && this.PEP.LenghtStr() > 3)
            {
                _subetapas = new List<PLAN_SUB_ETAPA>();
                _subetapas.AddRange(this.Getetapas().SelectMany(x => x.subetapas_com_chumbacao).ToList());

                if (carregou_pecas)
                {
                    foreach (var s in _subetapas)
                    {
                        s.Set(_pecas);
                    }
                }

                if (carregou_peps)
                {
                    foreach (var s in _subetapas)
                    {
                        s.Set(_peps);
                    }
                }
            }
            return _subetapas;
        }
        public List<PLAN_PEP> GetPeps()
        {
            if (_peps.Count == 0 && this.PEP.LenghtStr() > 3)
            {
                _peps = Consultas.GetPepsReal(new List<string> { this.PEP });
            }


            if (carregou_pecas)
            {
                foreach (var s in _peps)
                {
                    s.Set(_pecas);
                }
            }

            return _peps;
        }
        public List<PLAN_PECA> GetPecas(bool reset = false)
        {
            if ((_pecas.Count == 0 | reset) && this.PEP.LenghtStr() > 3)
            {
                _pecas = Consultas.GetPecasReal(new List<string> { this.PEP });
            }


            return _pecas;
        }

        public void Set(List<PLAN_PEDIDO> lista)
        {
            this._pedidos = new List<PLAN_PEDIDO>();
            this._pedidos.AddRange(lista.FindAll(x => x.PEP.ToUpper().Contem(this.PEP)));
        }
        public void Set(List<PLAN_PECA> lista)
        {

            this._pecas = new List<PLAN_PECA>();
            if(lista.Count==0)
            {
                return;
            }

            this._pecas.AddRange(lista.FindAll(x => x.PEP.StartsW(this.PEP)));
            if (this.carregou_etapas)
            {
                foreach (var et in this.Getetapas())
                {
                    et.Set(this.GetPecas());
                }
            }
            if (this.carregou_pedidos)
            {
                foreach (var et in this.GetPedidos())
                {
                    et.Set(this.GetPecas());
                }
            }
            if (this.carregou_sub_etapas)
            {
                foreach (var et in this.GetSubEtapas())
                {
                    et.Set(this.GetPecas());
                }
            }
            if (this.carregou_peps)
            {
                foreach (var et in this.GetPeps())
                {
                    et.Set(this.GetPecas());
                }
            }


        }
        public void Set(List<PLAN_ETAPA> lista)
        {
            this._etapas = new List<PLAN_ETAPA>();
            this._etapas.AddRange(lista.FindAll(x => x.PEP.ToUpper().StartsW(this.PEP)));
        }
        public void Set(List<PLAN_PEP> lista)
        {
            this._peps = new List<PLAN_PEP>();
            this._peps.AddRange(lista.FindAll(x => x.PEP.StartsW(this.PEP)));
        }

        public bool carregou_etapas
        {
            get
            {
                return _etapas.Count > 0;
            }
        }
        public bool carregou_peps
        {
            get
            {
                return _peps.Count > 0;
            }
        }
        public bool carregou_pedidos
        {
            get
            {
                return _pedidos.Count > 0;
            }
        }
        public bool carregou_sub_etapas
        {
            get
            {
                return _subetapas.Count > 0;
            }
        }
        public bool carregou_pecas
        {
            get
            {
                return this._pecas.Count > 0;
            }
        }

        public string setor_atividade
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Setor_Atividade(this.PEP);
            }
        }
        public string etapa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Etapa(this.PEP, true);
            }
        }
        public string subetapa
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Subetapa(this.PEP, true);
            }
        }
        public string contrato
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Contrato(this.PEP);
            }
        }
        public string pedido
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Pedido(this.PEP, true);
            }
        }
        public DateTime? ultima_edicao { get; set; }
        public DateTime? criado { get; set; }
        public DateTime? engenharia_liberacao { get; set; }
        public DateTime? montagem_inicio { get; set; }
        public DateTime? montagem_fim { get; set; }

        public DateTime? mi_s { get; set; }
        public DateTime? mf_s { get; set; }
        public double logistica_previsto
        {
            get
            {
                return embarque_previsto;
            }
        }
        public double total_produzido
        {
            get
            {
                return total_fabricado;
            }
        }

        private List<Grupo_Mercadoria> _Grupos_Mercadoria { get; set; }
        public List<Grupo_Mercadoria> GetGrupos_Mercadoria()
        {

            if (_Grupos_Mercadoria == null)
            {
                _Grupos_Mercadoria = Classificadores.GetGrupo_Mercadorias(this.GetPecas());
            }
            return _Grupos_Mercadoria;
        }

        [Browsable(false)]
        public Linha Linha { get; set; } = new Linha();

        public ImageSource imagem
        {
            get
            {
                //if (this.resumo_pecas.etapa_bloqueada | this.status_montagem == "TRANCADA")
                //{
                //    return BufferImagem._lock;
                //}
                if (this is PLAN_OBRAS)
                {
                    return BufferImagem.folder_new;
                }
                else if (this is PLAN_OBRA)
                {
                    return BufferImagem.folder_red;
                }
                else if (this is PLAN_PEDIDO)
                {
                    return BufferImagem.folder_green;
                }
                else if (this is PLAN_ETAPA)
                {
                    return BufferImagem.folder;
                }
                else if (this is PLAN_ETAPA)
                {
                    return BufferImagem.folder_txt;
                }
                else if (this is PLAN_SUB_ETAPA)
                {
                    return BufferImagem.folder_bookmark;

                }

                return BufferImagem.circulo_16x16;

            }
        }
        public string PEP { get; set; } = "";

        public bool exportacao => PEP.Contem("-90");
        public string update_montagem { get; set; } = "";



        public List<Atraso_Planejamento> GetAtrasos()
        {
            var peps = this.GetSubEtapas();

            List<Atraso_Planejamento> ps = new List<Atraso_Planejamento>();

            ps.AddRange(
                        peps.FindAll(x => x.engenharia_previsto > x.liberado_engenharia)
                        .Select(y => new Atraso_Planejamento(y, Setor.ENGENHARIA)));
            ps.AddRange(
                        peps.FindAll(x => x.atraso_fabrica > 0)
                        .Select(y => new Atraso_Planejamento(y, Setor.FÁBRICA)));
            ps.AddRange(
                        peps.FindAll(x => x.atraso_embarque > 0)
                        .Select(y => new Atraso_Planejamento(y, Setor.LOGÍSTICA)));
            ps.AddRange(
                        peps.FindAll(x => x.atraso_montagem > 0)
                        .Select(y => new Atraso_Planejamento(y, Setor.MONTAGEM)));

            return ps;
        }

        private DateTime? _engenharia_cronograma { get; set; } = null;
        private DateTime? _fabrica_cronograma { get; set; } = null;
        private DateTime? _logistica_cronograma { get; set; } = null;
        private DateTime? _montagem_cronograma { get; set; } = null;
        private DateTime? _engenharia_cronograma_inicio { get; set; } = null;
        private DateTime? _fabrica_cronograma_inicio { get; set; } = null;
        private DateTime? _logistica_cronograma_inicio { get; set; } = null;
        private DateTime? _montagem_cronograma_inicio { get; set; } = null;

        public DateTime? engenharia_cronograma
        {
            get
            {
                if (_engenharia_cronograma_inicio > _engenharia_cronograma)
                {
                    return _engenharia_cronograma_inicio;
                }
                else return _engenharia_cronograma;
            }
            set
            {
                _engenharia_cronograma = value;
            }
        }
        public DateTime? fabrica_cronograma
        {
            get
            {
                if (_fabrica_cronograma_inicio > _fabrica_cronograma)
                {
                    return _fabrica_cronograma_inicio;
                }
                else return _fabrica_cronograma;
            }
            set
            {
                _fabrica_cronograma = value;
            }
        }
        public DateTime? logistica_cronograma
        {
            get
            {
                if (_logistica_cronograma_inicio > _logistica_cronograma)
                {
                    return _logistica_cronograma_inicio;
                }
                else return _logistica_cronograma;
            }
            set
            {
                _logistica_cronograma = value;
            }
        }
        public DateTime? montagem_cronograma
        {
            get
            {
                if (_montagem_cronograma_inicio > _montagem_cronograma)
                {
                    return _montagem_cronograma_inicio;
                }
                else return _montagem_cronograma;
            }
            set
            {
                _montagem_cronograma = value;
            }
        }
        public DateTime? engenharia_cronograma_inicio
        {
            get
            {
                if (_engenharia_cronograma_inicio < _engenharia_cronograma)
                {
                    return _engenharia_cronograma_inicio;
                }
                else return _engenharia_cronograma;
            }
            set
            {
                _engenharia_cronograma_inicio = value;
            }
        }
        public DateTime? fabrica_cronograma_inicio
        {
            get
            {
                if (_fabrica_cronograma_inicio < _fabrica_cronograma)
                {
                    return _fabrica_cronograma_inicio;
                }
                else return _fabrica_cronograma;
            }
            set
            {
                _fabrica_cronograma_inicio = value;
            }
        }
        public DateTime? logistica_cronograma_inicio
        {
            get
            {
                if (_logistica_cronograma_inicio < _logistica_cronograma)
                {
                    return _logistica_cronograma_inicio;
                }
                else return _logistica_cronograma;
            }
            set
            {
                _logistica_cronograma_inicio = value;
            }
        }
        public DateTime? montagem_cronograma_inicio
        {
            get
            {
                if (_montagem_cronograma_inicio < _montagem_cronograma)
                {
                    return _montagem_cronograma_inicio;
                }
                else return _montagem_cronograma;
            }
            set
            {
                _montagem_cronograma_inicio = value;
            }
        }
        public DateTime? eini { get; set; } = null;
        public DateTime? fini { get; set; } = null;
        public DateTime? lini { get; set; } = null;
        public DateTime? mini { get; set; } = null;
        public DateTime? efim { get; set; } = null;
        public DateTime? ffim { get; set; } = null;
        public DateTime? lfim { get; set; } = null;
        public DateTime? mfim { get; set; } = null;

        private double _eng_base_st { get; set; } = 0;
        public double eng_base_st
        {
            get
            {
                if (fab_base_st > _eng_base_st)
                {
                    return fab_base_st;
                }
                return _eng_base_st;
            }
            set
            {
                _eng_base_st = value;
            }
        }

        private double _fab_base_st { get; set; } = 0;
        public double fab_base_st
        {
            get
            {
                if (log_base_st > _fab_base_st)
                {
                    return log_base_st;
                }
                return _fab_base_st;
            }
            set
            {
                _fab_base_st = value;
            }
        }
        private double _log_base_st { get; set; } = 0;
        public double log_base_st
        {
            get
            {
                if (mon_base_st > _log_base_st)
                {
                    return mon_base_st;
                }
                return _log_base_st;
            }
            set
            {
                _log_base_st = value;
            }
        }

        public double mon_base_st { get; set; } = 0;




        private string _status_montagem { get; set; } = "-1";
        public string status_montagem
        {
            get
            {
                //if (this.resumo_pecas.etapa_bloqueada)
                //{
                //    return "TRANCADA";
                //}
                if (_status_montagem.Replace("-1", "") == "")
                {
                    return "SEM APONTAMENTO";
                }
                return _status_montagem;
            }
            set
            {
                _status_montagem = value;
            }

        }
        public bool dados_montagem { get; set; } = false;
        public DateTime? ultima_consulta_sap { get; set; }
        public double peso_planejado { get; set; } = 0;
        public int atraso_engenharia { get; set; } = 0;
        public int atraso_fabrica { get; set; } = 0;
        public int atraso_embarque { get; set; } = 0;
        public int atraso_montagem { get; set; } = 0;


        private double _engenharia_previsto { get; set; } = 0;
        public double engenharia_previsto
        {
            get
            {
                if (fabrica_previsto > _engenharia_previsto)
                {
                    return fabrica_previsto;
                }
                return _engenharia_previsto;
            }
            set
            {
                _engenharia_previsto = value;
            }
        }

        private double _fabrica_previsto { get; set; } = 0;
        public double fabrica_previsto
        {
            get
            {
                if (embarque_previsto > _fabrica_previsto)
                {
                    return embarque_previsto;
                }
                return _fabrica_previsto;

            }
            set
            {
                _fabrica_previsto = value;
            }
        }
        public double montagem_previsto { get; set; } = 0;
        private double _embarque_previsto { get; set; } = 0;
        public double embarque_previsto
        {
            get
            {
                if (montagem_previsto > _embarque_previsto)
                {
                    return montagem_previsto;
                }
                return _embarque_previsto;
            }
            set
            {
                _embarque_previsto = value;
            }
        }

        private double _total_embarcado { get; set; } = 0;
        public double total_embarcado
        {
            get
            {

                if (total_montado > _total_embarcado)
                {
                    return total_montado;
                }

                if (_total_embarcado > 100)
                {
                    return 100;
                }
                return _total_embarcado;
            }
            set
            {
                _total_embarcado = value;
            }
        }
        private double _total_fabricado { get; set; } = 0;
        public double total_fabricado
        {
            get
            {
                if (total_embarcado > _total_fabricado)
                {
                    return total_embarcado;
                }

                if (_total_fabricado > 100)
                {
                    return 100;
                }
                return _total_fabricado;
            }
            set
            {
                _total_fabricado = value;
            }
        }
        private double _total_montado { get; set; } = 0;
        public double total_montado
        {
            get
            {
                if ((status_montagem == "CONCLUÍDA" | status_montagem == "ENTREGUE") && this is PLAN_PEDIDO)
                {
                    return 100;
                }
                if (_total_montado > 100)
                {
                    return 100;
                }
                return _total_montado;
            }
            set
            {
                _total_montado = value;
            }
        }
        public double peso_produzido { get; set; } = 0;
        public double peso_embarcado { get; set; } = 0;
        public double peso_montado { get; set; } = 0;
        public double liberado_engenharia
        {
            get
            {
                if (total_fabricado > _liberado_engenharia)
                {
                    return total_fabricado;
                }
                if (_liberado_engenharia > 100)
                {
                    return 100;
                }
                return _liberado_engenharia;
            }
            set
            {
                _liberado_engenharia = value;
            }
        }
        private double _liberado_engenharia { get; set; } = 0;





        public PLAN_BASE()
        {

        }
    }
}
