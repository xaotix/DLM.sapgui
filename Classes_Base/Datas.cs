using DLM.painel;
using Conexoes;
using System;
using DLM.vars;
using SAP.Middleware.Connector;

namespace DLM.sapgui
{
   public class CN47N
    {
        private PEP_Planejamento _PEP { get; set; }

        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha retorno = new DLM.db.Linha();
            retorno.Add("pep",this.PEP.Codigo);
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
            return PEP.Codigo;
        }
        public PEP_Planejamento PEP
        {
            get
            {
                if(_PEP==null)
                {
                    _PEP = new PEP_Planejamento();
                }
                return _PEP;
            }
            set
            {
                _PEP = value;
            }
        }

        public string Status { get; private set; } = "";
        public string Texto_Operacao { get; private set; } = "";

        public DateTime Data_Inicio_Base { get; private set; } = Cfg.Init.DataDummy();
        public DateTime Data_Fim_Base { get; private set; } = Cfg.Init.DataDummy();
        public DateTime Inicio_Previsto { get; private set; } = Cfg.Init.DataDummy();
        public DateTime Fim_Previsto { get; private set; } = Cfg.Init.DataDummy();

        public CN47N()
        {
            Status = "NÃO TEM";
        }
        public CN47N(IRfcStructure s)
        {
            this.PEP = new PEP_Planejamento(s.GetValue("ELEM_PEP").ToString());
            this.Texto_Operacao = s.GetValue("POST1").ToString();
            this.Data_Inicio_Base = s.GetValue("PSTRT").Data();
            this.Data_Fim_Base = s.GetValue("PENDE").Data();



            this.Inicio_Previsto =s.GetValue("ESTRT").Data();
            this.Fim_Previsto = s.GetValue("EENDE").Data();

            /*
             * FIELD PROJETO=20-103356.P00 
             * FIELD ELEM_PEP=20-103356.P00.001.30A.F3 
             * FIELD POST1=Fáb. Estrutura em SER 
             * FIELD PBUKR=1200 
             * FIELD PGSBR=1203 
             * FIELD STUFE=4 
             * FIELD PSTRT=2018-09-25 
             * FIELD PENDE=2018-10-23 
             * FIELD ESTRT=2018-09-25 
             * FIELD EENDE=2018-10-23 
             * FIELD VISTR=0000-00-00 
             * FIELD VIEND=0000-00-00 
             */
        }
        public CN47N(DLM.db.Linha l)
        {
            this.PEP = new PEP_Planejamento(l[(int)Colunas.CN47N.WBS].ToString());
            this.Status = l[(int)Colunas.CN47N.STATUS].ToString();
            this.Texto_Operacao = l[(int)Colunas.CN47N.TEXTO_OPERACAO].ToString();
            this.Data_Inicio_Base = l[(int)Colunas.CN47N.INICIO_BASE].Data();
            this.Data_Fim_Base = l[(int)Colunas.CN47N.ULTIMA_DATA_FIM_BASE].Data();
            
            if(l.Count>=(int)Colunas.CN47N.INICIO_PREVISTO)
            {
                this.Inicio_Previsto = l[(int)Colunas.CN47N.INICIO_PREVISTO].Data();
                this.Fim_Previsto = l[(int)Colunas.CN47N.FIM_PREVISTO].Data();
            }

        }
    }
}
