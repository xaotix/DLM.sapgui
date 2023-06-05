namespace DLM.sapgui
{
    public class ZPP0112
    {
        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha retorno = new DLM.db.Linha();
            retorno.Add("Centro", this.Centro);
            // 20/10/2020
            //TRATAMENTO QUE ALTERA QUALQUER LETRA DA UNIDADE FABRIL PRA 'F' 
            // PRA RESOLVER O PROBLEMA DE MERDA OS CADASTRO DE PEP NA 
            //LOGISTICA QUE FICAM FAZENDO TIGRADA DE BOTAR NOMES NÃO PADRÃO
            var PEP = CargaExcel.TratarPEP(this.Elemento_PEP);


            retorno.Add("Elemento_PEP", PEP
                .Replace(".Z2",".F3")

                );
            retorno.Add("Fornecedor", this.Fornecedor);
            retorno.Add("Fornecedor_2", this.Fornecedor_2);
            retorno.Add("Motorista", this.Motorista);
            retorno.Add("Nro_Carga", this.Nro_Carga);
            retorno.Add("Num_Placa", this.Num_Placa);
            retorno.Add("Observacoes", this.Observacoes);
            retorno.Add("RG", this.RG);
            retorno.Add("sel", this.sel);
            retorno.Add("Telefone", this.Telefone);
            retorno.Add("Telefone_2", this.Telefone_2);
            retorno.Add("Telefone_3", this.Telefone_3);
            retorno.Add("Tipo_Veiculo", this.Tipo_Veiculo);
            retorno.Add("VAL2", this.VAL2);

            return retorno;
        }
        public string sel { get; set; } = "";
        public string Nro_Carga { get; set; } = "";
        public string Elemento_PEP { get; set; } = "";
        public string Centro { get; set; } = "";
        public string Fornecedor { get; set; } = "";
        public string Tipo_Veiculo { get; set; } = "";
        public string Num_Placa { get; set; } = "";
        public string Motorista { get; set; } = "";
        public string RG { get; set; } = "";
        public string Telefone { get; set; } = "";
        public string VAL2 { get; set; } = "";
        public string Telefone_2 { get; set; } = "";
        public string Fornecedor_2 { get; set; } = "";
        public string Telefone_3 { get; set; } = "";
        public string Observacoes { get; set; } = "";
        public ZPP0112()
        {

        }
        public ZPP0112(DLM.db.Linha l)
        {
            this.Centro = l[(int)TAB_ZPP0112.Centro].ToString();
            this.Elemento_PEP = CargaExcel.TratarPEP(l[(int)TAB_ZPP0112.Elemento_PEP].ToString());
            this.Fornecedor = l[(int)TAB_ZPP0112.Fornecedor].ToString();
            this.Fornecedor_2 = l[(int)TAB_ZPP0112.Fornecedor_2].ToString();
            this.Motorista = l[(int)TAB_ZPP0112.Motorista].ToString();
            this.Nro_Carga = l[(int)TAB_ZPP0112.Nro_Carga].ToString();
            this.Num_Placa = l[(int)TAB_ZPP0112.Num_Placa].ToString();
            this.Observacoes = l[(int)TAB_ZPP0112.Observacoes].ToString();
            this.RG = l[(int)TAB_ZPP0112.RG].ToString();
            this.sel = l[(int)TAB_ZPP0112.sel].ToString();
            this.Telefone = l[(int)TAB_ZPP0112.Telefone].ToString();
            this.Telefone_2 = l[(int)TAB_ZPP0112.Telefone_2].ToString();
            this.Telefone_3 = l[(int)TAB_ZPP0112.Telefone_3].ToString();
            this.Tipo_Veiculo = l[(int)TAB_ZPP0112.Tipo_Veiculo].ToString();
            this.VAL2 = l[(int)TAB_ZPP0112.VAL2].ToString();

        }
    }
}
