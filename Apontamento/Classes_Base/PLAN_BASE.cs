using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DLM.painel
{
    public class PLAN_BASE
    {
        public DateTime mindia { get; set; } = Cfg.Init.DataDummy();
        public void SetBase(DLM.db.Linha l)
        {
            this.eini = l.Get("eini").Data();
            this.efim = l.Get("efim").Data();
            this.fini = l.Get("fini").Data();
            this.ffim = l.Get("ffim").Data();
            this.lini = l.Get("lini").Data();
            this.lfim = l.Get("lfim").Data();
            this.mini = l.Get("mini").Data();
            this.mfim = l.Get("mfim").Data();
            this.eng_base_st = l.Get("es").Double();
            this.fab_base_st = l.Get("fs").Double();
            this.log_base_st = l.Get("ls").Double();
            this.mon_base_st = l.Get("ms").Double();
        }
        public string descricao
        {
            get
            {
                return this.PEP + (this.Titulo.DESCRICAO != "" ? (" - " + this.Titulo.DESCRICAO) : "");
            }
        }
        private List<PLAN_ETAPA> _etapas { get; set; } = new List<PLAN_ETAPA>();
        private List<PLAN_PEP> _peps { get; set; } = new List<PLAN_PEP>();
        private List<PLAN_SUB_ETAPA> _subetapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        private List<PLAN_PEDIDO> _pedidos { get; set; } = new List<PLAN_PEDIDO>();
        private List<PLAN_PECA> _pecas { get; set; } = new List<PLAN_PECA>();

        public List<PLAN_PEDIDO> GetPedidos()
        {
            if (_pedidos.Count == 0 && this.PEP.Length > 3)
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
            if (_etapas.Count == 0 && this.PEP.Length > 3)
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
            if (_subetapas.Count == 0 && this.PEP.Length > 3)
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
            if (_peps.Count == 0 && this.PEP.Length > 3)
            {
                _peps = Consultas.GetPeps(new List<string> { this.PEP });
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
            if ((_pecas.Count == 0 | reset) && this.PEP.Length > 3)
            {
                _pecas = Consultas.GetPecasReal(new List<string> { this.PEP });
            }


            return _pecas;
        }
        public void Set(List<Titulo_Planejamento> titulos, bool contrato = false)
        {
            if (contrato)
            {
                this.Titulo = titulos.Find(x => x.CHAVE == this.contrato);
            }
            else
            {
                this.Titulo = titulos.Find(x => x.CHAVE == this.PEP);
            }

            if(this.Titulo==null)
            {

            }
        }
        public void Set(List<PLAN_PEDIDO> lista)
        {
            this._pedidos = new List<PLAN_PEDIDO>();
            this._pedidos.AddRange(lista.FindAll(x => x.PEP.ToUpper().Contains(this.PEP)));
        }
        public void Set(List<PLAN_PECA> lista)
        {
            this._pecas = new List<PLAN_PECA>();
            this._pecas.AddRange(lista.FindAll(x => x.PEP.ToUpper().StartsWith(this.PEP)));
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
            this._etapas.AddRange(lista.FindAll(x => x.PEP.ToUpper().StartsWith(this.PEP)));
        }
        public void Set(List<PLAN_SUB_ETAPA> lista)
        {
            this._subetapas = new List<PLAN_SUB_ETAPA>();
            this._subetapas.AddRange(lista.FindAll(x => x.PEP.ToUpper().StartsWith(this.PEP)));
        }
        public void Set(List<PLAN_PEP> lista)
        {
            this._peps = new List<PLAN_PEP>();
            this._peps.AddRange(lista.FindAll(x => x.PEP.StartsWith(this.PEP)));
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
        public DateTime? ultima_edicao { get; set; } = Cfg.Init.DataDummy();
        public DateTime? criado { get; set; } = Cfg.Init.DataDummy();
        public DateTime? engenharia_liberacao { get; set; } = Cfg.Init.DataDummy();
        public DateTime? montagem_inicio { get; set; } = Cfg.Init.DataDummy();
        public DateTime? montagem_fim { get; set; } = Cfg.Init.DataDummy();

        public DateTime? mi_s { get; set; } = Cfg.Init.DataDummy();
        public DateTime? mf_s { get; set; } = Cfg.Init.DataDummy();
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
        public double engenharia_peso_atraso
        {
            get
            {
                var t = Math.Round((peso_planejado * engenharia_previsto / 100) - (peso_planejado * liberado_engenharia / 100), 2);
                if (t > 0)
                {
                    return Math.Round(t, 2);
                }
                return 0;
            }
        }
        public double fabrica_peso_atraso
        {
            get
            {
                var t = Math.Round((peso_planejado * fabrica_previsto / 100) - (peso_planejado * total_fabricado / 100), 2);
                if (t > 0)
                {
                    return Math.Round(t, 2);
                }
                return 0;
            }
        }
        public double embarque_peso_atraso
        {
            get
            {
                var t = Math.Round((peso_planejado * embarque_previsto / 100) - (peso_planejado * total_embarcado / 100), 2);
                if (t > 0)
                {
                    return Math.Round(t, 2);
                }
                return 0;
            }
        }
        public double montagem_peso_atraso
        {
            get
            {
                if (exportacao | this.status_montagem == "SEM APONTAMENTO")
                {
                    return 0;
                }
                var t = Math.Round((peso_planejado * montagem_previsto / 100) - (peso_planejado * total_montado / 100), 2);
                if (t > 0)
                {
                    return Math.Round(t, 2);
                }
                return 0;
            }
        }
        public DLM.db.Linha L { get; set; } = new DLM.db.Linha();

        public ImageSource imagem
        {
            get
            {
                if (this.resumo_pecas.etapa_bloqueada | this.status_montagem == "TRANCADA")
                {
                    return Conexoes.BufferImagem._lock;
                }
                if (this is PLAN_OBRAS)
                {
                    return Conexoes.BufferImagem.folder_new;
                }
                else if (this is PLAN_OBRA)
                {
                    return Conexoes.BufferImagem.folder_red;
                }
                else if (this is PLAN_PEDIDO)
                {
                    return Conexoes.BufferImagem.folder_green;
                }
                else if (this is PLAN_ETAPA)
                {
                    return Conexoes.BufferImagem.folder;
                }
                else if (this is PLAN_ETAPA)
                {
                    return Conexoes.BufferImagem.folder_txt;
                }
                else if (this is PLAN_SUB_ETAPA)
                {
                    return Conexoes.BufferImagem.folder_bookmark;

                }

                return Conexoes.BufferImagem.circulo_16x16;

            }
        }
        public string PEP { get; set; } = "";
        public System.Windows.Visibility montagem_visivel
        {
            get
            {
                if (this.status_montagem == "SEM APONTAMENTO")
                {
                    return System.Windows.Visibility.Collapsed;
                }
                return System.Windows.Visibility.Visible;
            }
        }
        public System.Windows.Visibility montagem_visivel_grid
        {
            get
            {
                if (exportacao && this.status_montagem == "SEM APONTAMENTO")
                {
                    return System.Windows.Visibility.Collapsed;
                }
                return System.Windows.Visibility.Visible;
            }
        }
        public bool exportacao
        {
            get
            {
                return PEP.Contains("-90");
            }
        }
        public string update_montagem { get; set; } = "";
        private Titulo_Planejamento _Titulo { get; set; } = new Titulo_Planejamento();
        public Titulo_Planejamento Titulo
        {
            get
            {
                if (_Titulo == null) { _Titulo = new Titulo_Planejamento() { CHAVE = this.PEP }; }
                return _Titulo;
            }
            set
            {
                _Titulo = value;
            }
        }


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

        private DateTime? _engenharia_cronograma { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _fabrica_cronograma { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _logistica_cronograma { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _montagem_cronograma { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _engenharia_cronograma_inicio { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _fabrica_cronograma_inicio { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _logistica_cronograma_inicio { get; set; } = Cfg.Init.DataDummy();
        private DateTime? _montagem_cronograma_inicio { get; set; } = Cfg.Init.DataDummy();

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
        public DateTime? eini { get; set; } = Cfg.Init.DataDummy();
        public DateTime? fini { get; set; } = Cfg.Init.DataDummy();
        public DateTime? lini { get; set; } = Cfg.Init.DataDummy();
        public DateTime? mini { get; set; } = Cfg.Init.DataDummy();
        public DateTime? efim { get; set; } = Cfg.Init.DataDummy();
        public DateTime? ffim { get; set; } = Cfg.Init.DataDummy();
        public DateTime? lfim { get; set; } = Cfg.Init.DataDummy();
        public DateTime? mfim { get; set; } = Cfg.Init.DataDummy();

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

        public bool cron_eng_show { get; set; } = true;
        public bool cron_fab_show { get; set; } = true;
        public bool cron_log_show { get; set; } = true;
        public bool cron_mont_show { get; set; } = true;
        public DateTime? inicio_cronograma_filtro
        {
            get
            {
                var t = datas_cronograma_filtro.FindAll(x => x > Cfg.Init.DataDummy());
                if (t.Count > 0)
                {
                    return t.Min();
                }
                return DateTime.Now.AddMonths(-3);
            }
        }
        public DateTime? fim_cronograma_filtro
        {
            get
            {
                var t = datas_cronograma_filtro.FindAll(x => x != Cfg.Init.DataDummy());
                if (t.Count > 0)
                {
                    return t.Max();
                }
                return Cfg.Init.DataDummy();
            }
        }
        public List<DateTime> datas_cronograma_filtro
        {
            get
            {
                List<DateTime> retorno = new List<DateTime>();
                if (cron_eng_show)
                {
                    retorno.Add((DateTime)engenharia_cronograma);
                    retorno.Add((DateTime)engenharia_cronograma_inicio);
                }
                if (cron_fab_show)
                {
                    retorno.Add((DateTime)fabrica_cronograma);
                    retorno.Add((DateTime)fabrica_cronograma_inicio);
                }
                if (cron_log_show)
                {
                    retorno.Add((DateTime)logistica_cronograma);
                    retorno.Add((DateTime)logistica_cronograma_inicio);
                }
                if (cron_mont_show)
                {
                    retorno.Add((DateTime)montagem_cronograma);
                    retorno.Add((DateTime)montagem_cronograma_inicio);
                }
                return retorno;
            }
        }
        public DateTime? inicio_cronograma
        {
            get
            {
                var t = datas_cronograma.FindAll(x => x > Cfg.Init.DataDummy());
                if (t.Count > 0)
                {
                    return t.Min();
                }
                return DateTime.Now.AddMonths(-3);
            }
        }
        public DateTime? fim_cronograma
        {
            get
            {
                var t = datas_cronograma.FindAll(x => x != Cfg.Init.DataDummy());
                if (t.Count > 0)
                {
                    return t.Max();
                }
                return Cfg.Init.DataDummy();
            }
        }
        public List<DateTime?> datas_cronograma
        {
            get
            {

                return new List<DateTime?>
                {
                    engenharia_cronograma,
                    engenharia_cronograma_inicio,
                    fabrica_cronograma,
                    fabrica_cronograma_inicio,
                    logistica_cronograma,
                    logistica_cronograma_inicio,
                    montagem_cronograma,
                    montagem_cronograma_inicio
                };
            }
        }
        public Resumo_Pecas resumo_pecas { get; set; } = new Resumo_Pecas();
        private string _status_montagem { get; set; } = "-1";
        public string status_montagem
        {
            get
            {
                if (this.resumo_pecas.etapa_bloqueada)
                {
                    return "TRANCADA";
                }
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
        public DateTime? ultima_consulta_sap { get; set; } = Cfg.Init.DataDummy();
        public double peso_planejado { get; set; } = 0;
        public int atraso_engenharia { get; set; } = 0;
        public int atraso_fabrica { get; set; } = 0;
        public int atraso_embarque { get; set; } = 0;
        public int atraso_montagem { get; set; } = 0;
        public ImageSource imagem_engenharia
        {
            get
            {
                if (resumo_pecas.etapa_bloqueada)
                {
                    return Conexoes.BufferImagem._lock;
                }

                if (liberado_engenharia == 100)
                {
                    return Vars.Imagens.engenharia_32x32_verde;
                }
                if (atraso_engenharia > 0)
                {
                    return Vars.Imagens.engenharia_32x32_vermelho;
                }
                if (liberado_engenharia > 0)
                {
                    return Vars.Imagens.engenharia_32x32_azul;
                }
                return Vars.Imagens.engenharia_32x32;
            }
        }
        public ImageSource imagem_embarque
        {
            get
            {
                if (resumo_pecas.etapa_bloqueada)
                {
                    return Conexoes.BufferImagem._lock;
                }
                if (total_embarcado == 100)
                {
                    return Vars.Imagens.embarque_32x32_verde;
                }
                if (atraso_embarque > 0)
                {
                    return Vars.Imagens.embarque_32x32_vermelho;
                }
                if (total_embarcado > 0)
                {
                    return Vars.Imagens.embarque_32x32_azul;
                }
                return Vars.Imagens.embarque_32x32;
            }
        }
        public ImageSource imagem_fabrica
        {
            get
            {
                if (resumo_pecas.etapa_bloqueada)
                {
                    return Conexoes.BufferImagem._lock;
                }
                if (total_fabricado == 100)
                {
                    return Vars.Imagens.fabrica_32x32_verde;
                }
                if (atraso_fabrica > 0)
                {
                    return Vars.Imagens.fabrica_32x32_vermelho;
                }
                if (total_fabricado > 0)
                {
                    return Vars.Imagens.fabrica_32x32_azul;
                }
                return Vars.Imagens.fabrica_32x32;
            }
        }
        public ImageSource imagem_montagem
        {
            get
            {
                if (resumo_pecas.etapa_bloqueada)
                {
                    return Conexoes.BufferImagem._lock;
                }
                if (exportacao)
                {
                    return Conexoes.BufferImagem.globo;
                }
                if (total_montado == 100 | status_montagem == "CONCLUÍDA" | status_montagem == "ENTREGUE")
                {
                    return Vars.Imagens.montagem_32x32_verde;
                }
                if (!dados_montagem)
                {
                    return Vars.Imagens.montagem_32x32_cinza;
                }
                if (status_montagem == "TRANCADA")
                {
                    return Conexoes.BufferImagem._lock;

                }
                if (atraso_montagem > 0)
                {
                    return Vars.Imagens.montagem_32x32_vermelho;
                }
                else if (total_montado > 0)
                {
                    return Vars.Imagens.montagem_32x32_azul;
                }
                return Vars.Imagens.montagem_32x32;
            }
        }

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
        public double engenharia_peso_previsto
        {
            get
            {
                return Math.Round(this.engenharia_previsto * this.peso_planejado / 100);
            }
        }
        public double fabrica_peso_previsto
        {
            get
            {
                return Math.Round(this.fabrica_previsto * this.peso_planejado / 100);
            }
        }
        public double embarque_peso_previsto
        {
            get
            {
                return Math.Round(this.embarque_previsto * this.peso_planejado / 100);
            }
        }
        public double montagem_peso_previsto
        {
            get
            {
                return Math.Round(this.montagem_previsto * this.peso_planejado / 100);
            }
        }
        public double peso_projetado
        {
            get
            {
                return Math.Round(this.liberado_engenharia * this.peso_planejado / 100);
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

        public SolidColorBrush corengenharia
        {
            get
            {
                return Consultas.getCor(engenharia_previsto, liberado_engenharia);
            }
        }
        public SolidColorBrush corfabrica
        {
            get
            {
                return Consultas.getCor(fabrica_previsto, total_fabricado, opacidade1);
            }
        }
        public SolidColorBrush corembarque
        {
            get
            {
                return Consultas.getCor(embarque_previsto, total_embarcado, opacidade1);
            }
        }
        public SolidColorBrush cormontagem
        {
            get
            {
                return Consultas.getCor(montagem_previsto, total_montado, opacidade1);
            }
        }


        public SolidColorBrush corengenharia2
        {
            get
            {
                var s = corengenharia.Clone();
                s.Opacity = opacidade2;
                return s;
            }
        }
        public SolidColorBrush corfabrica2
        {
            get
            {
                var s = corfabrica.Clone();
                s.Opacity = opacidade2;
                return s;
            }
        }
        public SolidColorBrush corembarque2
        {
            get
            {
                var s = corembarque.Clone();
                s.Opacity = opacidade2;
                return s;
            }
        }
        public SolidColorBrush cormontagem2
        {
            get
            {
                var s = cormontagem.Clone();
                s.Opacity = opacidade2;
                return s;
            }
        }
        public double opacidade0 { get; set; } = 0.5;
        public double opacidade1 { get; set; } = 0.5;
        public double opacidade2 { get; set; } = 0.5;
        public double opacidade3 { get; set; } = 0.75;

        public SolidColorBrush cor
        {
            get
            {
                if (resumo_pecas.etapa_bloqueada | this.status_montagem == "TRANCADA")
                {
                    return new SolidColorBrush(Colors.Violet) { Opacity = opacidade0 };
                }
                else if (this.total_montado > 95 | this.status_montagem == "CONCLUÍDA" | this.status_montagem == "ENTREGUE")
                {
                    return new SolidColorBrush(Colors.LightGreen) { Opacity = opacidade0 };
                }


                return new SolidColorBrush(Colors.White);
            }
        }

        public SolidColorBrush corprevisto
        {
            get
            {
                return new SolidColorBrush(Colors.LimeGreen) { Opacity = opacidade0 };
            }
        }
        public SolidColorBrush corprevisto2
        {
            get
            {
                return new SolidColorBrush(Colors.LimeGreen) { Opacity = opacidade2 };
            }
        }

        public SolidColorBrush corbase
        {
            get
            {
                return new SolidColorBrush(Colors.DarkBlue) { Opacity = opacidade1 };
            }
        }
        public SolidColorBrush corbase2
        {
            get
            {
                return new SolidColorBrush(Colors.DarkBlue) { Opacity = opacidade2 };
            }
        }
        public SolidColorBrush corengenharia3
        {
            get
            {
                var s = corengenharia.Clone();
                s.Opacity = opacidade3;
                return s;
            }
        }
        public SolidColorBrush corfabrica3
        {
            get
            {
                var s = corfabrica.Clone();
                s.Opacity = opacidade3;
                return s;
            }
        }
        public SolidColorBrush corembarque3
        {
            get
            {
                return Consultas.getCor(embarque_previsto, total_embarcado, .2);
            }
        }
        public SolidColorBrush cormontagem3
        {
            get
            {
                return Consultas.getCor(montagem_previsto, total_montado, .3);
            }
        }
        public PLAN_BASE()
        {

        }
    }
}
