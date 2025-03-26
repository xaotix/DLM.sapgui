namespace DLM.painel
{
    public class PLAN_PECA_ZPMP
    {
        public string PEP { get; private set; } = "";
        public string Marca { get; private set; } = "";
        public string Material { get; private set; } = "";
        public string Grupo_Mercadoria { get; private set; } = "";
        public PLAN_PECA_ZPMP(db.Linha linha)
        {
            this.PEP = linha["pep"].Valor;
            this.Marca = linha["tamanho_dimensao"].Valor;
            this.Material = linha["material"].Valor;
            this.Grupo_Mercadoria = linha["grupo_mercadoria"].Valor;

        }
    }
}
