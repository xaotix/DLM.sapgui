using DLM.painel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    /*CUSTOS - REALIZADO*/
   public class FAGLL03
    {
        public string GetRow()
        {
            return ("(" + string.Join("", GetLinha().Celulas.Select(x => "'" + x.Valor.Replace("'", "") + "',")) + ")").Replace(",)", ")");
        }
        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("Pedido", this.Pedido);

            l.Add("Atribuicao", this.Atribuicao);
            l.Add("N_documento", this.N_documento);
            l.Add("Divisao", this.Divisao);
            l.Add("Tipo_de_documento", this.Divisao);
            l.Add("Data_do_documento", this.Data_do_documento);
            l.Add("Chave_de_lancamento", this.Chave_de_lancamento);
            l.Add("Montante_em_moeda_interna", this.Montante_em_moeda_interna);
            l.Add("Moeda_interna", this.Moeda_interna);
            l.Add("Centro_de_lucro", this.Centro_de_lucro);
            l.Add("Segmento", this.Segmento);
            l.Add("Texto", this.Texto);
            l.Add("BeneficiamentoCO", this.BeneficiamentoCO);
            l.Add("GGF_CO", this.GGF_CO);
            l.Add("MaterialCO", this.MaterialCO);
            l.Add("MOD_CO", this.MOD_CO);
            l.Add("SubContratacaoCO", this.SubContratacaoCO);
            l.Add("Periodo_contábil", this.Periodo_contábil);
            l.Add("Elemento_PEP", this.Elemento_PEP);
            l.Add("Data_de_lancamento", this.Data_de_lancamento);


            return l;
        }
        public string Pedido { get; set; } = "";
        public string Atribuicao { get; set; } = "";
        public string N_documento { get; set; } = "";
        public string Divisao { get; set; } = "";
        public string Tipo_de_documento { get; set; } = "";
        public string Data_do_documento { get; set; } = "";
        public string Chave_de_lancamento { get; set; } = "";
        public string Montante_em_moeda_interna { get; set; } = "";
        public string Moeda_interna { get; set; } = "";
        public string Centro_de_lucro { get; set; } = "";
        public string Segmento { get; set; } = "";
        public string Texto { get; set; } = "";
        public string BeneficiamentoCO { get; set; } = "";
        public string GGF_CO { get; set; } = "";
        public string MaterialCO { get; set; } = "";
        public string MOD_CO { get; set; } = "";
        public string SubContratacaoCO { get; set; } = "";
        public string Periodo_contábil { get; set; } = "";
        public string Elemento_PEP { get; set; } = "";
        public string Data_de_lancamento { get; set; } = "";


        public FAGLL03()
        {

        }
        public FAGLL03(string Pedido, DLM.db.Linha l)
        {
            this.Pedido = Pedido;

            this.Atribuicao = l[Colunas.FAGLL03.Atribuicao].ToString();
            this.N_documento = l[Colunas.FAGLL03.N_documento].ToString();
            this.Divisao = l[Colunas.FAGLL03.Divisao].ToString();
            this.Tipo_de_documento = l[Colunas.FAGLL03.Tipo_de_documento].ToString();
            this.Data_do_documento = l[Colunas.FAGLL03.Data_do_documento].ToString();
            this.Chave_de_lancamento = l[Colunas.FAGLL03.Chave_de_lancamento].ToString();
            this.Montante_em_moeda_interna = l[Colunas.FAGLL03.Montante_em_moeda_interna].ToString();
            this.Moeda_interna = l[Colunas.FAGLL03.Moeda_interna].ToString();
            this.Centro_de_lucro = l[Colunas.FAGLL03.Centro_de_lucro].ToString();
            this.Segmento = l[Colunas.FAGLL03.Segmento].ToString();
            this.Texto = l[Colunas.FAGLL03.Texto].ToString();
            this.BeneficiamentoCO = l[Colunas.FAGLL03.BeneficiamentoCO].ToString();
            this.GGF_CO = l[Colunas.FAGLL03.GGF_CO].ToString();
            this.MaterialCO = l[Colunas.FAGLL03.MaterialCO].ToString();
            this.MOD_CO = l[Colunas.FAGLL03.MOD_CO].ToString();
            this.SubContratacaoCO = l[Colunas.FAGLL03.SubContratacaoCO].ToString();
            this.Periodo_contábil = l[Colunas.FAGLL03.Periodo_contábil].ToString();
            this.Elemento_PEP = l[Colunas.FAGLL03.Elemento_PEP].ToString();
            this.Data_de_lancamento = l[Colunas.FAGLL03.Data_de_lancamento].ToString();

        }
    }
}
