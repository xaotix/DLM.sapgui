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
        public PLAN_SUB_ETAPA(DLM.db.Linha L, List<PLAN_PEP> peps)
        {
            this.PEP = L.Get("pep").Valor;


            if (peps != null && this.carregou_pecas)
            {
                this.Set(peps);   
                foreach(var pep in this.peps)
                {
                    pep.Set(this.GetPecas());
                }
            }


            this.Linha = L;

            this.engenharia_cronograma = L.Get("engenharia_cronograma").Data();
            this.engenharia_liberacao = L.Get("engenharia_liberacao").Data();
            this.fabrica_cronograma = L.Get("fabrica_cronograma").Data();
            this.liberado_engenharia = L.Get("liberado_engenharia").Double();
            this.peso_embarcado = L.Get("peso_embarcado").Double(6);
            this.peso_planejado = L.Get("peso_planejado").Double(6);
            this.peso_produzido = L.Get("peso_produzido").Double(6);
            this.total_montado = L.Get("total_montado").Double(6);
            this.engenharia_previsto = L.Get("engenharia_previsto").Double();
            this.fabrica_previsto = L.Get("fabrica_previsto").Double();
            this.montagem_previsto = L.Get("montagem_previsto").Double();
            this.peso_montado = L.Get("peso_montado").Double();
            this.ultima_edicao = L["ultima_edicao"].Data();
            this.peps_qtd = L.Get("pep_fabrica").Int();

            this.dados_montagem = L.Get("total_montado").Valor != "";

            this.ultima_consulta_sap = L.Get("ultima_consulta_sap").Data();

            this.engenharia_previsto = L.Get("es").Double();
            this.fabrica_previsto = L.Get("fs").Double();

            this.embarque_previsto = L.Get("ls").Double();

            this.montagem_previsto = L.Get("ms").Double();
     
            this.engenharia_cronograma_inicio = L.Get("ei").Data();
            this.fabrica_cronograma_inicio = L.Get("fi").Data();
            this.logistica_cronograma = L.Get("lf").Data();
            this.logistica_cronograma_inicio = L.Get("li").Data();
            this.montagem_cronograma = L.Get("mf").Data();
            this.montagem_cronograma_inicio = L.Get("mi").Data();

            this.mi_s = L.Get("mi_s").Data();
            this.mf_s = L.Get("mf_s").Data();




            if (this.logistica_cronograma_inicio == Cfg.Init.DataDummy() && this.logistica_cronograma != Cfg.Init.DataDummy())
            {
                this.logistica_cronograma_inicio = ((DateTime)this.fabrica_cronograma).AddDays(2);
            }

            if (this.montagem_cronograma_inicio == Cfg.Init.DataDummy() && this.montagem_cronograma != Cfg.Init.DataDummy())
            {
                this.montagem_cronograma_inicio = ((DateTime)this.logistica_cronograma).AddDays(2);
            }

            this.resumo = L.Get("descricao").Valor;


            this.atraso_engenharia = L.Get("atraso_engenharia").Int();
            this.atraso_fabrica = L.Get("atraso_fabrica").Int();
            this.atraso_embarque = L.Get("atraso_embarque").Int();
            this.atraso_montagem = L.Get("atraso_montagem").Int();

            this.montagem_engenheiro = L.Get("montagem_engenheiro").Valor;

            if (this.montagem_cronograma_inicio < Cfg.Init.DataDummy())
            {
                this.Montagem_Balanco = false;
                this.montagem_cronograma = L.Get("montagem_fim").Data();
                this.montagem_cronograma_inicio = L.Get("montagem_inicio").Data();

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



            DateTime mont = L.Get("update_montagem").Data();
            if (mont > Cfg.Init.DataDummy())
            {
                this.update_montagem = "Montagem: " + mont.ToShortDateString();
            }
        }


    }
}
