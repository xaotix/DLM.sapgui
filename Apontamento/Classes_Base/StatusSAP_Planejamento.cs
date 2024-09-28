
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
        public StatusSAP_Planejamento(DLM.db.Linha linha)
        {
            this.id = linha["id"].Int();
            this.descricao = linha["descricao"].Valor;
            this.status = linha["status"].Valor;
        }
    }
}
