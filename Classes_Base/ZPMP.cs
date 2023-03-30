using DLM.painel;
using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DLM.vars;

namespace DLM.sapgui
{

    public class ZPMP : INotifyPropertyChanged
    {
        public override string ToString()
        {
            return material.ToString();
        }


        private PEPConsultaSAP _PEP { get; set; }


        public DLM.db.Linha GetLinha()
        {
            DateTime dtmin = Cfg.Init.DataDummy();
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", PEP.Codigo);
            l.Add(nameof(centro), centro);
            l.Add(nameof(centro_producao), centro_producao);
            l.Add(nameof(material), material);
            l.Add(nameof(tamanho_dimensao), tamanho_dimensao);
            l.Add(nameof(grupo_mercadoria), grupo_mercadoria);
            l.Add(nameof(texto_breve), texto_breve);

            l.Add(nameof(peso_necessario), peso_necessario);
            l.Add(nameof(peso_produzido), peso_produzido);

            l.Add(nameof(qtd_necessaria), qtd_necessaria);
            l.Add(nameof(qtd_produzida), qtd_produzida);



            return l;
        }
        #region Properties
        [Browsable(false)]
        public event PropertyChangedEventHandler PropertyChanged;
        [Browsable(false)]
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        [Browsable(false)]
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public PEPConsultaSAP PEP
        {
            get
            {
                if (_PEP == null)
                {
                    _PEP = new PEPConsultaSAP();
                }
                return _PEP;
            }
            set
            {

                _PEP = value;
                NotifyPropertyChanged("PEP");
            }
        }

        public string denominacao { get; private set; } = "";
        public string centro { get; private set; } = "";
        public string centro_producao { get; private set; } = "";
        public string tamanho_dimensao { get; private set; } = "";
        public string material { get; private set; } = "";
        public string texto_breve { get; private set; } = "";
        public string grupo_mercadoria { get; private set; } = "";
        public double peso_unitario { get; private set; } = 0;
        public double peso_necessario { get; private set; } = 0;
        public double peso_produzido { get; private set; } =0;
        public double qtd_necessaria { get; private set; } = 0;
        public double qtd_produzida
        {
            get
            {
                if(peso_produzido>0 && peso_unitario>0)
                {
                    return peso_produzido / peso_unitario;
                }
                return 0;
            }
        }
        public double saldo_peso_produzido { get; private set; } = 0;

        public string status_usuario_pep { get; private set; } = "";




        public ZPMP(DLM.db.Linha l)
        {
            this.PEP = new PEPConsultaSAP(l[(int)TAB_ZPMP.ELEMENTO_PEP].Valor);
            this.centro = l[(int)TAB_ZPMP.CENTRO].Valor;
            this.material = l[(int)TAB_ZPMP.MATERIAL].Valor;
            this.tamanho_dimensao = l[(int)TAB_ZPMP.TAMANHO_DIMENSAO].Valor;
            this.texto_breve = l[(int)TAB_ZPMP.TEXTO_BREVE].Valor;
            this.qtd_necessaria = l[(int)TAB_ZPMP.QTD_NECESS].Double();
            this.peso_necessario = l[(int)TAB_ZPMP.PESO_NECESS].Double();
            this.peso_produzido = l[(int)TAB_ZPMP.PESO_PRODUZIDO].Double();
            this.peso_unitario = l[(int)TAB_ZPMP.UNIDADE].Double();
            this.denominacao = l[(int)TAB_ZPMP.DENOMINACAO].Valor;
            this.grupo_mercadoria = l[(int)TAB_ZPMP.DENOM_GRUPO_MERC].Valor;
        }
    }
}
