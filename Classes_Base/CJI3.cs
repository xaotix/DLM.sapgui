using DLM.painel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    /*REALIZADO MONTAGEM,LOGÍSTICA E SEGURO*/
    public class CJI3
    {
        public DLM.db.Linha GetLinha()
        {
            
            DLM.db.Linha l = new DLM.db.Linha();
            //l.Add("booleano", this.booleano);
            l.Add("Data_de_lancamento", this.Data_de_lancamento);
            l.Add("Tipo_de_objeto", this.Tipo_de_objeto);
            l.Add("Elemento_PEP", this.Elemento_PEP);
            l.Add("Objeto", this.Objeto);
            l.Add("Classe_de_custo", this.Classe_de_custo);
            l.Add("Valor_moeda_ACC", this.Valor_moeda_ACC);
            l.Add("Moeda_da_ACC", this.Moeda_da_ACC);
            l.Add("Valor_moed_transacao", this.Valor_moed_transacao);
            l.Add("Moeda_da_transacao", this.Moeda_da_transacao);
            l.Add("Divisao", this.Divisao);
            l.Add("Denominacao_de_objeto", this.Denominacao_de_objeto);
            l.Add("Denom_classe_custo", this.Denom_classe_custo);
            l.Add("Documento_de_compras", this.Documento_de_compras);

            return l;
        }
        public string Data_de_lancamento { get; set; } = "";
        public string Tipo_de_objeto { get; set; } = "";
        public string Elemento_PEP { get; set; } = "";
        public string Objeto { get; set; } = "";
        public string Classe_de_custo { get; set; } = "";
        public string Valor_moeda_ACC { get; set; } = "";
        public string Moeda_da_ACC { get; set; } = "";
        public string Valor_moed_transacao { get; set; } = "";
        public string Moeda_da_transacao { get; set; } = "";
        public string Divisao { get; set; } = "";
        public string Denominacao_de_objeto { get; set; } = "";
        public string Denom_classe_custo { get; set; } = "";
        public string Documento_de_compras { get; set; } = "";

        public CJI3()
        {

        }
        public CJI3(DLM.db.Linha l)
        {
            this.Data_de_lancamento = l[Colunas.CJI3.Data_de_lancamento].ToString();
            this.Tipo_de_objeto = l[Colunas.CJI3.Tipo_de_objeto].ToString();
            this.Elemento_PEP = CargaExcel.TratarPEP(l[Colunas.CJI3.Elemento_PEP].ToString());
            this.Objeto = l[Colunas.CJI3.Objeto].ToString();
            this.Classe_de_custo = l[Colunas.CJI3.Classe_de_custo].ToString();
            this.Valor_moeda_ACC = l[Colunas.CJI3.Valor_moeda_ACC].ToString();
            this.Moeda_da_ACC = l[Colunas.CJI3.Moeda_da_ACC].ToString();
            this.Valor_moed_transacao = l[Colunas.CJI3.Valor_moed_transacao].ToString();
            this.Moeda_da_transacao = l[Colunas.CJI3.Moeda_da_transacao].ToString();
            this.Divisao = l[Colunas.CJI3.Divisao].ToString();
            this.Denominacao_de_objeto = l[Colunas.CJI3.Denominacao_de_objeto].ToString();
            this.Denom_classe_custo = l[Colunas.CJI3.Denom_classe_custo].ToString();
            this.Documento_de_compras = l[Colunas.CJI3.Documento_de_compras].ToString();
        }
    }
}
