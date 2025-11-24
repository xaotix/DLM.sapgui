using DLM.painel;
using Conexoes;
using System;

namespace DLM.sapgui
{
   public class CN47N
    {
        private PEP_Planejamento _PEP { get; set; }

        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha retorno = new DLM.db.Linha();
            retorno.Add("pep",this.PEP);
            retorno.Add("status", this.Status);
            retorno.Add("texto_operacao", this.Texto_Operacao);
            if (this.Data_Inicio_Base.Valido())
            {
                retorno.Add("Data_Inicio_Base", this.Data_Inicio_Base);
            }
            if (this.Data_Fim_Base.Valido())
            {
                retorno.Add("Data_Fim_Base", this.Data_Fim_Base);
            }
            return retorno;
        }
        public override string ToString()
        {
            return PEP;
        }
        public string PEP { get; set; } = "";
        //public PEP_Planejamento PEP
        //{
        //    get
        //    {
        //        if(_PEP==null)
        //        {
        //            _PEP = new PEP_Planejamento();
        //        }
        //        return _PEP;
        //    }
        //    set
        //    {
        //        _PEP = value;
        //    }
        //}

        public string Status { get; set; } = "";
        public string Texto_Operacao { get; set; } = "";

        public DateTime? Data_Inicio_Base { get; set; }
        public DateTime? Data_Fim_Base { get;  set; }
        public DateTime? Inicio_Previsto { get;  set; }
        public DateTime? Fim_Previsto { get; set; }

        public CN47N()
        {
            Status = "NÃO TEM";
        }
        public CN47N(DLM.db.Linha l)
        {
            this.PEP = l[(int)Colunas.CN47N.WBS].Valor;
            this.Status = l[(int)Colunas.CN47N.STATUS].Valor;
            this.Texto_Operacao = l[(int)Colunas.CN47N.TEXTO_OPERACAO].Valor;
            this.Data_Inicio_Base = l[(int)Colunas.CN47N.INICIO_BASE].DataNull();
            this.Data_Fim_Base = l[(int)Colunas.CN47N.ULTIMA_DATA_FIM_BASE].DataNull();
            
            if(l.Count>=(int)Colunas.CN47N.INICIO_PREVISTO)
            {
                this.Inicio_Previsto = l[(int)Colunas.CN47N.INICIO_PREVISTO].DataNull();
                this.Fim_Previsto = l[(int)Colunas.CN47N.FIM_PREVISTO].DataNull();
            }

        }
    }
}
