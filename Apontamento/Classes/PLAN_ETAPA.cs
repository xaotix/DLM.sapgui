using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
    public class PLAN_ETAPA : PLAN_BASE
    {
        public override string ToString()
        {
            return descricao;
        }
        public List<PLAN_SUB_ETAPA> subetapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        public List<PLAN_SUB_ETAPA> get(bool atraso_engenharia = false, bool atraso_fabrica = false, bool atraso_embarque = false, bool atraso_montagem = false)
        {
            List<PLAN_SUB_ETAPA> retorno = new List<PLAN_SUB_ETAPA>();
            List<string> pep = new List<string>();
            if (atraso_engenharia)
            {
                pep.AddRange(this.subetapas.FindAll(x => x.atraso_engenharia > 0).Select(x => x.PEP));
            }
            if (atraso_fabrica)
            {
                pep.AddRange(this.subetapas.FindAll(x => x.atraso_fabrica > 0).Select(x => x.PEP));
            }
            if (atraso_embarque)
            {
                pep.AddRange(this.subetapas.FindAll(x => x.atraso_embarque > 0).Select(x => x.PEP));
            }
            if (atraso_montagem)
            {
                pep.AddRange(this.subetapas.FindAll(x => x.atraso_montagem > 0).Select(x => x.PEP));
            }
            pep = pep.Distinct().ToList();
            foreach (var p in pep)
            {
                retorno.AddRange(this.subetapas.FindAll(x => x.PEP == p));
            }
            return retorno;
        }
        public List<PLAN_SUB_ETAPA> subetapas_com_chumbacao { get; private set; } = new List<PLAN_SUB_ETAPA>();
        public double total
        {
            get
            {
                return ((DateTime)montagem_cronograma - (DateTime)engenharia_cronograma_inicio).TotalDays;
            }
        }
        public PLAN_ETAPA()
        {

        }
        public PLAN_ETAPA(List<PLAN_SUB_ETAPA> subetapas)
        {
            this.subetapas_com_chumbacao = subetapas;
            this.subetapas = subetapas_com_chumbacao.FindAll(x => !x.PEP.Contains(".10A")).ToList();
            if (this.subetapas.Count > 0)
            {
                this.PEP = this.subetapas[0].etapa;
                this.engenharia_cronograma = this.subetapas.Max(x => x.engenharia_cronograma);
                this.fabrica_cronograma = this.subetapas.Max(x => x.fabrica_cronograma);
                this.logistica_cronograma = this.subetapas.Max(x => x.logistica_cronograma);
                this.montagem_cronograma = this.subetapas.Max(x => x.montagem_cronograma);

                var eng = this.subetapas.FindAll(x => x.engenharia_cronograma_inicio != new DateTime());
                var fab = this.subetapas.FindAll(x => x.fabrica_cronograma_inicio != new DateTime());
                var log = this.subetapas.FindAll(x => x.fabrica_cronograma_inicio != new DateTime());
                var mont = this.subetapas.FindAll(x => x.montagem_cronograma_inicio != new DateTime());

                if (eng.Count > 0)
                {
                    this.engenharia_cronograma_inicio = eng.Min(x => x.engenharia_cronograma_inicio);
                }
                if (fab.Count > 0)
                {
                    this.fabrica_cronograma_inicio = fab.Min(x => x.fabrica_cronograma_inicio);
                }
                if (log.Count > 0)
                {
                    this.logistica_cronograma_inicio = log.Min(x => x.logistica_cronograma_inicio);
                }
                if (mont.Count > 0)
                {
                    this.montagem_cronograma_inicio = mont.Min(x => x.montagem_cronograma_inicio);
                }
                this.atraso_embarque = this.subetapas.FindAll(x => x.atraso_embarque > 0).Count;
                this.atraso_engenharia = this.subetapas.FindAll(x => x.atraso_engenharia > 0).Count;
                this.atraso_fabrica = this.subetapas.FindAll(x => x.atraso_fabrica > 0).Count;
                this.atraso_montagem = this.subetapas.FindAll(x => x.atraso_montagem > 0).Count;


                this.engenharia_previsto = Math.Round(this.subetapas.Sum(x => x.engenharia_previsto / this.subetapas.Count), 2);
                this.fabrica_previsto = Math.Round(this.subetapas.Sum(x => x.fabrica_previsto / this.subetapas.Count), 2);
                this.montagem_previsto = Math.Round(this.subetapas.Sum(x => x.montagem_previsto / this.subetapas.Count), 2);
                this.embarque_previsto = Math.Round(this.subetapas.Sum(x => x.logistica_previsto / this.subetapas.Count), 2);


                this.peso_produzido = Math.Round(this.subetapas.Sum(x => x.peso_produzido), 2);
                this.peso_embarcado = Math.Round(this.subetapas.Sum(x => x.peso_embarcado), 2);
                this.peso_montado = Math.Round(this.subetapas.Sum(x => x.peso_montado), 2);
                this.peso_planejado = Math.Round(this.subetapas.Sum(x => x.peso_planejado), 2);

                this.liberado_engenharia = Math.Round(this.subetapas.Sum(x => x.liberado_engenharia / this.subetapas.Count), 2);
                
                this.total_embarcado = Math.Round(this.subetapas.Sum(x => x.peso_embarcado) / this.subetapas.Sum(x => x.peso_planejado) * 100, 2);
                this.total_fabricado = Math.Round(this.subetapas.Sum(x => x.peso_produzido) / this.subetapas.Sum(x => x.peso_planejado) * 100, 2);
                this.total_montado = Math.Round(this.subetapas.Sum(x => x.peso_montado) / this.subetapas.Sum(x => x.peso_planejado) * 100, 2);

                this.ultima_consulta_sap = this.subetapas.Max(x => x.ultima_consulta_sap);

                this.dados_montagem = this.subetapas.FindAll(x => x.dados_montagem).Count > 0;

                this.resumo_pecas = new Resumo_Pecas();
                this.resumo_pecas.pep = this.etapa;
                this.resumo_pecas.fases = this.subetapas.Sum(x => x.resumo_pecas.fases);
                this.resumo_pecas.peso_necessario = this.subetapas.Sum(x => x.resumo_pecas.peso_necessario);
                this.resumo_pecas.qtd_embarcada = this.subetapas.Sum(x => x.resumo_pecas.qtd_embarcada);
                this.resumo_pecas.qtd_necessaria = this.subetapas.Sum(x => x.resumo_pecas.qtd_necessaria);
                this.resumo_pecas.qtd_produzida = this.subetapas.Sum(x => x.resumo_pecas.qtd_produzida);
                this.resumo_pecas.subfases = this.subetapas.Sum(x => x.resumo_pecas.subfases);

                this.resumo_pecas.etapa_bloqueada = (this.subetapas.FindAll(x => x.resumo_pecas.etapa_bloqueada).Count == this.subetapas.Count);

                var MIN = Conexoes.Utilz.Calendario.DataDummy();
                var D0 = this.subetapas.Select(X => X.resumo_pecas.Inicio).ToList().FindAll(X => X > MIN).OrderBy(x => x);
                var D1 = this.subetapas.Select(X => X.resumo_pecas.Fim).ToList().FindAll(X => X > MIN).OrderBy(x => x);

                if (D0.Count() > 0)
                {
                    this.resumo_pecas.Inicio = D0.First();
                }
                if (D1.Count() > 0)
                {
                    this.resumo_pecas.Fim = D1.Last();
                }

                var apontamentos = this.subetapas.FindAll(x => x.update_montagem != "").Distinct().ToList().Select(x => Conexoes.Extensoes.Data(x.update_montagem.ToUpper().Replace(" ", "").Replace("MONTAGEM:", "")));
                if (apontamentos.Count() > 0)
                {
                    this.update_montagem = apontamentos.Max().ToShortDateString();

                }
            }
        }
    }
}
