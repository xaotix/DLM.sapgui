using DLM.painel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    /*RECEITA BRUTA REALIZADA*/
    public class ZCONTRATOS
    {
        public override string ToString()
        {
            return this.Elemento_PEP;
        }
        public string GetRow()
        {
            return ("(" + string.Join("", GetLinha().Celulas.Select(x => "'" + x.Valor + "',")) + ")").Replace(",)", ")");
        }


        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("Empresa", this.Empresa);
            l.Add("Cen", this.Cen);
            l.Add("Cen", this.Cen);
            l.Add("Contrato", this.Contrato);
            l.Add("Cliente", this.Cliente);
            l.Add("CNPJ", this.CNPJ);
            l.Add("Razao_Social_Cliente", this.Razao_Social_Cliente);
            l.Add("SetInd", this.SetInd);
            l.Add("UF", this.UF);
            l.Add("Situacao", this.Situacao);
            l.Add("Devolucoes", this.Devolucoes);
            l.Add("Nome_da_obra", this.Nome_da_obra);
            l.Add("Elemento_PEP", this.Elemento_PEP);
            l.Add("Contas_a_receber", this.Contas_a_receber);
            l.Add("Receita", this.Receita);
            l.Add("Cotacao", this.Cotacao);
            l.Add("Itm", this.Itm);
            l.Add("Ordem_venda", this.Ordem_venda);
            l.Add("TpDV", this.TpDV);
            l.Add("Fatura", this.Fatura);
            l.Add("TipFt", this.TipFt);
            l.Add("Descr_tipo_fat", this.Descr_tipo_fat);
            l.Add("NF", this.NF);
            l.Add("Data_emissao", this.Data_emissao);
            l.Add("Material", this.Material);
            l.Add("CFOP", this.CFOP);
            l.Add("Quantidade", this.Quantidade);
            l.Add("UM", this.UM);
            l.Add("Valor_unit", this.Valor_unit);
            l.Add("Peso_liq", this.Peso_liq);
            l.Add("Valor_total_NF", this.Valor_total_NF);
            return l;
        }
        public string Empresa {get;set;}  =   "";
        public string Cen {get;set;} =  "";
        public string Contrato {get;set;} =  "";
        public string Cliente {get;set;} =  "";
        public string CNPJ {get;set;} =  "";
        public string Razao_Social_Cliente {get;set;} =  "";
        public string SetInd {get;set;} =  "";
        public string UF {get;set;} =  "";
        public string Situacao {get;set;} =  "";
        public string Devolucoes {get;set;} =  "";
        public string Nome_da_obra {get;set;} =  "";
        public string Elemento_PEP {get;set;} =  "";
        public string Contas_a_receber {get;set;} =  "";
        public string Receita {get;set;} =  "";
        public string Cotacao {get;set;} =  "";
        public string Itm {get;set;} =  "";
        public string Ordem_venda {get;set;} =  "";
        public string TpDV {get;set;} =  "";
        public string Fatura {get;set;} =  "";
        public string TipFt {get;set;} =  "";
        public string Descr_tipo_fat {get;set;} =  "";
        public string NF {get;set;} =  "";
        public string Data_emissao {get;set;} =  "";
        public string Material {get;set;} =  "";
        public string CFOP {get;set;} =  "";
        public string Quantidade {get;set;} =  "";
        public string UM {get;set;} =  "";
        public string Valor_unit {get;set;} =  "";
        public string Peso_liq {get;set;} =  "";
        public string Valor_total_NF {get;set;} =  "";
        public ZCONTRATOS(DLM.db.Linha l)
        {
            this.Empresa = l[(int)TAB_ZCONTRATOS.Empresa].ToString();
            this.Cen = l[(int)TAB_ZCONTRATOS.Cen].ToString();
            this.Contrato = l[(int)TAB_ZCONTRATOS.Contrato].ToString();
            this.Cliente = l[(int)TAB_ZCONTRATOS.Cliente].ToString();
            this.CNPJ = l[(int)TAB_ZCONTRATOS.CNPJ].ToString();
            this.Razao_Social_Cliente = l[(int)TAB_ZCONTRATOS.Razao_Social_Cliente].ToString();
            this.SetInd = l[(int)TAB_ZCONTRATOS.SetInd].ToString();
            this.UF = l[(int)TAB_ZCONTRATOS.UF].ToString();
            this.Situacao = l[(int)TAB_ZCONTRATOS.Situacao].ToString();
            this.Devolucoes = l[(int)TAB_ZCONTRATOS.Devolucoes].ToString();
            this.Nome_da_obra = l[(int)TAB_ZCONTRATOS.Nome_da_obra].ToString();
            this.Elemento_PEP = CargaExcel.TratarPEP(l[(int)TAB_ZCONTRATOS.Elemento_PEP].ToString());
            this.Contas_a_receber = l[(int)TAB_ZCONTRATOS.Contas_a_receber].ToString();
            this.Receita = l[(int)TAB_ZCONTRATOS.Receita].ToString();
            this.Cotacao = l[(int)TAB_ZCONTRATOS.Cotacao].ToString();
            this.Itm = l[(int)TAB_ZCONTRATOS.Itm].ToString();
            this.Ordem_venda = l[(int)TAB_ZCONTRATOS.Ordem_venda].ToString();
            this.TpDV = l[(int)TAB_ZCONTRATOS.TpDV].ToString();
            this.Fatura = l[(int)TAB_ZCONTRATOS.Fatura].ToString();
            this.TipFt = l[(int)TAB_ZCONTRATOS.TipFt].ToString();
            this.Descr_tipo_fat = l[(int)TAB_ZCONTRATOS.Descr_tipo_fat].ToString();
            this.NF = l[(int)TAB_ZCONTRATOS.NF].ToString();
            this.Data_emissao = l[(int)TAB_ZCONTRATOS.Data_emissao].ToString();
            this.Material = l[(int)TAB_ZCONTRATOS.Material].ToString();
            this.CFOP = l[(int)TAB_ZCONTRATOS.CFOP].ToString();
            this.Quantidade = l[(int)TAB_ZCONTRATOS.Quantidade].ToString();
            this.UM = l[(int)TAB_ZCONTRATOS.UM].ToString();
            this.Valor_unit = l[(int)TAB_ZCONTRATOS.Valor_unit].ToString();
            this.Peso_liq = l[(int)TAB_ZCONTRATOS.Peso_liq].ToString();
            this.Valor_total_NF = l[(int)TAB_ZCONTRATOS.Valor_total_NF].ToString();
        }
    }
}
