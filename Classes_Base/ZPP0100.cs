using DLM.painel;
using Conexoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    public class ZPP0100
    {
        //public string booleano { get; set; } = "";
        public PEPConsultaSAP PEP { get; set; } = new PEPConsultaSAP();
        public string Ordem_Embarque { get; set; } = "";
        public string Qtd_Embarque { get; set; } = "";
        public string Nro_Carga { get; set; } = "";
        public bool Carregado()
        {
            return this.St_Conf_.ToUpper() == "@5Y@";
        }
        public string St_Embarque { get; set; } = "";
        public string St_Carga { get; set; } = "";
        public string Etq_Material { get; set; } = "";
        public string Material { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string Tamanho_dimensao { get; set; } = "";
        public string Comprimento { get; set; } = "";
        public string Peso_item_Tot { get; set; } = "";
        public string Nome_da_Obra { get; set; } = "";
        public string Elemento_PEP { get; set; } = "";
        public string Centro { get; set; } = "";
        public string Etq_Impressa { get; set; } = "";
        public string Etq_Volume { get; set; } = "";
        public string Status { get; set; } = "";
        public string Qtd_Carregada { get; set; } = "";
        public string Sld_1202 { get; set; } = "";
        public string Sld_1203 { get; set; } = "";
        public string Sld_1204 { get; set; } = "";
        public string St_Conf_ { get; set; } = "";
        public string St_DtProg_ { get; set; } = "";
        public DateTime Data { get; set; } = new DateTime();
        public string Ordem_Prod_ { get; set; } = "";
        public string Apontamento_Fert { get; set; } = "";
        public DLM.db.Linha GetLinha()
        {

            DLM.db.Linha l = new DLM.db.Linha();
            //l.Add("booleano", this.booleano);
            l.Add("Elemento_PEP", this.Elemento_PEP);
            l.Add("Ordem_Embarque", this.Ordem_Embarque);
            if(this.Qtd_Embarque!="")
            {
            l.Add("Qtd_Embarque", this.Qtd_Embarque);
            }
            if(this.Qtd_Carregada!="")
            {
            l.Add("Qtd_Carregada", this.Qtd_Carregada);
            }
            l.Add("Material", this.Material);
            if(this.Peso_item_Tot!="")
            {
            l.Add("Peso_item_Tot", this.Peso_item_Tot);
            }

            l.Add("Nro_Carga", this.Nro_Carga);
            if (St_Embarque != "")
            {
            l.Add("St_Embarque", this.St_Embarque);
            }
            if(St_Carga!="")
            {
            l.Add("St_Carga", this.St_Carga);
            }
            if(Etq_Material!="")
            {
            l.Add("Etq_Material", this.Etq_Material);
            }
            l.Add("Descricao", this.Descricao);
            if(Tamanho_dimensao!="")
            {
            l.Add("Tamanho_dimensao", this.Tamanho_dimensao);
            }
            if(Comprimento!="")
            {
            l.Add("Comprimento", this.Comprimento);
            }
            //l.Add("Nome_da_Obra", this.Nome_da_Obra);
            l.Add("Centro", this.Centro);
            if(Etq_Impressa!="")
            {
            l.Add("Etq_Impressa", this.Etq_Impressa);
            }
            if(Etq_Volume!="")
            {
            l.Add("Etq_Volume", this.Etq_Volume);
            }

            l.Add("Status", this.Status);
            if(Sld_1202!="")
            {
            l.Add("Sld_1202", this.Sld_1202);
            }
            if(Sld_1203!="")
            {
            l.Add("Sld_1203", this.Sld_1203);
            }
            if(Sld_1204 != "")
            {
            l.Add("Sld_1204", this.Sld_1204);
            }
            if(this.St_Conf_!="")
            {
                l.Add("St_Conf_", this.St_Conf_);
            }
            
            l.Add("St_DtProg_", this.St_DtProg_);
            if(this.Data>new DateTime(2001,01,01))
            {
            l.Add("Data", this.Data);
            }

            if(Ordem_Prod_!="")
            {
            l.Add("Ordem_Prod_", this.Ordem_Prod_);
            }
            l.Add("Apontamento_Fert", this.Apontamento_Fert);
            return l;
        }
        public ZPP0100()
        {

        }
        public ZPP0100(DLM.db.Linha l)
        {
            //this.booleano = l[Colunas.ZPP0100.booleano].ToString();
            this.Elemento_PEP = CargaExcel.TratarPEP(l[(int)TAB_ZPP0100.Elemento_PEP].ToString());
            this.PEP = new PEPConsultaSAP(this.Elemento_PEP.ToString().Replace(" ", ""));
            this.Ordem_Embarque = l[(int)TAB_ZPP0100.Ordem_Embarque].ToString();
            this.Qtd_Embarque = l[(int)TAB_ZPP0100.Qtd_Embarque].ToString();
            this.Nro_Carga = l[(int)TAB_ZPP0100.Nro_Carga].ToString();
            this.St_Embarque = l[(int)TAB_ZPP0100.St_Embarque].ToString();
            this.St_Carga = l[(int)TAB_ZPP0100.St_Carga].ToString();
            this.Etq_Material = l[(int)TAB_ZPP0100.Etq_Material].ToString();
            this.Material = l[(int)TAB_ZPP0100.Material].ToString();
            this.Descricao = l[(int)TAB_ZPP0100.Descricao].ToString().CortarString(140);
            this.Tamanho_dimensao = l[(int)TAB_ZPP0100.Tamanho_dimensao].ToString();
            this.Comprimento = l[(int)TAB_ZPP0100.Comprimento].ToString();
            this.Peso_item_Tot = l[(int)TAB_ZPP0100.Peso_item_Tot].ToString();
            this.Nome_da_Obra = l[(int)TAB_ZPP0100.Nome_da_Obra].ToString();
            this.Centro = l[(int)TAB_ZPP0100.Centro].ToString();
            this.Etq_Impressa = l[(int)TAB_ZPP0100.Etq_Impressa].ToString();
            this.Etq_Volume = l[(int)TAB_ZPP0100.Etq_Volume].ToString();
            this.Status = l[(int)TAB_ZPP0100.Status].ToString();
            this.Qtd_Carregada = l[(int)TAB_ZPP0100.Qtd_Carregada].ToString();
            this.Sld_1202 = l[(int)TAB_ZPP0100.Sld_1202].ToString();
            this.Sld_1203 = l[(int)TAB_ZPP0100.Sld_1203].ToString();
            this.Sld_1204 = l[(int)TAB_ZPP0100.Sld_1204].ToString();
            this.St_Conf_ = l[(int)TAB_ZPP0100.St_Conf_].ToString();
            this.St_DtProg_ = l[(int)TAB_ZPP0100.St_DtProg_].ToString();
            this.Data = Conexoes.Extensoes.Data(l[(int)TAB_ZPP0100.Data].ToString());
            this.Ordem_Prod_ = l[(int)TAB_ZPP0100.Ordem_Prod_].ToString();
            this.Apontamento_Fert = l[(int)TAB_ZPP0100.Apontamento_Fert].ToString();
        }
    }
}
