using DLM.painel;
using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{

    public class ZPMP : INotifyPropertyChanged
    {
        public override string ToString()
        {
            return material.ToString();
        }




        public DLM.db.Linha GetLinha()
        {
            DateTime dtmin = Conexoes.Utilz.Calendario.DataDummy();
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", PEP.Codigo);
            //l.Add("denominacao", denominacao);
            l.Add("centro", centro);
            if(centro_producao!="")
            {
            l.Add("centro_producao", centro_producao);
            }
            l.Add("material", material);
            l.Add("texto_breve", texto_breve);
            l.Add("grupo_mercadoria", grupo_mercadoria);
            l.Add("peso_necessario", peso_necessario);
            l.Add("peso_produzido", peso_produzido);
            l.Add("qtd_necessaria", qtd_necessaria);
            l.Add("qtd_mercadoria_entrada", qtd_mecadoria_entrada);

            if (fim_engenharia_base > dtmin) { l.Add("fim_engenharia_base", fim_engenharia_base); };
            if (fim_engenharia_real > dtmin) { l.Add("fim_engenharia_real", fim_engenharia_real); };
            if (fim_logistica_base > dtmin) { l.Add("fim_logistica_base", fim_logistica_base); };
            if (fim_logistica_real > dtmin) { l.Add("fim_logistica_real", fim_logistica_real); };
            if (fim_montagem_base > dtmin) { l.Add("fim_montagem_base", fim_montagem_base); };
            if (fim_montagem_real > dtmin) { l.Add("fim_montagem_real", fim_montagem_real); };
            if (inicio_montagem_base > dtmin) { l.Add("inicio_montagem_base", inicio_montagem_base); };

            if(saldo_peso_produzido!="")
            {
                l.Add("saldo_peso_produzido", saldo_peso_produzido);
            }
            l.Add("status_sistema_pep", Status_Sistema_PEP);
            if(status_usuario_pep!="")
            {
            l.Add("status_usuario_pep", status_usuario_pep);
            }
            l.Add("status_sistema_tarefa", status_sistema_tarefa);
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
        private PEPConsultaSAP _PEP { get; set; }
        public string denominacao { get; private set; } = "";
        public string centro { get; private set; } = "";
        public string centro_producao { get; private set; } = "";
        public string material { get; private set; } = "";
        public string texto_breve { get; private set; } = "";
        public string grupo_mercadoria { get; private set; } = "";
        public string peso_necessario { get; private set; } = "";
        public string peso_produzido { get; private set; } ="";
        public string qtd_necessaria { get; private set; } = "";
        public string qtd_mecadoria_entrada { get; private set; } = "";
        public DateTime fim_engenharia_base { get; private set; } = new DateTime();
        public DateTime fim_engenharia_real { get; private set; } = new DateTime();
        public DateTime Fim_Fabrica_Base { get; private set; } = new DateTime();
        public DateTime Fim_Fabrica_Real { get; private set; } = new DateTime();
        public DateTime fim_logistica_base { get; private set; } = new DateTime();
        public DateTime fim_logistica_real { get; private set; } = new DateTime();
        public DateTime fim_montagem_base { get; private set; } = new DateTime();
        public DateTime fim_montagem_real { get; private set; } = new DateTime();
        public DateTime inicio_montagem_base { get; private set; } = new DateTime();
        public string saldo_peso_produzido { get; private set; } = "";
        public string Status_Sistema_PEP { get; private set; } = "";
        public string status_usuario_pep { get; private set; } = "";
        public string status_sistema_tarefa { get; private set; } = "";
        public ZPMP()
        {

        }
        public ZPMP(DLM.db.Linha l)
        {
            this.centro = l[Colunas.ZPMP.Centro].ToString();
            this.centro_producao = l[Colunas.ZPMP.Centro_producao].ToString();
            this.denominacao = l[Colunas.ZPMP.Denominacao].ToString();
            this.fim_engenharia_base = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Engenharia_Base].ToString());
            this.fim_engenharia_real = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Engenharia_Real].ToString());
            this.Fim_Fabrica_Base = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Fabrica_Base].ToString());
            this.Fim_Fabrica_Real = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Fabrica_Real].ToString());
            this.fim_logistica_base = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Logistica_Base].ToString());
            this.fim_logistica_real = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Logistica_Real].ToString());
            this.fim_montagem_base = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Montagem_Base].ToString());
            this.fim_montagem_real = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Fim_Montagem_Real].ToString());
            this.grupo_mercadoria = l[Colunas.ZPMP.Denom_grupo_merc].ToString();
            this.inicio_montagem_base = Conexoes.Extensoes.Data(l[Colunas.ZPMP.Inicio_Montagem_Base].ToString());
            this.material = l[Colunas.ZPMP.Material].ToString();
            this.PEP = new PEPConsultaSAP(l[Colunas.ZPMP.Elemento_PEP].ToString());
            this.peso_necessario = l[Colunas.ZPMP.Peso_necessario].ToString();
            this.peso_produzido = l[Colunas.ZPMP.Peso_produzido].ToString();
            this.qtd_mecadoria_entrada = l[Colunas.ZPMP.Qtd_mercad_entrada].ToString();
            this.qtd_necessaria = l[Colunas.ZPMP.Qtd_necessaria].ToString();
            this.saldo_peso_produzido = l[Colunas.ZPMP.Saldo_peso_produzido].ToString();
            this.Status_Sistema_PEP = l[Colunas.ZPMP.Status_Sistema_PEP].ToString();
            this.status_sistema_tarefa = l[Colunas.ZPMP.Status_Sistema_Tarefa].ToString();
            this.status_usuario_pep = l[Colunas.ZPMP.Status_Usuario_PEP].ToString();
            this.texto_breve = l[Colunas.ZPMP.Texto_breve_material].ToString().CortarString(140);


            
        }
    }
}
