using Conexoes;
using System;
using DLM.vars;

namespace DLM.sapgui
{
    public class ZPP0100
    {
        public override string ToString()
        {
            return $"[{PEP}] - {Material} - [{Marca}] [{Descricao}]";
        }
        public long POSNR { get; set; }
        public string PEP { get; set; } = "";
        public string Material { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Descricao { get; set; } = "";

        public int Ordem_Embarque { get; set; } = 0;
        public double Qtd_Embarque { get; set; } = 0;
        public int Nro_Carga { get; set; } = 0;
        public bool Carregado => this.St_Conf_.ToUpper() == Cfg.Init.ZPP0100_CARGA_CONFIRMADA;
        public string Etq_Material { get; set; } = "";
        public double Comprimento { get; set; } = 0;
        public double Peso_Tot { get; set; } = 0;
        public int Centro { get; set; } = 0;
        public int CentroProducao { get; set; } = 0;
        public string Etq_Volume { get; set; } = "";
        public double Qtd_Carregada { get; set; } = 0;
        public string St_Conf_ { get; set; } = "";
        public string Ordem_Prod_ { get; set; } = "";
        public string Apontamento_Fert { get; set; } = "";
        public int Nota_Fiscal { get; set; } = 0;
        public DateTime? Data { get; set; }
        public DateTime? Data_NF { get; set; }
        public DLM.db.Linha GetLinha()
        {

            var l = new DLM.db.Linha();
            l.Add("posnr", this.POSNR);
            l.Add("Elemento_PEP", this.PEP);
            l.Add("Material", this.Material);

            l.Add("Ordem_Embarque", this.Ordem_Embarque);
            l.Add("Qtd_Embarque", this.Qtd_Embarque);
            l.Add("Qtd_Carregada", this.Qtd_Carregada);
            l.Add("Peso_item_Tot", this.Peso_Tot);

            l.Add("Nro_Carga", this.Nro_Carga);

            l.Add("Etq_Material", this.Etq_Material);
            l.Add("Descricao", this.Descricao);
            l.Add("Tamanho_dimensao", this.Marca);
            l.Add("Comprimento", this.Comprimento);

            l.Add("Centro", this.Centro);
            l.Add("CentroProducao", this.CentroProducao);

            l.Add("Etq_Volume", this.Etq_Volume);


            l.Add("St_Conf_", this.St_Conf_);

            l.Add("Data", this.Data);



            l.Add("Data_NF", this.Data_NF);
            l.Add("Nota_Fiscal", this.Nota_Fiscal);
            l.SetNullIfZero();
            return l;
        }
        public ZPP0100()
        {

        }
        public ZPP0100(DLM.db.Linha l, bool avanco = false)
        {
            if (avanco)
            {
                //this.Elemento_PEP = CargaExcel.TratarPEP(l[""].Valor);
                //this.PEP = new PEP_Planejamento(this.Elemento_PEP.Replace(" ", ""));

                this.POSNR = l["POSNR"].Long();
                this.Material = l["MATNR"].Valor;
                this.Marca = l["GROES"].Valor;

                this.Centro = l["WERKS"].Int();
                this.CentroProducao = l["WRK02"].Int();

                this.Ordem_Embarque = l["ZEMBARQUE"].Int();
                this.Qtd_Embarque = l["QTDEPACK"].Double();
                this.Qtd_Carregada = l["QTD_CARGA"].Double();

                this.Nro_Carga = l["CARGA"].Int();
                this.Etq_Material = l["ETIQMAT"].Valor;
                this.Descricao = l["MAKTX"].Valor.Esquerda(140, true);
                this.Comprimento = l["COMPR"].Double();
                this.Peso_Tot = l["PESO_TOT"].Double();

                this.St_Conf_ = l["STAT_CONF"].Valor;
                this.Ordem_Prod_ = l["AUFNR"].Valor;
                this.Apontamento_Fert = l["STATUS_FERT"].Valor;
                this.Nota_Fiscal = l["NFENUM"].Int();
                
                this.Data = l["DATA_EMBARQUE"].DataNull();
                this.Data_NF = l["DOCDAT"].DataNull();
            }
            else
            {
                this.Centro = l[(int)TAB_ZPP0100.Centro].Int();
                this.CentroProducao = l[(int)TAB_ZPP0100.CentroProducao].Int();
                this.PEP = CargaExcel.TratarPEP(l[(int)TAB_ZPP0100.Elemento_PEP].Valor);
                this.Ordem_Embarque = l[(int)TAB_ZPP0100.Ordem_Embarque].Int();
                this.Qtd_Embarque = l[(int)TAB_ZPP0100.Qtd_Embarque].Double();
                this.Nro_Carga = l[(int)TAB_ZPP0100.Nro_Carga].Int();
                this.Etq_Material = l[(int)TAB_ZPP0100.Etq_Material].Valor;
                this.Material = l[(int)TAB_ZPP0100.Material].Valor;
                this.Descricao = l[(int)TAB_ZPP0100.Descricao].Valor.Esquerda(140, true);
                this.Marca = l[(int)TAB_ZPP0100.Tamanho_dimensao].Valor;
                this.Comprimento = l[(int)TAB_ZPP0100.Comprimento].Double();
                this.Peso_Tot = l[(int)TAB_ZPP0100.Peso_Item_Tot].Double();

                this.Qtd_Carregada = l[(int)TAB_ZPP0100.Qtd_Carregada].Double();
                this.St_Conf_ = l[(int)TAB_ZPP0100.St_Conf_].Valor;
                this.Ordem_Prod_ = l[(int)TAB_ZPP0100.Ordem_Prod_].Valor;
                this.Apontamento_Fert = l[(int)TAB_ZPP0100.Apontamento_Fert].Valor;
                this.Nota_Fiscal = l[(int)TAB_ZPP0100.Nota_Fiscal].Int();

                this.Data_NF = l[(int)TAB_ZPP0100.Data_NF].DataNull();
                this.Data = l[(int)TAB_ZPP0100.Data].DataNull();
            }



        }
    }
}
