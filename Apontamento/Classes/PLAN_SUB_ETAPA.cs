using DLM.vars;
using System;
using System.Collections.Generic;

namespace DLM.painel
{
    public class PLAN_SUB_ETAPA : PLAN_BASE
    {
        public override string ToString()
        {
            return descricao;
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

            this.engenharia_cronograma = linha["engenharia_cronograma"].Data();
            this.engenharia_liberacao = linha["engenharia_liberacao"].Data();
            this.fabrica_cronograma = linha["fabrica_cronograma"].Data();
            this.liberado_engenharia = linha["liberado_engenharia"].Double();
            this.peso_embarcado = linha["peso_embarcado"].Double(6);
            this.peso_planejado = linha["peso_planejado"].Double(6);
            this.peso_produzido = linha["peso_produzido"].Double(6);
            this.total_montado = linha["total_montado"].Double(6);
            this.engenharia_previsto = linha["engenharia_previsto"].Double();
            this.fabrica_previsto = linha["fabrica_previsto"].Double();
            this.montagem_previsto = linha["montagem_previsto"].Double();
            this.peso_montado = linha["peso_montado"].Double();
            this.ultima_edicao = linha["ultima_edicao"].Data();
            this.peps_qtd = linha["pep_fabrica"].Int();

            this.dados_montagem = linha["total_montado"].Valor != "";

            this.ultima_consulta_sap = linha["ultima_consulta_sap"].Data();

            this.engenharia_previsto = linha["es"].Double();
            this.fabrica_previsto = linha["fs"].Double();

            this.embarque_previsto = linha["ls"].Double();

            this.montagem_previsto = linha["ms"].Double();
     
            this.engenharia_cronograma_inicio = linha["ei"].Data();
            this.fabrica_cronograma_inicio = linha["fi"].Data();
            this.logistica_cronograma = linha["lf"].Data();
            this.logistica_cronograma_inicio = linha["li"].Data();
            this.montagem_cronograma = linha["mf"].Data();
            this.montagem_cronograma_inicio = linha["mi"].Data();

            this.mi_s = linha["mi_s"].Data();
            this.mf_s = linha["mf_s"].Data();




            if (this.logistica_cronograma_inicio == Cfg.Init.DataDummy() && this.logistica_cronograma != Cfg.Init.DataDummy())
            {
                this.logistica_cronograma_inicio = ((DateTime)this.fabrica_cronograma).AddDays(2);
            }

            if (this.montagem_cronograma_inicio == Cfg.Init.DataDummy() && this.montagem_cronograma != Cfg.Init.DataDummy())
            {
                this.montagem_cronograma_inicio = ((DateTime)this.logistica_cronograma).AddDays(2);
            }

            this.resumo = linha["descricao"].Valor;


            this.atraso_engenharia = linha["atraso_engenharia"].Int();
            this.atraso_fabrica = linha["atraso_fabrica"].Int();
            this.atraso_embarque = linha["atraso_embarque"].Int();
            this.atraso_montagem = linha["atraso_montagem"].Int();

            this.montagem_engenheiro = linha["montagem_engenheiro"].Valor;

            if (this.montagem_cronograma_inicio < Cfg.Init.DataDummy())
            {
                this.Montagem_Balanco = false;
                this.montagem_cronograma = linha["montagem_fim"].Data();
                this.montagem_cronograma_inicio = linha["montagem_inicio"].Data();
            }


            var t = peso_embarcado / peso_planejado * 100;
            if (t > 0)
            {
                this.total_embarcado = Math.Round(t, 2);
            }
            else if (t > 100)
            {
                this.total_embarcado = 100;

            }

            var t2 = peso_produzido / peso_planejado * 100;
            if (t2 > 0)
            {
                this.total_fabricado = Math.Round(t2, 2);
            }
            else if (t2 > 100)
            {
                this.total_fabricado = 100;

            }
            DateTime mont = linha["update_montagem"].Data();
            if (mont > Cfg.Init.DataDummy())
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }
        }


    }
}
