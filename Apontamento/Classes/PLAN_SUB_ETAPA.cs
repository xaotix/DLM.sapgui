using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;

namespace DLM.painel
{
    public class PLAN_SUB_ETAPA : PLAN_BASE
    {
        public override string ToString()
        {
            return $"{PEP} {descricao}";
        }
        public List<PLAN_PEP> peps
        {
            get
            {
                return GetPeps();
            }
        }

        public bool Montagem_Balanco { get; set; } = false;
        public string montagem_engenheiro { get; private set; } = "";
        public string resumo { get; private set; } = "";

        public int peps_qtd { get; private set; } = 0;


        public PLAN_SUB_ETAPA()
        {

        }
        public PLAN_SUB_ETAPA(DLM.db.Linha linha, List<PLAN_PEP> peps)
        {
            this.PEP = linha["pep"].Valor;


            if (peps != null && this.carregou_pecas)
            {
                this.Set(peps);   
                foreach(var pep in this.peps)
                {
                    pep.Set(this.GetPecas());
                }
            }


            this.Linha = linha;

            this.engenharia_cronograma = linha["engenharia_cronograma"].DataNull();
            this.engenharia_liberacao = linha["engenharia_liberacao"].DataNull();
            this.fabrica_cronograma = linha["fabrica_cronograma"].DataNull();
            this.liberado_engenharia = linha["liberado_engenharia"].Double();
            this.peso_embarcado = linha["peso_embarcado"].Double(6);
            this.peso_planejado = linha["peso_planejado"].Double(6);
            this.peso_produzido = linha["peso_produzido"].Double(6);
            this.total_montado = linha["total_montado"].Double(6);
            this.engenharia_previsto = linha["engenharia_previsto"].Double();
            this.fabrica_previsto = linha["fabrica_previsto"].Double();
            this.montagem_previsto = linha["montagem_previsto"].Double();
            this.peso_montado = linha["peso_montado"].Double();
            this.ultima_edicao = linha["ultima_edicao"].DataNull();
            this.peps_qtd = linha["pep_fabrica"].Int();

            this.dados_montagem = linha["total_montado"].Valor != "";

            this.ultima_consulta_sap = linha["ultima_consulta_sap"].DataNull();

            this.engenharia_previsto = linha["es"].Double();
            this.fabrica_previsto = linha["fs"].Double();

            this.embarque_previsto = linha["ls"].Double();

            this.montagem_previsto = linha["ms"].Double();
     
            this.engenharia_cronograma_inicio = linha["ei"].DataNull();
            this.fabrica_cronograma_inicio = linha["fi"].DataNull();
            this.logistica_cronograma = linha["lf"].DataNull();
            this.logistica_cronograma_inicio = linha["li"].DataNull();
            this.montagem_cronograma = linha["mf"].DataNull();
            this.montagem_cronograma_inicio = linha["mi"].DataNull();

            this.mi_s = linha["mi_s"].DataNull();
            this.mf_s = linha["mf_s"].DataNull();




            if (this.logistica_cronograma_inicio == null && this.logistica_cronograma != null)
            {
                this.logistica_cronograma_inicio = ((DateTime)this.fabrica_cronograma).AddDays(2);
            }

            if (this.montagem_cronograma_inicio == null && this.montagem_cronograma != null)
            {
                this.montagem_cronograma_inicio = ((DateTime)this.logistica_cronograma).AddDays(2);
            }

            this.resumo = linha["descricao"].Valor;


            this.atraso_engenharia = linha["atraso_engenharia"].Int();
            this.atraso_fabrica = linha["atraso_fabrica"].Int();
            this.atraso_embarque = linha["atraso_embarque"].Int();
            this.atraso_montagem = linha["atraso_montagem"].Int();

            this.montagem_engenheiro = linha["montagem_engenheiro"].Valor;

            if (this.montagem_cronograma_inicio < Cfg.Init.DataDummy)
            {
                this.Montagem_Balanco = false;
                this.montagem_cronograma = linha["montagem_fim"].DataNull();
                this.montagem_cronograma_inicio = linha["montagem_inicio"].DataNull();
            }


            var t = peso_embarcado / peso_planejado * 100;
            if (t > 0)
            {
                this.total_embarcado = t.Round(2);
            }
            else if (t > 100)
            {
                this.total_embarcado = 100;

            }

            var t2 = peso_produzido / peso_planejado * 100;
            if (t2 > 0)
            {
                this.total_fabricado = t2.Round(2);
            }
            else if (t2 > 100)
            {
                this.total_fabricado = 100;

            }
            DateTime mont = linha["update_montagem"].Data();
            if (mont > Cfg.Init.DataDummy)
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }
        }


    }
}
