using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.painel
{
    public class PLAN_PEDIDO : PLAN_BASE
    {
        public long id_montagem { get; set; } = 0;
        public double latitude { get; private set; } = 0;
        public double longitude { get; private set; } = 0;
        private List<PLAN_PECA_LOG> _logistica_st { get; set; }
        public List<PLAN_PECA_LOG> GetLogistica()
        {
            if(_logistica_st==null)
            {
                var orfas = new List<PLAN_PECA>();
                _logistica_st = DLM.painel.Consultas.GetLogistica(this.GetPecas(), out orfas);
            }
            return _logistica_st;
        }
       
        public List<PLAN_ETAPA> etapas
        {
            get
            {
                return Getetapas();
            }
        }

        public int dias
        {
            get
            {
                var dt = DateTime.Now;
                var ts = (DateTime)ultima_consulta_sap;
                return Conexoes.Utilz.Int((dt - ts).TotalDays);
            }
        }
        public int total_atraso
        {
            get
            {
                return atraso_embarque + atraso_engenharia + atraso_fabrica + atraso_montagem;
            }
        }
        public override string ToString()
        {
            return descricao;
        }


        public string contratostr
        {
            get
            {
                if (pedido.Length >= 9)
                {
                    return pedido.Substring(3, 6);
                }
                return "";
            }
        }
        public string nome { get; private set; } = "";
        public string montagem_engenheiro { get; private set; } = "";
        public long id { get; private set; } = -1;
        public int etapas_qtd { get; private set; } = 0;
        public int pedidos { get; private set; } = 0;

        public PLAN_PEDIDO(DLM.db.Linha L, PLAN_OBRA contrato)
        {
            this.PEP = L.Get("pedido").Valor;
            this.Titulo.Descricao = L.Get("nome").Valor;
            this.engenharia_cronograma = L.Get("engenharia_cronograma").Data();
            this.engenharia_liberacao = L.Get("engenharia_liberacao").Data();
            this.etapas_qtd = L.Get("etapas").Int();
            this.fabrica_cronograma = L.Get("fabrica_cronograma").Data();
            this.peso_embarcado = L.Get("peso_embarcado").Double(6);
            this.peso_planejado = L.Get("peso_planejado").Double(6);
            this.peso_produzido = L.Get("peso_produzido").Double(6);
            this.peso_montado = L.Get("peso_montado").Double(6);
            this.total_montado = L.Get("total_montado").Double();
            this.total_embarcado = L.Get("total_embarcado").Double();
            this.total_fabricado = L.Get("total_produzido").Double();
            this.liberado_engenharia = L.Get("liberado_engenharia").Double();
            this.ultima_edicao = L.Get("ultima_atualizacao").Data();
            this.montagem_inicio = L.Get("montagem_inicio").Data();
            this.montagem_fim = L.Get("montagem_fim").Data();

            this.dados_montagem = L.Get("total_montado").Valor != "";

            this.engenharia_previsto = L.Get("es").Double();
            this.fabrica_previsto = L.Get("fs").Double();
            this.embarque_previsto = L.Get("ls").Double();
            this.montagem_previsto = L.Get("ms").Double();

            this.ultima_consulta_sap = L.Get("ultima_consulta_sap").Data();

            this.nome = L["Nome"].Valor;

            this.montagem_engenheiro = L.Get("montagem_engenheiro").Valor;



            this.atraso_embarque = L.Get("atraso_embarque").Int();
            this.atraso_engenharia = L.Get("atraso_engenharia").Int();
            this.atraso_fabrica = L.Get("atraso_fabrica").Int();
            this.atraso_montagem = L.Get("atraso_montagem").Int();
            this.id_montagem = L.Get("id_montagem").Int();

            this.nome = L["Nome"].Valor;

            this.status_montagem = L.Get("status_montagem").Valor;

            DateTime mont = L.Get("update_montagem").Data();
            if (mont > Cfg.Init.DataDummy())
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }

            this.latitude = L.Get("latitude").Double(8);
            this.longitude = L.Get("longitude").Double(8);
        }
        public PLAN_PEDIDO()
        {

        }
        public PLAN_PEDIDO(List<string> l_sap)
        {
            var ls = l_sap.Select(x => x.Split('=').ToList()).ToList().FindAll(x => x.Count > 0);
            var ped = ls.Find(x => x[0].Contains("PROJETO"));
            if(ped!=null)
            {
                this.PEP = ped[1];
            }
            var desc = ls.Find(x => x[0].Contains("VTEXT"));
            if (desc != null)
            {
                this.nome = desc[1];
            }
        }
    }
}
