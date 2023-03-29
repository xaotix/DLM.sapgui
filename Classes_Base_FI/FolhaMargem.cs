using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DLM.sapgui
{
    [Serializable]
    public class FolhaMargem
    {
        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        public long id { get; set; } = -1;
        [Category("Receita")]
        [DisplayName("Líquida")]
        public double receita_liquida
        {
            get
            {
                return this.receitabruta.total.valor - this.impostos.total_deducoes;
            }
        }
        [Browsable(false)]
        public bool Carregado { get; set; } = false;
        [ReadOnly(true)]
        public string PEP { get; set; } = "";

        [Category("Outros")]
        [DisplayName("Faturamento Direto")]
        [Browsable(false)]
        public FaturamentoDireto FaturamentoDireto { get; set; } = new FaturamentoDireto();
        [Category("Outros")]
        [DisplayName("Saving")]
        [Browsable(false)]
        public Saving Saving { get; set; } = new Saving();

        [Category("Receita")]
        [DisplayName("Bruta")]
        public ReceitaBruta receitabruta { get; set; } = new ReceitaBruta();
        [Category("Receita")]
        [DisplayName("Líquida")]
        public ReceitaLiquida receitaliquida { get; set; } = new ReceitaLiquida();
        [Category("Custos")]
        [DisplayName("Impostos")]
        public Impostos impostos { get; set; } = new Impostos();
        [Category("Custos")]
        [DisplayName("Total Custo Projeto")]
        public Variavel total_custo_projeto { get; set; } = new Variavel();
        [DisplayName("Materiais")]
        [Category("Custos")]
        public CustosMateriais custosmateriais { get; set; } = new CustosMateriais();
        [DisplayName("Montagem")]
        [Category("Custos")]
        public CustosMontagem custosmontagem { get; set; } = new CustosMontagem();
        [DisplayName("Materiais")]
        [Category("Logística")]
        public GastosLogisticos gastoslogisticos { get; set; } = new GastosLogisticos();
        [DisplayName("Despesas")]
        [Category("Gerais")]
        public DespesasGerais despesasgerais { get; set; } = new DespesasGerais();
        [Category("Margens")]
        [DisplayName("Margens")]
        public Variavel margens { get; set; } = new Variavel();
        public FolhaMargem()
        {

        }

        public FolhaMargem(List<FolhaMargem> j)
        {
            this.custosmateriais.contingencia = new Variavel(j.Select(x => x.custosmateriais.contingencia).ToList());
            this.custosmateriais.materiais = new Variavel(j.Select(x => x.custosmateriais.materiais).ToList());

            this.custosmontagem.empreiteiros_despesas = new Variavel(j.Select(x => x.custosmontagem.empreiteiros_despesas).ToList());
            this.custosmontagem.equipamentos = new Variavel(j.Select(x => x.custosmontagem.equipamentos).ToList());
            this.custosmontagem.supervisao = new Variavel(j.Select(x => x.custosmontagem.supervisao).ToList());
            this.custosmontagem.equipe_propria = new Variavel(j.Select(x => x.custosmontagem.equipe_propria).ToList());

            this.despesasgerais.assessoria = new Variavel(j.Select(x => x.despesasgerais.assessoria).ToList());
            this.despesasgerais.comissao = new Variavel(j.Select(x => x.despesasgerais.comissao).ToList());
            this.despesasgerais.creditos_debitos_material = new Variavel(j.Select(x => x.despesasgerais.creditos_debitos_material).ToList());
            this.despesasgerais.creditos_debitos_montagem = new Variavel(j.Select(x => x.despesasgerais.creditos_debitos_montagem).ToList());
            this.despesasgerais.creditos_debitos_projeto = new Variavel(j.Select(x => x.despesasgerais.creditos_debitos_projeto).ToList());
            this.despesasgerais.custo_financeiro = new Variavel(j.Select(x => x.despesasgerais.custo_financeiro).ToList());
            this.despesasgerais.outros = new Variavel(j.Select(x => x.despesasgerais.outros).ToList());
            this.despesasgerais.projeto_exportacao = new Variavel(j.Select(x => x.despesasgerais.projeto_exportacao).ToList());
            this.despesasgerais.seguro = new Variavel(j.Select(x => x.despesasgerais.seguro).ToList());
            this.despesasgerais.supervisao_exportacao = new Variavel(j.Select(x => x.despesasgerais.supervisao_exportacao).ToList());

            this.receitabruta.material = new Variavel(j.Select(x => x.receitabruta.material).ToList());
            this.receitabruta.montagem = new Variavel(j.Select(x => x.receitabruta.montagem).ToList());
            this.receitabruta.projeto = new Variavel(j.Select(x => x.receitabruta.projeto).ToList());

            this.receitaliquida.material = new Variavel(j.Select(x => x.receitaliquida.material).ToList());
            this.receitaliquida.montagem = new Variavel(j.Select(x => x.receitaliquida.montagem).ToList());
            this.receitaliquida.projeto = new Variavel(j.Select(x => x.receitaliquida.projeto).ToList());

            this.total_custo_projeto = new Variavel(j.Select(x => x.receitaliquida.projeto).ToList());

            this.PEP = string.Join(" - ", j.Select(x => x.PEP));

            this.FaturamentoDireto = new FaturamentoDireto(j.Select(x => x.FaturamentoDireto).ToList());

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class ReceitaBruta
    {
        [XmlIgnore]
        public Variavel total
        {
            get
            {
                return new Variavel() { valor = material.valor + montagem.valor + projeto.valor, porcentagem = material.porcentagem + montagem.porcentagem + projeto.porcentagem };
            }
        }
        public override string ToString()
        {
            return " Material " + material.ToString() + "/ Montagem " + montagem.ToString() + "/ Projeto " + projeto.ToString();
        }

        [DisplayName("Material")]
        public Variavel material { get; set; } = new Variavel();

        [DisplayName("Montagem")]
        public Variavel montagem { get; set; } = new Variavel();

        [DisplayName("Projeto")]
        public Variavel projeto { get; set; } = new Variavel();
        public ReceitaBruta()
        {

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class FaturamentoDireto
    {
        [Category("Receitas")]
        [DisplayName("Bruta Projeto")]
        public double receitabrutaprojeto { get; set; } = 0;
        [Category("Receitas")]
        [DisplayName("Bruta Materiais")]
        public double receitabrutamateriais { get; set; } = 0;
        [Category("Receitas")]
        [DisplayName("Bruta Montagem")]
        public double receitabrutamontagem { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("MP")]
        public double custosmp { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Logística")]
        public double custoslogistica { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem MO DI")]
        public double custosmontagem_mo_di { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Equipamentos")]
        public double custosmontagemequipamentos { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Supervisor")]
        public double custosmontagemsupervisor { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Equipe Própria")]
        public double custosmontagemequipe_propria { get; set; } = 0;
        public FaturamentoDireto()
        {

        }
        public FaturamentoDireto(List<FaturamentoDireto> l)
        {
            this.custoslogistica = l.Sum(x => x.custoslogistica);
            this.custosmontagemequipamentos = l.Sum(x => x.custosmontagemequipamentos);
            this.custosmontagemequipe_propria = l.Sum(x => x.custosmontagemequipe_propria);
            this.custosmontagemsupervisor = l.Sum(x => x.custosmontagemsupervisor);
            this.custosmontagem_mo_di = l.Sum(x => x.custosmontagem_mo_di);
            this.custosmp = l.Sum(x => x.custosmp);
            this.receitabrutamateriais = l.Sum(x => x.receitabrutamateriais);
            this.receitabrutamontagem = l.Sum(x => x.receitabrutamontagem);
            this.receitabrutaprojeto = l.Sum(x => x.receitabrutaprojeto);
        }
    }

    [Serializable]
    [ExpandableObject()]
    public class Saving
    {
        [Category("Receitas")]
        [DisplayName("Bruta Projeto")]
        public double receitabrutaprojeto { get; set; } = 0;
        [Category("Receitas")]
        [DisplayName("Bruta Materiais")]
        public double receitabrutamateriais { get; set; } = 0;
        [Category("Receitas")]
        [DisplayName("Bruta Montagem")]
        public double receitabrutamontagem { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("MP")]
        public double custosmp { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Logística")]
        public double custoslogistica { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem MO DI")]
        public double custosmontagem_mo_di { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Equipamentos")]
        public double custosmontagemequipamentos { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Supervisor")]
        public double custosmontagemsupervisor { get; set; } = 0;
        [Category("Custos")]
        [DisplayName("Montagem Equipe Própria")]
        public double custosmontagemequipe_propria { get; set; } = 0;
        public Saving()
        {

        }
        public Saving(List<Saving> l)
        {
            this.custoslogistica = l.Sum(x => x.custoslogistica);
            this.custosmontagemequipamentos = l.Sum(x => x.custosmontagemequipamentos);
            this.custosmontagemequipe_propria = l.Sum(x => x.custosmontagemequipe_propria);
            this.custosmontagemsupervisor = l.Sum(x => x.custosmontagemsupervisor);
            this.custosmontagem_mo_di = l.Sum(x => x.custosmontagem_mo_di);
            this.custosmp = l.Sum(x => x.custosmp);
            this.receitabrutamateriais = l.Sum(x => x.receitabrutamateriais);
            this.receitabrutamontagem = l.Sum(x => x.receitabrutamontagem);
            this.receitabrutaprojeto = l.Sum(x => x.receitabrutaprojeto);
        }
    }
    [Serializable]
    [ExpandableObject()]
    public class Impostos
    {
        [XmlIgnore]

        [DisplayName("Total de Deduções")]
        public double total_deducoes
        {
            get
            {
                return IPI_Material.valor
                    + ICMS_Material.valor
                    + PIS_COFINS_Material.valor
                    + PIS_COFINS_Montagem.valor
                    + PIS_COFINS_Projeto.valor
                    + ISS_Locacao_Equipamentos.valor
                    + ISS_Supervisao.valor
                    + ISS_Montagem.valor
                    + ISS_Projeto.valor
                    + CPRB_Servico.valor
                    + CPRB_Material.valor;
            }
        }

        [DisplayName("IPI Material")]
        public Variavel IPI_Material { get; set; } = new Variavel();

        [DisplayName("ICMS Material")]
        public Variavel ICMS_Material { get; set; } = new Variavel();

        [DisplayName("PS COFINS Material")]
        public Variavel PIS_COFINS_Material { get; set; } = new Variavel();

        [DisplayName("PIS COFINS Montagem")]
        public Variavel PIS_COFINS_Montagem { get; set; } = new Variavel();

        [DisplayName("PIS COFINS Projeto")]
        public Variavel PIS_COFINS_Projeto { get; set; } = new Variavel();

        [DisplayName("ISS Locação Equipamentos")]
        public Variavel ISS_Locacao_Equipamentos { get; set; } = new Variavel();

        [DisplayName("ISS Supervisão")]
        public Variavel ISS_Supervisao { get; set; } = new Variavel();

        [DisplayName("ISS Montagem")]
        public Variavel ISS_Montagem { get; set; } = new Variavel();

        [DisplayName("ISS Projeto")]
        public Variavel ISS_Projeto { get; set; } = new Variavel();

        [DisplayName("CPRB Serviço")]
        public Variavel CPRB_Servico { get; set; } = new Variavel();

        [DisplayName("CPRB Material")]
        public Variavel CPRB_Material { get; set; } = new Variavel();
        public Impostos()
        {

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class ReceitaLiquida
    {
        [XmlIgnore]

        [DisplayName("Total")]
        public Variavel total
        {
            get
            {
                return new Variavel() { valor = material.valor + montagem.valor + projeto.valor , porcentagem = material.porcentagem + montagem.porcentagem + projeto.porcentagem};
            }
        }

        [DisplayName("Material")]
        public Variavel material { get; set; } = new Variavel();

        [DisplayName("Montagem")]
        public Variavel montagem { get; set; } = new Variavel();

        [DisplayName("Projeto")]
        public Variavel projeto { get; set; } = new Variavel();
        public ReceitaLiquida()
        {

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class CustosMateriais
    {

        [DisplayName("Materiais")]
        public Variavel materiais { get; set; } = new Variavel();

        [DisplayName("Contingência")]
        public Variavel contingencia { get; set; } = new Variavel();
        public CustosMateriais()
        {

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class CustosMontagem
    {
        [XmlIgnore]
        [Category("Custos Montagem")]
        [DisplayName("Total")]
        [ReadOnly(true)]
        [Browsable(false)]
        public double total
        {
            get
            {
                return equipamentos.valor + empreiteiros_despesas.valor + supervisao.valor + equipe_propria.valor;
            }
        }
        [XmlIgnore]

        [DisplayName("Porcentagem Equipamentos")]
        public double equipamentos_porcentagem
        {
            get
            {
                return equipamentos.valor / total;
            }
        }
        [XmlIgnore]

        [DisplayName("Porcentagem Emprenteiros")]
        public double empreiteiros_despesas_porcentagem
        {
            get
            {
                return empreiteiros_despesas.valor / total;
            }
        }
        [XmlIgnore]
   
        [DisplayName("Porcentagem Supervisão")]
        public double supervisao_porcentagem
        {
            get
            {
                return supervisao.valor / total;
            }
        }

        [XmlIgnore]

        [DisplayName("Porcentagem Supervisão")]
        public double equipe_propria_porcentagem
        {
            get
            {
                return equipe_propria.valor / total;
            }
        }

        [DisplayName("Equipamentos")]
        public Variavel equipamentos { get; set; } = new Variavel();

        [DisplayName("Equipe próripa")]
        public Variavel equipe_propria { get; set; } = new Variavel();

        [DisplayName("Empreiteiros")]
        public Variavel empreiteiros_despesas { get; set; } = new Variavel();

        [DisplayName("Supervisão")]
        public Variavel supervisao { get; set; } = new Variavel();
        public CustosMontagem()
        {

        }
    }
    [Serializable]
    [ExpandableObject()]
    public class GastosLogisticos
    {
        [XmlIgnore]
        [DisplayName("Total")]
        public Variavel total
        {
            get
            {
                return new Variavel()
                {
                    valor =
                    frete_aereo.valor +
                    verba_adicional.valor +
                    gastos_logisticos.valor +
                    outras_despesas.valor +
                    seguro_internacional.valor +
                    frete_maritimo.valor +
                    frete_rodoviario_internacional.valor +
                    frete_rodoviario_nacional_cabotagem.valor +
                    frete_maritimo_cabotagem.valor +
                    frete_rodoviario_nacional_exportacao.valor +
                    frete_rodoviario.valor
                };
            }
        }

        [DisplayName("Frete Rodoviário")]
        public Variavel frete_rodoviario { get; set; } = new Variavel();

        [DisplayName("Verba adicional")]
        public Variavel verba_adicional { get; set; } = new Variavel();

        [DisplayName("Gastos Logísticos")]
        public Variavel gastos_logisticos { get; set; } = new Variavel();

        [DisplayName("Outras Despesas")]
        public Variavel outras_despesas { get; set; } = new Variavel();

        [DisplayName("Seguro Internacional")]
        public Variavel seguro_internacional { get; set; } = new Variavel();

        [DisplayName("Frete Aéreo")]
        public Variavel frete_aereo { get; set; } = new Variavel();

        [DisplayName("Frete Marítimo")]
        public Variavel frete_maritimo { get; set; } = new Variavel();

        [DisplayName("Frete Rodoviário Internacional")]
        public Variavel frete_rodoviario_internacional { get; set; } = new Variavel();

        [DisplayName("Frete Rodoviário Nacional Exportação")]
        public Variavel frete_rodoviario_nacional_exportacao { get; set; } = new Variavel();

        [DisplayName("Frete Marítimo Cabotagem")]
        public Variavel frete_maritimo_cabotagem { get; set; } = new Variavel();
      
        [DisplayName("Frete Rodoviário Nacional Cabotagem")]
        public Variavel frete_rodoviario_nacional_cabotagem { get; set; } = new Variavel();

        public GastosLogisticos()
        {

        }

    }
    [Serializable]
    [ExpandableObject()]
    public class DespesasGerais
    {
      
        [DisplayName("Seguro")]
        public Variavel seguro { get; set; } = new Variavel();
   
        [DisplayName("Comissão")]
        public Variavel comissao { get; set; } = new Variavel();
    
        [DisplayName("Assessoria")]
        public Variavel assessoria { get; set; } = new Variavel();
       
        [DisplayName("Custo Financeiro")]
        public Variavel custo_financeiro { get; set; } = new Variavel();
       
        [DisplayName("Supervisão Exportação")]
        public Variavel supervisao_exportacao { get; set; } = new Variavel();
    
        [DisplayName("Créditos / Débitos Material")]
        public Variavel creditos_debitos_material { get; set; } = new Variavel();

        [DisplayName("Créditos / Débitos Projeto")]
        public Variavel creditos_debitos_projeto { get; set; } = new Variavel();
    
        [DisplayName("Créditos / Débitos Montagem")]
        public Variavel creditos_debitos_montagem { get; set; } = new Variavel();

        [DisplayName("Projeto Exportação")]
        public Variavel projeto_exportacao { get; set; } = new Variavel();

        [DisplayName("Outros")]
        public Variavel outros { get; set; } = new Variavel();

       public DespesasGerais()
        {

        }

    }
    [Serializable]
    [ExpandableObject()]
    public class Variavel
    {
        public override string ToString()
        {
            return (valor > 0 ? "[Valor :" + valor + "]" : "") + (porcentagem > 0 ? "[Porcentagem :" + porcentagem + "]" : "");
        }
        [DisplayName("Valor")]
        public double valor { get; set; } = 0;
        [DisplayName("Porcentagem")]
        public double porcentagem { get; set; } = 0;
        public Variavel()
        {

        }

        public Variavel(List<Variavel> juntar)
        {
            valor = juntar.Sum(x => x.valor);
            porcentagem = juntar.Max(x => x.valor);
        }
    }
}
