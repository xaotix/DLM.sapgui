using Conexoes;
using System;
using DLM.vars;

namespace DLM.sapgui
{
    public class ZPP0100
    {
        //public string booleano { get; set; } = "";
        public PEP_Planejamento PEP { get; set; } = new PEP_Planejamento();
        public int Ordem_Embarque { get; set; } = 0;
        public double Qtd_Embarque { get; set; } = 0;
        public double Nro_Carga { get; set; } = 0;
        public bool Carregado => this.St_Conf_.ToUpper() == Cfg.Init.ZPP0100_CARGA_CONFIRMADA;
        public string St_Embarque { get; set; } = "";
        public string St_Carga { get; set; } = "";
        public string Etq_Material { get; set; } = "";
        public string Material { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string Tamanho_dimensao { get; set; } = "";
        public double Comprimento { get; set; } = 0;
        public double Peso_item_Tot { get; set; } = 0;
        public string Nome_da_Obra { get; set; } = "";
        public string Elemento_PEP { get; set; } = "";
        public int Centro { get; set; } = 0;
        public int CentroProducao { get; set; } = 0;

        public bool Etq_Impressa { get; set; } = false;
        public string Etq_Volume { get; set; } = "";
        public string Status { get; set; } = "";
        public double Qtd_Carregada { get; set; } = 0;
        public double Sld_1202 { get; set; } = 0;
        public double Sld_1203 { get; set; } = 0;
        public double Sld_1204 { get; set; } = 0;
        public string St_Conf_ { get; set; } = "";
        public string St_DtProg_ { get; set; } = "";
        public DateTime Data { get; set; } = Cfg.Init.DataDummy();
        public string Ordem_Prod_ { get; set; } = "";
        public string Apontamento_Fert { get; set; } = "";
        public string End_Logistico { get; set; } = "";
        public string N_do_item { get; set; } = "";
        public string Sequencia_Item { get; set; } = "";
        public string Ordem_Venda { get; set; } = "";
        public string Remessa { get; set; } = "";
        public int Nota_Fiscal { get; set; } = 0;
        public DateTime Data_NF { get; set; } = Cfg.Init.DataDummy();
        public DLM.db.Linha GetLinha()
        {

            var l = new DLM.db.Linha();
            //l.Add("booleano", this.booleano);
            l.Add("Elemento_PEP", this.Elemento_PEP);
            l.Add("Ordem_Embarque", this.Ordem_Embarque);
            if (this.Qtd_Embarque > 0)
            {
                l.Add("Qtd_Embarque", this.Qtd_Embarque);
            }
            if (this.Qtd_Carregada > 0)
            {
                l.Add("Qtd_Carregada", this.Qtd_Carregada);
            }
            l.Add("Material", this.Material);
            if (this.Peso_item_Tot > 0)
            {
                l.Add("Peso_item_Tot", this.Peso_item_Tot);
            }

            l.Add("Nro_Carga", this.Nro_Carga);
            if (St_Embarque != "")
            {
                l.Add("St_Embarque", this.St_Embarque);
            }
            if (St_Carga != "")
            {
                l.Add("St_Carga", this.St_Carga);
            }
            if (Etq_Material != "")
            {
                l.Add("Etq_Material", this.Etq_Material);
            }
            l.Add("Descricao", this.Descricao);
            if (Tamanho_dimensao != "")
            {
                l.Add("Tamanho_dimensao", this.Tamanho_dimensao);
            }
            if (Comprimento > 0)
            {
                l.Add("Comprimento", this.Comprimento);
            }
            //l.Add("Nome_da_Obra", this.Nome_da_Obra);
            l.Add("Centro", this.Centro);
            if (this.CentroProducao > 0)
            {
                l.Add("CentroProducao", this.CentroProducao);
            }

            if (Etq_Impressa)
            {
                l.Add("Etq_Impressa", this.Etq_Impressa);
            }
            if (Etq_Volume != "")
            {
                l.Add("Etq_Volume", this.Etq_Volume);
            }

            l.Add("Status", this.Status);
            if (Sld_1202 > 0)
            {
                l.Add("Sld_1202", this.Sld_1202);
            }
            if (Sld_1203 > 0)
            {
                l.Add("Sld_1203", this.Sld_1203);
            }
            if (Sld_1204 > 0)
            {
                l.Add("Sld_1204", this.Sld_1204);
            }
            if (this.St_Conf_ != "")
            {
                l.Add("St_Conf_", this.St_Conf_);
            }

            l.Add("St_DtProg_", this.St_DtProg_);
            if (this.Data > new DateTime(2001, 01, 01))
            {
                l.Add("Data", this.Data);
            }

            if (Ordem_Prod_ != "")
            {
                l.Add("Ordem_Prod_", this.Ordem_Prod_);
            }
            l.Add("Apontamento_Fert", this.Apontamento_Fert);


            if (this.Data_NF > new DateTime(2001, 01, 01))
            {
                l.Add("Data_NF", this.Data_NF);
            }

            if (Nota_Fiscal > 0)
            {
                l.Add("Nota_Fiscal", this.Nota_Fiscal);
            }

            return l;
        }
        public ZPP0100()
        {

        }
        public ZPP0100(DLM.db.Linha l)
        {
            this.Centro = l[(int)TAB_ZPP0100.Centro].Int();
            this.CentroProducao = l[(int)TAB_ZPP0100.CentroProducao].Int();

            this.Elemento_PEP = CargaExcel.TratarPEP(l[(int)TAB_ZPP0100.Elemento_PEP].Valor);
            this.PEP = new PEP_Planejamento(this.Elemento_PEP.Replace(" ", ""));
            this.Ordem_Embarque = l[(int)TAB_ZPP0100.Ordem_Embarque].Int();
            this.Qtd_Embarque = l[(int)TAB_ZPP0100.Qtd_Embarque].Double();
            this.Nro_Carga = l[(int)TAB_ZPP0100.Nro_Carga].Double();
            this.St_Embarque = l[(int)TAB_ZPP0100.St_Embarque].Valor;
            this.St_Carga = l[(int)TAB_ZPP0100.St_Carga].Valor;
            this.Etq_Material = l[(int)TAB_ZPP0100.Etq_Material].Valor;
            this.Material = l[(int)TAB_ZPP0100.Material].Valor;
            this.Descricao = l[(int)TAB_ZPP0100.Descricao].Valor.CortarString(140);
            this.Tamanho_dimensao = l[(int)TAB_ZPP0100.Tamanho_dimensao].Valor;
            this.Comprimento = l[(int)TAB_ZPP0100.Comprimento].Double();
            this.Peso_item_Tot = l[(int)TAB_ZPP0100.Peso_item_Tot].Double();
            this.Nome_da_Obra = l[(int)TAB_ZPP0100.Nome_da_Obra].Valor;

            this.Etq_Impressa = l[(int)TAB_ZPP0100.Etq_Impressa].Boolean();
            this.Etq_Volume = l[(int)TAB_ZPP0100.Etq_Volume].Valor;
            this.Status = l[(int)TAB_ZPP0100.Status].Valor;
            this.Qtd_Carregada = l[(int)TAB_ZPP0100.Qtd_Carregada].Double();
            this.Sld_1202 = l[(int)TAB_ZPP0100.Sld_1202].Double();
            this.Sld_1203 = l[(int)TAB_ZPP0100.Sld_1203].Double();
            this.Sld_1204 = l[(int)TAB_ZPP0100.Sld_1204].Double();
            this.St_Conf_ = l[(int)TAB_ZPP0100.St_Conf_].Valor;
            this.St_DtProg_ = l[(int)TAB_ZPP0100.St_DtProg_].Valor;
            this.Data = l[(int)TAB_ZPP0100.Data].Data();
            this.Ordem_Prod_ = l[(int)TAB_ZPP0100.Ordem_Prod_].Valor;
            this.Apontamento_Fert = l[(int)TAB_ZPP0100.Apontamento_Fert].Valor;



            this.End_Logistico = l[(int)TAB_ZPP0100.End_Logistico].Valor;
            this.N_do_item = l[(int)TAB_ZPP0100.N_do_item].Valor;
            this.Sequencia_Item = l[(int)TAB_ZPP0100.Sequencia_Item].Valor;
            this.Ordem_Venda = l[(int)TAB_ZPP0100.Ordem_Venda].Valor;
            this.Remessa = l[(int)TAB_ZPP0100.Remessa].Valor;
            this.Nota_Fiscal = l[(int)TAB_ZPP0100.Nota_Fiscal].Int();
            this.Data_NF = l[(int)TAB_ZPP0100.Data_NF].Data();
        }
    }
}
