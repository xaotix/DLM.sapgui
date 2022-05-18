using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
   public class Descricao_PEP
    {
        public string PEP { get; set; } = "";
        public string FAB { get; set; } = "";
        public string DESC { get; set; } = "";
        public Descricao_PEP()
        {

        }
        public Descricao_PEP(DLM.db.Linha l)
        {
            this.PEP =  l["PEP"].Valor;
            this.FAB =  l["FAB"].Valor;
            this.DESC = l["DESC"].Valor;
        }
    }
}
