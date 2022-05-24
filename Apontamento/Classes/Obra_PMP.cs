using DLM.sapgui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DLM.painel
{
    public class Base_PMP: INotifyPropertyChanged
    {
        public override string ToString()
        {
            return this.pep + " - [" + this.tipo.ToString() + "]";
        }
        public void _forcartipo(Tipo_Material tipo)
        {
            switch (tipo)
            {
                case Tipo_Material.Real:
                    if (this.Material_REAL) { this.tipo = tipo; }
                    break;
                case Tipo_Material.Orçamento:
                    if (this.Material_ORC) { this.tipo = tipo; }
                    break;
                case Tipo_Material.Consolidado:
                    if (this.Material_CONS) { this.tipo = tipo; }
                    break;
                case Tipo_Material._:
                    break;
                default:
                    break;
            }
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
        public void _mudartipo()
        {
            if(tipo == Tipo_Material.Real)
            {
                if(Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                }
                else if(Material_ORC)
                {
                    tipo = Tipo_Material.Orçamento;
                }
            }
            else if(tipo == Tipo_Material.Orçamento)
            {
                if (Material_REAL)
                {
                    tipo = Tipo_Material.Real;
                }
               else if (Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                }
            }
            else if(tipo == Tipo_Material.Consolidado)
            {
                if (Material_ORC)
                {
                    tipo = Tipo_Material.Orçamento;
                }
                else if (Material_REAL)
                {
                    tipo = Tipo_Material.Real;
                }
            }




        }
        private Tipo_Material _tipo { get; set; } = Tipo_Material._;
        public Tipo_Material tipo
        {
            get
            {
                if (_tipo == Tipo_Material._)
                {
                    if (Material_REAL)
                    {
                        _tipo = Tipo_Material.Real;
                    }
                    else if (Material_ORC)
                    {
                        _tipo = Tipo_Material.Orçamento;
                    }
                }
                return _tipo;
            }
            set
            {
                _tipo = value;
                NotifyPropertyChanged("imagem");
            }
        }
        public string setor_atividade
        {
            get
            {
                if (this.pep.Length >= 2)
                {
                    return this.pep.Substring(0, 2);
                }

                return "";
            }
        }
        public string etapa
        {
            get
            {
                if (this.pep.Length >= 17)
                {
                    return this.pep.Substring(0, 17);
                }
                return "";
            }
        }
        public string subetapa
        {
            get
            {
                if (this.pep.Length >= 21)
                {
                    return this.pep.Substring(0, 21);
                }
                return "";
            }
        }
        public string pedido
        {
            get
            {
                if (pep.Length > 13)
                {
                    return pep.Substring(0, 13);
                }
                return "";
            }
        }
        public string contrato
        {
            get
            {
                if (pep.Length > 9)
                {
                    return pep.Substring(3, 6);
                }
                return "";
            }
        }
        public string descricao { get; set; } = "";
        public bool Material_ORC { get; set; } = false;
        public bool Material_REAL { get; set; } = false;
        public bool Material_CONS { get; set; } = false;
        public string pep { get; set; } = "";
        public ImageSource imagem_ORC
        {
            get
            {
                if(Material_ORC && Material_REAL)
                {
                    return Conexoes.BufferImagem.O_AZUL;

                }
                else if (Material_ORC && Material_CONS)
                {
                    return Conexoes.BufferImagem.O_LARANJA;

                }
                else if (Material_ORC)
                {
                    return Conexoes.BufferImagem.O_VERDE;
                }
              return  Conexoes.BufferImagem.O_PRETO;
            }
        }
        public ImageSource imagem_CONS
        {
            get
            {
                if (Material_CONS && Material_REAL)
                {
                    return Conexoes.BufferImagem.C_AZUL;

                }
                else if (Material_CONS && Material_ORC)
                {
                    return Conexoes.BufferImagem.C_LARANJA;

                }
                else if (Material_CONS)
                {
                    return Conexoes.BufferImagem.C_VERDE;
                }
                return Conexoes.BufferImagem.C_PRETO;
            }
        }
        public ImageSource imagem_REAL
        {
            get
            {
                if (Material_REAL && Material_CONS)
                {
                    return Conexoes.BufferImagem.R_AZUL;
                }
                else if (Material_REAL && Material_ORC)
                {
                    return Conexoes.BufferImagem.R_LARANJA;

                }
                else if (Material_REAL)
                {
                    return Conexoes.BufferImagem.R_VERDE;
                }
                return Conexoes.BufferImagem.R_PRETO;
            }
        }
        public Base_PMP()
        {

        }
    }
    public class PEP_PMP : Base_PMP
    {
        public DateTime ei
        {
            get
            {
                if(tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma_inicio;
                }
                else if(tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ef
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime fi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ff
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime li
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime lf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }

        private List<Grupo_Mercadoria> _Grupos_Mercadoria { get; set; }
        public List<Grupo_Mercadoria> GetGrupos_Mercadoria()
        {


            if (_Grupos_Mercadoria == null)
            {
                _Grupos_Mercadoria = Classificadores.GetGrupo_Mercadorias(this.Getpecas());
            }
            return _Grupos_Mercadoria;
        }
        public void mudartipo()
        {
            _mudartipo();
        }
        public ImageSource imagem
        {
            get
            {
                if (tipo == Tipo_Material.Real)
                {
                    return imagem_REAL;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {

                    return imagem_CONS;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {

                    return imagem_ORC;
                }
                return Conexoes.BufferImagem.dialog_error;
            }
        }

        public List<PLAN_PECA> Getpecas()
        {
          return Obra.Getpecas().FindAll(x => x.Tipo == tipo).FindAll(x => x.pep.StartsWith(this.pep));
        }
        public double total_embarcado
        {
            get
            {
                return this.Real.total_embarcado;
            }
        }
        public double total_fabricado
        {
            get
            {
                return this.Real.total_fabricado;
            }
        }
        public double peso
        {
            get
            {
                if(Real.peso_planejado>0 && (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real))
                {
                    return Real.peso_planejado;
                }
                if (tipo == Tipo_Material.Real)
                {
                    return Real.peso_planejado;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {
                    return Consolidada.peso_planejado;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.peso_planejado;
                }
                return 0;
            }
        }
        public string centro
        {
            get
            {
                if (Obra.tipo == Tipo_Material.Real && this.Material_REAL)
                {
                    return Real.resumo_pecas.centro_producao;
                }
                else if (Obra.tipo == Tipo_Material.Orçamento && this.Material_ORC)
                {
                    return Orcamento.centro;
                }
                return "";
            }
        }
        public Pedido_PMP Obra { get; set; } = new Pedido_PMP();
        public ORC_PEP Orcamento { get; set; } = new ORC_PEP();
        public ORC_PEP Consolidada { get; set; } = new ORC_PEP();
        public PLAN_PEP Real { get; set; } = new PLAN_PEP();
        public PEP_PMP()
        {

        }
        public PEP_PMP(Pedido_PMP obra, PLAN_PEP real, ORC_PEP orcamento, ORC_PEP consolidada)
        {
            this.Obra = obra;

            if(real != null)
            {
                this.Real = real;
                this.Material_REAL = true;
                this.pep = real.pep;
                this.descricao = real.descricao;
            }
            if(orcamento != null)
            {
                this.Orcamento = orcamento;
                this.Material_ORC = true;
                if(this.pep=="")
                {
                    this.pep = this.Orcamento.pep;
                }
            }

            if (consolidada != null)
            {
                this.Consolidada = consolidada;
                this.Material_CONS = true;
                if (this.pep == "")
                {
                    this.pep = this.Consolidada.pep;
                }
            }


            if (this.Material_REAL)
            {
            var sub = this.Obra.Getsubetapas().Find(x => this.pep.StartsWith(x.pep));
                if(sub!=null)
                {
                    if(sub.liberado_engenharia>0)
                    {
                        tipo = Tipo_Material.Real;
                        return;
                    }
                }

                if (this.Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                }
                else if (this.Material_ORC)
                {
                    tipo = Tipo_Material.Orçamento;
                }
            }
            else if (this.Material_CONS)
            {
                tipo = Tipo_Material.Consolidado;
            }
            else if (this.Material_ORC)
            {
                tipo = Tipo_Material.Orçamento;
            }
        }

    }
    public class SubEtapa_PMP : Base_PMP
    {
        public DateTime ei
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ef
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime fi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ff
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime li
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime lf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mi_s
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.mi_s;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mf_s
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.mf_s;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public double liberado_engenharia
        {
            get
            {
                return this.Real.liberado_engenharia;
            }
        }
        public double total_embarcado
        {
            get
            {
                return this.Real.total_embarcado;
            }
        }
        public double total_fabricado
        {
            get
            {
                return this.Real.total_fabricado;
            }
        }
        public double total_montado
        {
            get
            {
                return this.Real.total_montado;
            }
        }
        
        public void mudartipo()
        {
            this._mudartipo();
            foreach (var p in this.Obra.Getpeps().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            NotifyPropertyChanged("peso");
            NotifyPropertyChanged("centro");
            NotifyPropertyChanged("total_embarcado");
            NotifyPropertyChanged("total_fabricado");
        }
        public ImageSource imagem
        {
            get
            {
                if (tipo == Tipo_Material.Real)
                {
                    return imagem_REAL;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {

                    return imagem_CONS;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {

                    return imagem_ORC;
                }
                return Conexoes.BufferImagem.dialog_error;
            }
        }

        public List<PLAN_PECA> Getpecas()
        {
            return Getpeps().SelectMany(x => x.Getpecas()).ToList();
        }

        public double peso
        {
            get
            {
                return Math.Round(Getpeps().Sum(x => x.peso), 2);
            }
        }
        public string centro
        {
            get
            {
                if (Obra.tipo == Tipo_Material.Real && this.Material_REAL)
                {
                    return Real.resumo_pecas.centro_producao;
                }
                else if (Obra.tipo == Tipo_Material.Orçamento && this.Material_ORC)
                {
                    return Orcamento.centro;
                }
                return "";
            }
        }




        public List<PEP_PMP> Getpeps()
        {
            return this.Obra.Getpeps().FindAll(x => x.pep.StartsWith(this.pep) && x.tipo == this.tipo).OrderBy(x => x.pep).ToList();
        }

        public Pedido_PMP Obra { get; set; } = new Pedido_PMP();
        public ORC_SUB Orcamento { get; set; } = new ORC_SUB();
        public ORC_SUB Consolidada { get; set; } = new ORC_SUB();
        public PLAN_SUB_ETAPA Real { get; set; } = new PLAN_SUB_ETAPA();

        public SubEtapa_PMP(Pedido_PMP obra, PLAN_SUB_ETAPA real, ORC_SUB orcamento, ORC_SUB consolidada)
        {
            this.Obra = obra;
            if(real!=null)
            {
            this.Real = real;
                this.Material_REAL = true;
                this.pep = Real.pep;
                this.descricao = real.Titulo.DESCRICAO;

            }

            if (orcamento!=null)
            {
                this.Material_ORC = true;
                this.Orcamento = orcamento;
                if(this.pep == "")
                {
                    this.pep = this.Orcamento.pep;
                }
            }

            if(consolidada!=null)
            {
                this.Material_CONS = true;
                this.Consolidada = consolidada;
                if (this.pep == "")
                {
                    this.pep = this.Consolidada.pep;
                }
            }
     

            if(this.Material_REAL)
            {
                if(this.Real.liberado_engenharia > 0)
                {
                    tipo = Tipo_Material.Real;
                }
                else if (this.Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                }
            }
            else if (this.Material_CONS)
            {
                tipo = Tipo_Material.Consolidado;
            }
            else if(this.Material_ORC)
            {
                tipo = Tipo_Material.Orçamento;
            }
        }

    }
    public class Etapa_PMP :Base_PMP
    {
        public DateTime ei
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ef
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime fi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ff
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime li
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime lf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public double liberado_engenharia
        {
            get
            {
                return this.Real.liberado_engenharia;
            }
        }
        public double total_embarcado
        {
            get
            {
                return this.Real.total_embarcado;
            }
        }
        public double total_fabricado
        {
            get
            {
                return this.Real.total_fabricado;
            }
        }
        public double total_montado
        {
            get
            {
                return this.Real.total_montado;
            }
        }

        public void mudartipo()
        {
            this._mudartipo();
            foreach (var p in this.Obra.Getsubetapas().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            foreach (var p in this.Obra.Getpeps().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            NotifyPropertyChanged("peso");
            NotifyPropertyChanged("centro");
            NotifyPropertyChanged("total_embarcado");
            NotifyPropertyChanged("total_fabricado");
        }
        public ImageSource imagem
        {
            get
            {
                if (tipo == Tipo_Material.Real)
                {
                    return imagem_REAL;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {

                    return imagem_CONS;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {

                    return imagem_ORC;
                }
                return Conexoes.BufferImagem.dialog_error;
            }
        }

 

        public double peso
        {
            get
            {
                return Math.Round(Getsubetapas().Sum(x => x.peso),2);
            }
        }
        public string centro
        {
            get
            {
                if (Obra.tipo == Tipo_Material.Real && this.Material_REAL)
                {
                    return Real.resumo_pecas.centro_producao;
                }
                else if (Obra.tipo == Tipo_Material.Orçamento && this.Material_ORC)
                {
                    return Orcamento.centro;
                }
                return "";
            }
        }


        public List<SubEtapa_PMP> Getsubetapas()
        {
            return this.Obra.Getsubetapas().FindAll(x => x.pep.StartsWith(this.pep)).ToList();
        }



        public Pedido_PMP Obra { get; set; } = new Pedido_PMP();
        public ORC_ETP Orcamento { get; set; } = new ORC_ETP();
        public ORC_ETP Consolidada { get; set; } = new ORC_ETP();
        public PLAN_ETAPA Real { get; set; } = new PLAN_ETAPA();

        public Etapa_PMP(Pedido_PMP obra, PLAN_ETAPA real, ORC_ETP orcamento , ORC_ETP consolidada)
        {
            this.Obra = obra;
            if (real != null)
            {
                this.Real = real;

                this.Material_REAL = true;
                this.descricao = real.Titulo.DESCRICAO;
                this.pep = real.pep;
            }
            if (orcamento != null)
            {
                this.Orcamento = orcamento;
                this.Material_ORC = true;
                if (this.pep == "")
                {
                    this.pep = this.Orcamento.pep;
                }
            }


            if(consolidada!=null)
            {
                this.Material_CONS = true;
                this.Consolidada = consolidada;
                if (this.pep == "")
                {
                    this.pep = this.Consolidada.pep;
                }
            }

            if (this.Material_REAL)
            {
                if (this.Real.liberado_engenharia > 0)
                {
                    tipo = Tipo_Material.Real;
                }
                else if (this.Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                }
            }
            else if (this.Material_CONS)
            {
                tipo = Tipo_Material.Consolidado;
            }
            else if (this.Material_ORC)
            {
                tipo = Tipo_Material.Orçamento;
            }
        }

    }
    public class Pedido_PMP : Base_PMP
    {
        public DateTime ultima_edicao { get; set; } = new DateTime();
        public DateTime ei
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ef
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.engenharia_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.engenharia_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime fi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime ff
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.fabrica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.fabrica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime li
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime lf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.logistica_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.logistica_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mi
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma_inicio;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma_inicio;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public DateTime mf
        {
            get
            {
                if (tipo == Tipo_Material.Consolidado | tipo == Tipo_Material.Real)
                {
                    return Real.montagem_cronograma;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {
                    return Orcamento.montagem_cronograma;
                }
                else
                {
                    return new DateTime();
                }
            }
        }
        public double peso_planejado
        {
            get
            {
                return Real.peso_planejado;
            }
        }
        private List<Grupo_Mercadoria> _Grupos_Mercadoria { get; set; }


        public void mudartipo(Tipo_Material tipo)
        {
           if(tipo == Tipo_Material.Consolidado && Material_CONS)
            {
                this.tipo = tipo;
            }
           else if (tipo == Tipo_Material.Orçamento && Material_ORC)
            {
                this.tipo = tipo;
            }
            else if (tipo == Tipo_Material.Real && Material_REAL)
            {
                this.tipo = tipo;
            }
           else
            {
                return;
            }

            foreach (var p in this.Getetapas().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            foreach (var p in this.Getsubetapas().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            foreach (var p in this.Getpeps().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            NotifyPropertyChanged("peso");
            NotifyPropertyChanged("total_embarcado");
            NotifyPropertyChanged("total_fabricado");
        }
        public void mudartipo()
        {
            this._mudartipo();
            foreach (var p in this.Getetapas().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            foreach (var p in this.Getsubetapas().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            foreach (var p in this.Getpeps().FindAll(x => x.pep.StartsWith(this.pep)))
            {
                p._forcartipo(this.tipo);
            }
            NotifyPropertyChanged("peso");
            NotifyPropertyChanged("total_embarcado");
            NotifyPropertyChanged("total_fabricado");
        }
        public ImageSource imagem
        {
            get
            {
                if (tipo == Tipo_Material.Real)
                {
                    return imagem_REAL;
                }
                else if (tipo == Tipo_Material.Orçamento)
                {

                    return imagem_ORC;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {

                    return imagem_CONS;
                }
                return Conexoes.BufferImagem.dialog_error;
            }
        }

        public double liberado_engenharia
        {
            get
            {
                return this.Real.liberado_engenharia;
            }
        }
        public double total_embarcado
        {
            get
            {
                return this.Real.total_embarcado;
            }
        }
        public double total_fabricado
        {
            get
            {
                return this.Real.total_fabricado;
            }
        }
        public double total_montado
        {
            get
            {
                return this.Real.total_montado;
            }
        }
        public double peso
        {
            get
            {
                return Math.Round(Getpeps().Sum(x => x.peso), 2);
            }
        }
        public override string ToString()
        {
            return this.pep + (this.Material_ORC ? "[O]" : "") + (this.Material_REAL ? "[R]" : "") + " - " + this.descricao;
        }
        private List<Etapa_PMP> _etapas { get; set; }
        private List<SubEtapa_PMP> _subetapas { get; set; }
        private List<PEP_PMP> _peps { get; set; }

        public List<Etapa_PMP> Getetapas()
        {
            if (_etapas == null)
            {
                _etapas = new List<Etapa_PMP>();
                var reais = Real.Getetapas();
                var orcs = Orcamento.Getetapas_orcamento();
                var cons = Consolidada.Getetapas_orcamento();
                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var s in lista)
                {
                    var real = reais.Find(x => x.pep == s);
                    var orc = orcs.Find(x => x.pep == s);
                    var con = cons.Find(x => x.pep == s);
                    if (real != null | orc != null | con != null)
                    {
                        _etapas.Add(new Etapa_PMP(this, real, orc, con));
                    }
                }
            }
            return _etapas;
        }
        public List<SubEtapa_PMP> Getsubetapas()
        {
            if (_subetapas == null)
            {
                _subetapas = new List<SubEtapa_PMP>();
                var reais = Real.GetSubEtapas();
                var orcs = Orcamento.subetapas;
                var cons = Consolidada.subetapas;

                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var s in lista)
                {
                    var real = reais.Find(x => x.pep == s);
                    var orc = orcs.Find(x => x.pep == s);
                    var con = cons.Find(x => x.pep == s);
                    if (real != null | orc != null | con != null)
                    {
                        _subetapas.Add(new SubEtapa_PMP(this, real, orc, con));
                    }

                }

            }
            return _subetapas;
        }
        public List<PEP_PMP> Getpeps()
        {
            if (_peps == null)
            {
                _peps = new List<PEP_PMP>();
                var reais = Real.GetPeps();
                var orcs = Orcamento.peps;
                var cons = Consolidada.peps;
                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var s in lista)
                {
                    var real = reais.Find(x => x.pep == s);
                    var orc = orcs.Find(x => x.pep == s);
                    var con = cons.Find(x => x.pep == s);
                    if (real != null | orc != null | con!=null)
                    {
                        _peps.Add(new PEP_PMP(this, real, orc, con));
                    }

                }
            }
            return _peps;
        }
        public void Set(List<PLAN_PECA> pecas)
        {
            this._pecas = new List<PLAN_PECA>();
            this._pecas.AddRange(pecas.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public void Set(List<SubEtapa_PMP> pecas)
        {
            this._subetapas = new List<SubEtapa_PMP>();
            this._subetapas.AddRange(pecas.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public void Set(List<PEP_PMP> pecas)
        {
            this._peps = new List<PEP_PMP>();
            this._peps.AddRange(pecas.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public void Set(List<Etapa_PMP> pecas)
        {
            this._etapas = new List<Etapa_PMP>();
            this._etapas.AddRange(pecas.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        private List<PLAN_PECA> _pecas { get; set; }
        public List<PLAN_PECA> Getpecas()
        {
            if (_pecas == null)
            {
                _pecas = DLM.painel.Consultas.GetPecasPMP(new List<Pedido_PMP> { this });
            }
            return _pecas;
        }
        public PLAN_PEDIDO Real { get; set; } = new PLAN_PEDIDO();
        public ORC_PED Orcamento { get; set; } = new ORC_PED();
        public ORC_PED Consolidada { get; set; } = new ORC_PED();
        public Pedido_PMP(PLAN_PEDIDO real, ORC_PED orcamento, ORC_PED consolidado)
        {
        
            if (real != null)
            {

                this.Material_REAL = true;
                this.Real = real;
                this.pep = real.pedido;
                this.descricao = real.Titulo.DESCRICAO;
            }
            if (orcamento!=null)
            {
            this.Orcamento = orcamento;
            this.Material_ORC = true;
                if(pep=="")
                {
                    this.pep = Orcamento.pep;
                    this.descricao = orcamento.descricao;
                }
            }

            if(consolidado!=null)
            {
                this.Consolidada = consolidado;
                this.Material_CONS = true;
                if(this.pep == "")
                {
                    this.pep = this.Consolidada.pep;
                    this.descricao = this.Consolidada.descricao;

                }
            }

            if(this.Orcamento!=null && descricao=="")
            {
                this.descricao = this.Orcamento.descricao;
            }
            if (this.Consolidada != null && descricao == "")
            {
                this.descricao = this.Consolidada.descricao;
            }
            if (this.Material_REAL)
            {
                if (this.Real.liberado_engenharia > 0)
                {
                    tipo = Tipo_Material.Real;
                    ultima_edicao = real.ultima_consulta_sap;
                }
                else if (this.Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                    ultima_edicao = consolidado.ultima_consulta_sap;

                }
            }
            else if (this.Material_CONS)
            {
                tipo = Tipo_Material.Consolidado;
                ultima_edicao = consolidado.ultima_consulta_sap;
            }
            else if (this.Material_ORC)
            {
                tipo = Tipo_Material.Orçamento;
                ultima_edicao = orcamento.ultima_consulta_sap;
            }

            //if(real.ultima_consulta_sap> consolidado.ultima_consulta_sap)
            //{
            //    ultima_edicao = real.ultima_consulta_sap;
            //}
            //else if(consolidado.ultima_consulta_sap> orcamento.ultima_consulta_sap)
            //{
            //    ultima_edicao = consolidado.ultima_consulta_sap;
            //}
            //else
            //{
            //    ultima_edicao = orcamento.ultima_consulta_sap;
            //}
        }

        public Pedido_PMP()
        {

        }
    }


    public class Pacote_PMP
    {
        public List<Pedido_PMP> pedidos { get; set; } = new List<Pedido_PMP>();

        private List<Etapa_PMP> _etapas { get; set; }
        private List<SubEtapa_PMP> _subetapas { get; set; }
        private List<PEP_PMP> _peps { get; set; }

        private List<PLAN_PECA> _pecas { get; set; }

        public List<PLAN_PECA> GetPecas()
        {
            if(_pecas ==null)
            {
                _pecas = DLM.painel.Consultas.GetPecasPMP(this.pedidos);
            }
            return _pecas;
        }

        public List<PEP_PMP> Getpeps()
        {
            if (_peps == null)
            {
                _peps = new List<PEP_PMP>();
                var reais = Consultas.GetPeps(this.pedidos.Select(x => x.pep).Distinct().ToList());
                var orcs = DLM.painel.Consultas.GetPEPsPGO(this.pedidos.Select(x => x.pep).Distinct().ToList(), false);
                var cons = DLM.painel.Consultas.GetPEPsPGO(this.pedidos.Select(x => x.pep).Distinct().ToList(), true);
                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var ped in this.pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(ped.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.pep == s);
                        var orc = orcs.Find(x => x.pep == s);
                        var con = cons.Find(x => x.pep == s);
                        if (real != null | orc != null | con != null)
                        {
                            _peps.Add(new PEP_PMP(ped, real, orc, con));
                        }
                    }
                    ped.Set(_peps);
                }

            }
            return _peps;
        }
        public List<SubEtapa_PMP> Getsubetapas()
        {
            if (_subetapas == null)
            {
                var etp = GetEtapas();
                _subetapas = new List<SubEtapa_PMP>();
                var reais = this.pedidos.SelectMany(x => x.Real.GetSubEtapas()).ToList();
                    
                var orcs = Consultas.GetSubEtapasPGO(this.pedidos.Select(x=>x.pep).Distinct().ToList(),false);
                var cons = Consultas.GetSubEtapasPGO(this.pedidos.Select(x => x.pep).Distinct().ToList(), true);

                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var ped in this.pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(ped.pep));
                    var etapas = etp.FindAll(x => x.pep.StartsWith(ped.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.pep == s);
                        var orc = orcs.Find(x => x.pep == s);
                        var con = cons.Find(x => x.pep == s);
                        if (real != null | orc != null | con != null)
                        {
                            var nnn = new SubEtapa_PMP(ped, real, orc, con);
                            _subetapas.Add(nnn);
                        
                        }
                    }

                   ped.Set(_subetapas);
                }
                
            }
            return _subetapas;
        }

        public List<Etapa_PMP> GetEtapas()
        {
            if(_etapas ==null)
            {
                _etapas = new List<Etapa_PMP>();
                var reais = Consultas.GetEtapas(this.pedidos.Select(x=>x.pep).Distinct().ToList());
                var orcs = DLM.painel.Consultas.GetEtapasPGO(this.pedidos.Select(x => x.pep).Distinct().ToList(), false);
                var cons = DLM.painel.Consultas.GetEtapasPGO(this.pedidos.Select(x => x.pep).Distinct().ToList(), true);
                var lista = reais.Select(x => x.pep).Distinct().ToList();
                lista.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.pep).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach(var ped in this.pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(ped.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.pep == s);
                        var orc = orcs.Find(x => x.pep == s);
                        var con = cons.Find(x => x.pep == s);
                        if (real != null | orc != null | con != null)
                        {
                            _etapas.Add(new Etapa_PMP(ped, real, orc, con));
                        }
                    }
                    ped.Set(_etapas);
                }

            }
            return _etapas;
        }

        public Pacote_PMP(List<Pedido_PMP> pedidos)
        {
            this.pedidos = pedidos;

            this.pedidos[0].Getetapas();
        }
    }
}
