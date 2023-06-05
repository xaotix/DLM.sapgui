using DLM.painel;
using Conexoes;
using System.Linq;

namespace DLM.sapgui
{
    public class ZPP0066N : Notificar
    {
        public override string ToString()
        {
            return Material.ToString();
        }
        public string GetRow()
        {
            return ("(" + string.Join("", GetLinha().Celulas.Select(x => "'" + x.Valor + "',")) + ")").Replace(",)", ")");
        }


        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", PEP.Codigo);
            l.Add("material", Material);
            l.Add("carga_confirmada", Carga_Confirmada);
            l.Add("status_material", Status_Material);
            l.Add("desenho", Desenho);
            l.Add("num_carga", Num_Carga);
            l.Add("pack_list", Pack_List);
            l.Add("quantidade", Quantidade);
            l.Add("nota_fiscal", Nota_Fiscal);
            return l;
        }


        public PEP_Planejamento PEP
        {
            get
            {
                if (_PEP == null)
                {
                    _PEP = new PEP_Planejamento();
                }
                return _PEP;
            }
            set
            {

                _PEP = value;
                NotifyPropertyChanged("PEP");
            }
        }
        private PEP_Planejamento _PEP { get; set; }
        public string Material { get; private set; } = "";
        public bool Carga_Confirmada { get; private set; } = false;
        public string Status_Material { get; private set; } = "";
        public string Desenho { get; private set; } = "";
        public string Num_Carga { get; private set; } = "";
        public string Pack_List { get; private set; } = "";
        public string Nota_Fiscal { get; private set; } = "";
        public string Quantidade { get; private set; } = "";
        public ZPP0066N(DLM.db.Linha l, bool semperfil)
        {
            if (!semperfil)
            {
                this.Carga_Confirmada = l[Colunas.ZPP0066N.CARGA].ToString().Replace(" ", "").ToUpper() == "X";
                this.Material = l[Colunas.ZPP0066N.MATERIAL].ToString();
                this.Pack_List = l[Colunas.ZPP0066N.PACKLIST].ToString().Replace(" ", "");
                this.Status_Material = l[Colunas.ZPP0066N.STATUSMAT].ToString().Replace(" ", "");
                this.Desenho = l[Colunas.ZPP0066N.DESENHO].ToString().Replace(" ", "");
                this.Num_Carga = l[Colunas.ZPP0066N.NUMCARGA].ToString().Replace(" ", "");
                this.Quantidade =l[Colunas.ZPP0066N.QUANTIDADE].ToString();
                this.PEP = new PEP_Planejamento(l[Colunas.ZPP0066N.PEP].ToString().Replace(" ", ""));
            }
            else
            {
                this.Carga_Confirmada = l[Colunas.ZPP0066N_Sem_Perfil.ConfirmCarga].ToString().Replace(" ", "").ToUpper() == "X";
                this.Material = l[Colunas.ZPP0066N_Sem_Perfil.Material].ToString();
                this.Pack_List = l[Colunas.ZPP0066N_Sem_Perfil.Packing_List].ToString().Replace(" ", "");
                this.Status_Material = l[Colunas.ZPP0066N_Sem_Perfil.Status_Mat].ToString().Replace(" ", "");
                this.Desenho = l[Colunas.ZPP0066N_Sem_Perfil.NumDesEngenharia].ToString().Replace(" ", "");
                this.Num_Carga = l[Colunas.ZPP0066N_Sem_Perfil.NumCarga].ToString().Replace(" ", "");
                this.Quantidade = l[Colunas.ZPP0066N_Sem_Perfil.QtdePacking].ToString();
                this.PEP = new PEP_Planejamento(l[Colunas.ZPP0066N_Sem_Perfil.Elemento_PEP].ToString().Replace(" ", ""));
                this.Nota_Fiscal = l[Colunas.ZPP0066N_Sem_Perfil.NFiscal].ToString().Replace(" ", "");
            }

        }
    }

}
