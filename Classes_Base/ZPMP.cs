using Conexoes;
using System;
using DLM.vars;

namespace DLM.sapgui
{

    public class ZPMP : Notificar
    {
        public override string ToString()
        {
            return $"[{PEP}] - {Material} - [{Marca}] [{Descricao}]";
        }

        public DLM.db.Linha GetLinha()
        {
            var l = new DLM.db.Linha();
            l.Add("posnr", POSNR);
            l.Add("pep", PEP);
            l.Add("centro", Centro);
            l.Add("centro_producao", CentroProducao);
            l.Add("material", Material);
            l.Add("tamanho_dimensao", Marca);
            l.Add("grupo_mercadoria", Grp_Mercadoria);
            l.Add("texto_breve", Descricao);
            l.Add("peso_necessario", Peso_Nec);
            l.Add("peso_produzido", Peso_Fab);
            l.Add("qtd_necessaria", Qtd_Nec);
            l.Add("qtd_produzida", Qtd_Fab);
            l.Add("peso_unitario", Peso_Un);



            return l;
        }

        public long POSNR { get; set; }
        public string PEP { get; set; } = "";


        public string Centro { get; private set; } = "";
        public string CentroProducao { get; private set; } = "";
        public string Marca { get; private set; } = "";
        public string Material { get; private set; } = "";
        public string Descricao { get; set; } = "";
        public string Grp_Mercadoria { get; private set; } = "";

        public double Peso_Un
        {
           get
            {
                if(Qtd_Nec>0 && Peso_Nec>0)
                {
                    return Peso_Nec / Qtd_Nec;
                }
                return 0;
            }
        }
        public double Peso_Nec { get; private set; } = 0;
        public double Peso_Fab { get; private set; } =0;

        public double Qtd_Nec { get; private set; } = 0;
        public double Qtd_Fab
        {
            get
            {
                if(Peso_Fab>0 && Peso_Un>0)
                {
                    return Peso_Fab / Peso_Un;
                }
                return 0;
            }
        }



        public ZPMP(DLM.db.Linha l, bool avanco = false)
        {
            if(avanco)
            {
                this.POSNR = l["ELPEP"].Long();
                this.PEP = Conexoes.Utilz.PEP.Ajustar(l["POSID"].Valor);
                this.Material = l["MATNR"].Valor;
                this.Marca = l["GROES"].Valor;

                this.Centro = l["WERKS"].Valor;
                this.CentroProducao = l["WERKR"].Valor;
                this.Qtd_Nec = l["BDMNG"].Double();
                this.Peso_Nec = l["PESO_NEC"].Double();
                this.Peso_Fab = l["PESO_FAB"].Double();

                //this.texto_breve = l["MAKTX"].Valor;
                //this.grupo_mercadoria = l[""].Valor;
            }
            else
            {
                this.PEP = l[(int)TAB_ZPMP.ELEMENTO_PEP].Valor;
                this.Centro = l[(int)TAB_ZPMP.CENTRO].Valor;
                this.CentroProducao = l[(int)TAB_ZPMP.CENTRO_PRODUCAO].Valor;
                this.Material = l[(int)TAB_ZPMP.MATERIAL].Valor;
                this.Marca = l[(int)TAB_ZPMP.TAMANHO_DIMENSAO].Valor;
                this.Descricao = l[(int)TAB_ZPMP.TEXTO_BREVE].Valor;
                this.Qtd_Nec = l[(int)TAB_ZPMP.QTD_NECESS].Double();
                this.Peso_Nec = l[(int)TAB_ZPMP.PESO_NECESS].Double();
                this.Peso_Fab = l[(int)TAB_ZPMP.PESO_PRODUZIDO].Double();
                this.Grp_Mercadoria = l[(int)TAB_ZPMP.DENOM_GRUPO_MERC].Valor;
            }

        }
    }
}
