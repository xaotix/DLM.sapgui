using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
    public class StatusSAP_Planejamento
    {
        public long id { get; set; }
        public string status { get; set; } = "";
        public string descricao { get; set; } = "";
        public StatusSAP_Planejamento()
        {

        }
        public StatusSAP_Planejamento(DLM.db.Linha L)
        {
            this.id = L["id"].Int();
            this.descricao = L.Get("descricao").ToString();
            this.status = L.Get("status").ToString();
        }
    }
}
