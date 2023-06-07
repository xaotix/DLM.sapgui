using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;


namespace DLM.painel
{

    public class PLAN_OBRAS : PLAN_OBRA
    {
        public List<PLAN_ETAPA> etapas { get; set; }
        public List<PLAN_OBRA> obras { get; set; } = new List<PLAN_OBRA>();
        public PLAN_OBRAS()
        {

        }
        public PLAN_OBRAS(List<PLAN_OBRA> Obras, string contrato = "", string nome = "")
        {

            if (Obras.Count == 0) { return; }

            this.obras = Obras;

            this.PEP = $"{this.obras.Count} Obras";
            this.Titulo.Contrato = contrato;
            this.Titulo.Descricao = nome;

            this.engenharia_cronograma = Obras.Max(x => x.engenharia_cronograma);
            this.engenharia_liberacao = Obras.Max(x => x.engenharia_cronograma);
            this.etapas_qtd = Obras.Sum(x => x.etapas_qtd);
            this.pedidos_qtd = Obras.Sum(x => x.pedidos_qtd);
            this.fabrica_cronograma = Obras.Max(x => x.fabrica_cronograma);

            this.nome = Obras.Count() + " OBRAS";

            this.peso_planejado = Math.Round(Obras.Sum(x => x.peso_planejado));
            this.peso_produzido = Math.Round(Obras.Sum(x => x.peso_produzido));
            this.peso_embarcado = Math.Round(Obras.Sum(x => x.peso_embarcado));
            this.peso_montado = Math.Round(Obras.Sum(x => x.peso_montado));

            this.total_embarcado = Obras.Sum(x => x.total_embarcado) / Obras.Count();
            this.total_fabricado = Obras.Sum(x => x.total_fabricado) / Obras.Count();
            this.liberado_engenharia = Obras.Sum(x => x.liberado_engenharia) / Obras.Count();
            this.total_montado = Obras.Sum(x => x.total_montado) / Obras.Count();

            this.ultima_edicao = Obras.Min(x => x.ultima_edicao);

            this.dados_montagem = true;

            this.criado = Obras.Min(x => x.criado);

            this.montagem_inicio = Obras.Min(x => x.montagem_inicio);
            this.montagem_fim = Obras.Max(x => x.montagem_fim);

            this.ultima_consulta_sap = Obras.Max(x => x.ultima_consulta_sap);


            this.efim = obras.Max(x => x.efim);
            this.eini = obras.Min(x => x.eini);
            this.eng_base_st = obras.Sum(x => x.eng_base_st) / obras.Count;


            this.ffim = obras.Max(x => x.ffim);
            this.fini = obras.Min(x => x.fini);
            this.fab_base_st = obras.Sum(x => x.fab_base_st) / obras.Count;


            this.lfim = obras.Max(x => x.lfim);
            this.lini = obras.Min(x => x.lini);
            this.log_base_st = obras.Sum(x => x.log_base_st) / obras.Count;

            this.mfim = obras.Max(x => x.lfim);
            this.mini = obras.Min(x => x.lini);
            this.mon_base_st = obras.Sum(x => x.mon_base_st) / obras.Count;


            this.engenharia_previsto = Obras.Sum(x => x.engenharia_previsto) / Obras.Count();
            this.fabrica_previsto = Obras.Sum(x => x.fabrica_previsto) / Obras.Count();
            this.embarque_previsto = Obras.Sum(x => x.embarque_previsto) / Obras.Count();
            this.montagem_previsto = Obras.Sum(x => x.montagem_previsto) / Obras.Count();


            this.atraso_embarque = Obras.Sum(x => x.atraso_embarque);
            this.atraso_engenharia = Obras.Sum(x => x.atraso_engenharia);
            this.atraso_fabrica = Obras.Sum(x => x.atraso_fabrica);
            this.atraso_montagem = Obras.Sum(x => x.atraso_montagem);

            this.id_montagem = 1;

            //this.resumo_pecas = new Resumo_Pecas(Obras.Select(x => x.resumo_pecas).ToList());

            this.status_montagem = "EM ANDAMENTO";

            if(nome!="")
            {
                this.nome = nome;
            }
        }
    }
    public class PLAN_OBRA : PLAN_BASE
    {
        private List<Conexoes.MSAP_PEP> _peps_eng { get; set; }
        private List<Conexoes.MSAP_Pedido> _Pedidos_Eng { get; set; }

        public ImageSource Imagem
        {
            get
            {
                return imagem;
            }
        }
        public PLAN_OBRA Clonar()
        {
            return new PLAN_OBRA(this.Linha);
        }
        public bool finalizado { get; set; } = false;

        public string chave_pedido { get; private set; } = "";
        public long id_montagem { get; set; } = 0;
    


        public Telerik.Windows.Controls.Map.Location Location
        {
            get
            {
                return new Telerik.Windows.Controls.Map.Location(latitude, longitude) { Description = this.nome };
            }
        }
        public double dias
        {
            get
            {
                var dt = DateTime.Now;
                var ts = (DateTime)ultima_consulta_sap;
                return Conexoes.Utilz.Double((dt - ts).TotalDays,2);
            }
        }
        public override string ToString()
        {
            return descricao;
        }

        private List<string> _pedidos_clean { get; set; }
        public List<string> pedidos_clean
        {
            get
            {
                if (_pedidos_clean == null)
                {
                    if (this.contrato == "")
                    {
                        return new List<string>();
                    }
                    _pedidos_clean = DLM.painel.Consultas.GetPedidosClean(new List<string> {this.contrato },false);


                }
                return _pedidos_clean;
            }
        }
        public List<PLAN_PEDIDO> pedidos
        {
            get
            {
                return GetPedidos();
            }
        }

        public string contrato_completo
        {
            get
            {
                return this.setor_atividade + "-" + this.contrato;
            }
        }

        public string nome { get; set; } = "";
        public double latitude { get; private set; } = 0;
        public double longitude { get; private set; } = 0;

        public int etapas_qtd { get; set; } = 0;
        public int pedidos_qtd { get; set; } = 0;



        public List<Conexoes.MSAP_Pedido> pedidos_eng
        {
            get
            {
                if (_Pedidos_Eng == null)
                {
                    _Pedidos_Eng = DLM.painel.Vars.Obras.GetPedidos(contrato);
                }
                return _Pedidos_Eng;
            }
        }

        public List<Conexoes.MSAP_PEP> peps_eng
        {
            get
            {
                if (_peps_eng == null)
                {
                    _peps_eng = pedidos_eng.SelectMany(x => x.GetPEPs()).OrderBy(x => x.Codigo).ToList();
                }
                return _peps_eng;
            }
        }



        public PLAN_OBRA(string pedido_principal)
        {
            this.PEP = pedido_principal.Replace(".C00", ".P").Replace(".P00", "").Replace(".G00", "");
            this.chave_pedido = pedido_principal.Replace(".C00", ".P").Replace(".P00", ".P").Replace(".G00", ".G");
        }
        public PLAN_OBRA()
        {

        }
        public PLAN_OBRA(DLM.db.Linha linha)
        {
            this.Linha = linha;
            string pedido_principal = linha["pedido_principal"].Valor;
            this.PEP = pedido_principal.Replace(".C00", ".P").Replace(".P00","").Replace(".G00","");
            this.Titulo.Descricao = linha["nome"].Valor;
            this.chave_pedido = pedido_principal.Replace(".C00", ".P").Replace(".P00", ".P").Replace(".G00", ".G");
            this.engenharia_cronograma = linha["engenharia_cronograma"].Data();
            this.engenharia_liberacao = linha["engenharia_liberacao"].Data();
            this.etapas_qtd = linha["etapas"].Int();
            this.pedidos_qtd = linha["pedidos"].Int();
            this.fabrica_cronograma = linha["fabrica_cronograma"].Data();
            this.latitude = linha["latitude"].Double(8);
            this.longitude = linha["longitude"].Double(8);
            this.nome = linha["nome"].Valor;
            this.peso_embarcado = linha["peso_embarcado"].Double();
            this.peso_planejado = linha["peso_planejado"].Double();
            this.peso_produzido = linha["peso_produzido"].Double();
            this.total_embarcado = linha["total_embarcado"].Double();
            this.total_fabricado = linha["total_produzido"].Double();
            this.liberado_engenharia = linha["liberado_engenharia"].Double();
            this.peso_montado = linha["peso_montado"].Double();
            this.total_montado = linha["total_montado"].Double();
            this.ultima_edicao = linha["ultima_atualizacao"].Data();

            this.dados_montagem = linha["total_montado"].Valor != "";

            this.montagem_inicio = linha["montagem_inicio"].Data();
            this.montagem_fim = linha["montagem_fim"].Data();

            this.ultima_consulta_sap = linha["ultima_consulta_sap"].Data();

            this.engenharia_previsto = linha["es"].Double();
            this.fabrica_previsto = linha["fs"].Double();
            this.embarque_previsto = linha["ls"].Double();
            this.montagem_previsto = linha["ms"].Double();

            this.atraso_embarque = linha["atraso_embarque"].Int();
            this.atraso_engenharia = linha["atraso_engenharia"].Int();
            this.atraso_fabrica = linha["atraso_fabrica"].Int();
            this.atraso_montagem = linha["atraso_montagem"].Int();

            this.id_montagem = linha["id_montagem"].Int();
            this.status_montagem = linha["status_montagem"].Valor;

            this.criado = linha["criado"].Data();

            DateTime mont = linha["update_montagem"].Data();
            if (mont > Cfg.Init.DataDummy())
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }

            this.finalizado = linha["finalizado"].Valor.ToUpper() == "X";

        }
    }
}
