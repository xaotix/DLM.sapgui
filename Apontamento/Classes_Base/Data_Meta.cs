using DLM.sapgui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{

  public  class Meta
    {
        public override string ToString()
        {
            return "De " + Data_Inicio.ToShortDateString() + " até " + Data_Fim.ToShortDateString() + " Previsto: " + Previsto.ToString() + " Realizado: " + Realizado.ToString() + " Desvio: " + Nao_Previsto.ToString();
        }
        public List<PLAN_SUB_ETAPA> SubEtapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        public List<Valor_Meta> Metas
        {
            get
            {
                List<Valor_Meta> retorno = new List<Valor_Meta>();
                if(Previsto.SubEtapas.Count>0)
                {
                    retorno.Add(Previsto);
                }
                if (Realizado.SubEtapas.Count > 0)
                {
                    retorno.Add(Realizado);
                }
                if (Nao_Previsto.SubEtapas.Count > 0)
                {
                    retorno.Add(Nao_Previsto);
                }
                if (Nao_Previsto_Atrasos.SubEtapas.Count > 0)
                {
                    retorno.Add(Nao_Previsto_Atrasos);
                }
                return retorno;
            }
        }

        public Valor_Meta Realizado_Total
        {
            get
            {
                Valor_Meta retorno = new Valor_Meta();
                retorno.Peso_Total = Realizado.Peso_Total + Finalizado_Sem_Data.Peso_Total;
                retorno.Quantidade = Realizado.Quantidade + Finalizado_Sem_Data.Quantidade;
                retorno.SubEtapas.AddRange(Realizado.SubEtapas);
                retorno.SubEtapas.AddRange(Finalizado_Sem_Data.SubEtapas);
                retorno.Data = this.Data_Fim;
                retorno.Titulo = "Realizado";
                return retorno;
            }
        }

        public Valor_Meta Avanco_Previsto { get; set; } = new Valor_Meta();
        public Valor_Meta Avanco_Realizado { get; set; } = new Valor_Meta();
        public Valor_Meta Avanco_Realizado_Nao_Previsto { get; set; } = new Valor_Meta();

        public DateTime Data_Inicio { get; set; } = new DateTime();
        public DateTime Data_Fim { get; set; } = new DateTime();
        public Valor_Meta Previsto { get; set; } = new Valor_Meta();
        public Valor_Meta Realizado { get; set; } = new Valor_Meta();
        public Valor_Meta Nao_Previsto { get; set; } = new Valor_Meta();
        public Valor_Meta Nao_Previsto_Atrasos { get; set; } = new Valor_Meta();
        public Valor_Meta Finalizado_Sem_Data { get; set; } = new Valor_Meta();
        public Meta()
        {

        }

        public Meta(List<PLAN_SUB_ETAPA> lista, DateTime f0, DateTime f0_fim, Tipo_Meta Tipo, Tipo_Filtro_Meta Filtro)
        {
            List<PLAN_SUB_ETAPA> total = lista;
            try
            {
                double valor_considerar_finalizado = (Filtro == Tipo_Filtro_Meta.Etapa ? 99 : 0);
                var min_data = Conexoes.Utilz.Calendario.DataDummy();
                this.SubEtapas = lista;
                if (Tipo == Tipo_Meta.Engenharia)
                {
                    this.SubEtapas = this.SubEtapas.OrderBy(x => x.engenharia_cronograma).ToList().FindAll(x => x.engenharia_cronograma >= f0 && x.engenharia_cronograma <= f0_fim);
                    var subs_real = this.SubEtapas.FindAll(x =>  (x.engenharia_liberacao <= f0_fim && x.engenharia_liberacao > min_data)  && x.liberado_engenharia > valor_considerar_finalizado).FindAll(x => this.SubEtapas.Find(y => y.PEP == x.PEP) != null);
                    var desvio = lista.FindAll(x => x.engenharia_liberacao <= f0_fim && x.engenharia_liberacao >= f0).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);

              

                    var desvio_adiantamento = desvio.FindAll(x => x.engenharia_cronograma >= f0_fim).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);
                    var desvio_atrasos = desvio.FindAll(x => x.engenharia_cronograma < f0_fim).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);


                    var finalizado_sem_data = this.SubEtapas.FindAll(x => x.liberado_engenharia > valor_considerar_finalizado && x.engenharia_liberacao < min_data);
                    this.Data_Inicio = f0;
                    this.Data_Fim = f0_fim;
                    this.Nao_Previsto = new Valor_Meta(desvio_adiantamento, "Adiantamentos", f0_fim);
                    this.Nao_Previsto_Atrasos = new Valor_Meta(desvio_atrasos, "Atrasos", f0_fim);
                    this.Previsto = new Valor_Meta(this.SubEtapas, "Previsto", f0_fim);
                    this.Realizado = new Valor_Meta(subs_real, "Realizado", f0_fim,true);
                    this.Finalizado_Sem_Data = new Valor_Meta(finalizado_sem_data, "Finalizados Sem Apontamento", f0_fim);



                    var avanco_previsto = total.OrderBy(x => x.engenharia_cronograma).ToList().FindAll(x => x.engenharia_cronograma <= f0_fim).ToList();
                    var avanco_total_realizado = total.FindAll(x => x.engenharia_liberacao <= f0_fim && x.engenharia_liberacao > min_data && x.liberado_engenharia > valor_considerar_finalizado);
                    var avanco_realizado = avanco_total_realizado.FindAll(x => avanco_previsto.Find(y => y.PEP == x.PEP) != null);
                    var avanco_realizado_nao_previsto = avanco_total_realizado.FindAll(x => avanco_previsto.Find(y => y.PEP == x.PEP) == null);


                    this.Avanco_Previsto = new Valor_Meta(avanco_previsto, "Avanço Previsto", f0_fim);
                    this.Avanco_Realizado = new Valor_Meta(avanco_realizado, "Avanço Realizado Previsto", f0_fim);
                    this.Avanco_Realizado_Nao_Previsto = new Valor_Meta(avanco_total_realizado, "Total Realizado", f0_fim);
                }
                else if (Tipo == Tipo_Meta.Fabrica)
                {

                    this.SubEtapas = this.SubEtapas.OrderBy(x => x.fabrica_cronograma).ToList().FindAll(x => x.fabrica_cronograma >= f0 && x.fabrica_cronograma <= f0_fim);
                    var subs_real = this.SubEtapas.FindAll(x => x.resumo_pecas.Fim <= f0_fim && x.resumo_pecas.Fim > min_data && x.total_fabricado > valor_considerar_finalizado).FindAll(x => this.SubEtapas.Find(y => y.PEP == x.PEP) != null);
                    var desvio = lista.FindAll(x => x.resumo_pecas.Fim <= f0_fim && x.resumo_pecas.Fim >= f0 && x.resumo_pecas.Fim <= f0_fim && x.total_fabricado > valor_considerar_finalizado).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);
                    var finalizado_sem_data = this.SubEtapas.FindAll(x => x.total_fabricado > valor_considerar_finalizado && x.resumo_pecas.Fim < min_data);
                    this.Data_Inicio = f0;
                    this.Data_Fim = f0_fim;

                    var desvio_adiantamento = desvio.FindAll(x => x.fabrica_cronograma >= f0_fim).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);
                    var desvio_atrasos = desvio.FindAll(x => x.fabrica_cronograma < f0_fim).FindAll(x => subs_real.Find(y => y.PEP == x.PEP) == null);

                    this.Nao_Previsto = new Valor_Meta(desvio_adiantamento, "Adiantamentos", f0_fim);
                    this.Nao_Previsto_Atrasos = new Valor_Meta(desvio_atrasos, "Atrasos", f0_fim);
                    this.Previsto = new Valor_Meta(this.SubEtapas, "Previsto", f0_fim);
                    this.Realizado = new Valor_Meta(subs_real, "Realizado", f0_fim,true);
                    this.Finalizado_Sem_Data = new Valor_Meta(finalizado_sem_data, "Finalizados Sem Apontamento", f0_fim);



                    var avanco_previsto = total.OrderBy(x => x.fabrica_cronograma).ToList().FindAll(x => x.fabrica_cronograma <= f0_fim);
                    var avanco_total_realizado = total.FindAll(x => x.resumo_pecas.Fim <= f0_fim && x.resumo_pecas.Fim > min_data && x.total_fabricado > valor_considerar_finalizado);
                    var avanco_realizado = avanco_total_realizado.FindAll(x => avanco_previsto.Find(y => y.PEP == x.PEP) != null);
                    var avanco_realizado_nao_previsto = avanco_total_realizado.FindAll(x => avanco_previsto.Find(y => y.PEP == x.PEP) == null);


                    this.Avanco_Previsto = new Valor_Meta(avanco_previsto, "Avanço Previsto", f0_fim);
                    this.Avanco_Realizado = new Valor_Meta(avanco_realizado, "Avanço Realizado Previsto", f0_fim);
                    this.Avanco_Realizado_Nao_Previsto = new Valor_Meta(avanco_total_realizado, "Total Realizado", f0_fim);

                }
            }
            catch (Exception)
            {

            }


        }
    }
    public class Valor_Meta
    {
        public DateTime Data { get; set; } = new DateTime();

        public string Titulo { get; set; } = "";
        public override string ToString()
        {
            return "Peso: " + Peso_Total + "/ Subetapas: " + Quantidade;
        }
        public double Peso_Total { get; set; } = 0;
        public int Quantidade { get; set; } = 0;
        public List<PLAN_SUB_ETAPA> SubEtapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        public Valor_Meta()
        {

        }

        public Valor_Meta(int Quantidade, double Peso_Total)
        {

        }
        public Valor_Meta(List<PLAN_SUB_ETAPA> subs, string titulo, DateTime Data, bool peso_realizado = false)
        {
            this.Data = Data;
            this.Titulo = titulo;
            if(peso_realizado)
            {
                this.Peso_Total = subs.Sum(x => x.peso_produzido);

            }
            else
            {
                this.Peso_Total = subs.Sum(x => x.peso_planejado);
            }
            this.Quantidade = subs.Count();
            this.SubEtapas = subs;
        }

    }
}
