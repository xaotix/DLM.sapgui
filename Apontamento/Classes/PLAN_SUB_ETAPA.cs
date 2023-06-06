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

            this.engenharia_cronograma = linha.Get("engenharia_cronograma").Data();
            this.engenharia_liberacao = linha.Get("engenharia_liberacao").Data();
            this.fabrica_cronograma = linha.Get("fabrica_cronograma").Data();
            this.liberado_engenharia = linha.Get("liberado_engenharia").Double();
            this.peso_embarcado = linha.Get("peso_embarcado").Double(6);
            this.peso_planejado = linha.Get("peso_planejado").Double(6);
            this.peso_produzido = linha.Get("peso_produzido").Double(6);
            this.total_montado = linha.Get("total_montado").Double(6);
            this.engenharia_previsto = linha.Get("engenharia_previsto").Double();
            this.fabrica_previsto = linha.Get("fabrica_previsto").Double();
            this.montagem_previsto = linha.Get("montagem_previsto").Double();
            this.peso_montado = linha.Get("peso_montado").Double();
            this.ultima_edicao = linha["ultima_edicao"].Data();
            this.peps_qtd = linha.Get("pep_fabrica").Int();

            this.dados_montagem = linha.Get("total_montado").Valor != "";

            this.ultima_consulta_sap = linha.Get("ultima_consulta_sap").Data();

            this.engenharia_previsto = linha.Get("es").Double();
            this.fabrica_previsto = linha.Get("fs").Double();

            this.embarque_previsto = linha.Get("ls").Double();

            this.montagem_previsto = linha.Get("ms").Double();
     
            this.engenharia_cronograma_inicio = linha.Get("ei").Data();
            this.fabrica_cronograma_inicio = linha.Get("fi").Data();
            this.logistica_cronograma = linha.Get("lf").Data();
            this.logistica_cronograma_inicio = linha.Get("li").Data();
            this.montagem_cronograma = linha.Get("mf").Data();
            this.montagem_cronograma_inicio = linha.Get("mi").Data();

            this.mi_s = linha.Get("mi_s").Data();
            this.mf_s = linha.Get("mf_s").Data();




            if (this.logistica_cronograma_inicio == Cfg.Init.DataDummy() && this.logistica_cronograma != Cfg.Init.DataDummy())
            {
                this.logistica_cronograma_inicio = ((DateTime)this.fabrica_cronograma).AddDays(2);
            }

            if (this.montagem_cronograma_inicio == Cfg.Init.DataDummy() && this.montagem_cronograma != Cfg.Init.DataDummy())
            {
                this.montagem_cronograma_inicio = ((DateTime)this.logistica_cronograma).AddDays(2);
            }

            this.resumo = linha["descricao"].Valor;


            this.atraso_engenharia = linha.Get("atraso_engenharia").Int();
            this.atraso_fabrica = linha.Get("atraso_fabrica").Int();
            this.atraso_embarque = linha.Get("atraso_embarque").Int();
            this.atraso_montagem = linha.Get("atraso_montagem").Int();

            this.montagem_engenheiro = linha.Get("montagem_engenheiro").Valor;

            if (this.montagem_cronograma_inicio < Cfg.Init.DataDummy())
            {
                this.Montagem_Balanco = false;
                this.montagem_cronograma = linha.Get("montagem_fim").Data();
                this.montagem_cronograma_inicio = linha.Get("montagem_inicio").Data();

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

            //this.almox_comprado = L.Get("almox_comprado").Boolean();
            //this.almox_comprado_data = L.Get("almox_comprado_data").Data();
            //this.almox_comprado_user = L.Get("almox_comprado_user").ToString();



            DateTime mont = linha.Get("update_montagem").Data();
            if (mont > Cfg.Init.DataDummy())
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }
        }


    }
}
