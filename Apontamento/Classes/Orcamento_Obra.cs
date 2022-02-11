using DLM.sapgui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{

    public class Base_Orcamento : PLAN_BASE
    {
        public void GetDatas()
        {
            this.engenharia_cronograma_inicio = L.Get("ei").Data();
            this.engenharia_cronograma = L.Get("ef").Data();

            this.fabrica_cronograma_inicio = L.Get("fi").Data();
            this.fabrica_cronograma = L.Get("ff").Data();

            this.logistica_cronograma_inicio = L.Get("li").Data();
            this.logistica_cronograma = L.Get("lf").Data();

            this.montagem_cronograma_inicio = L.Get("mi").Data();
            this.montagem_cronograma = L.Get("mf").Data();
        }


        public string unidade_fabril
        {
            get
            {
                if (pep.Length >= 24)
                {
                    return pep.Substring(22, 2);
                }
                return "";
            }
        }



        public override string ToString()
        {
            return this.pep + "[" + this.peso_planejado + "]";
        }
        public Tipo_Material tipo { get; set; } = Tipo_Material.Orçamento;
        private List<Peca_Planejamento> _pecas { get; set; }
        public List<Peca_Planejamento> GetPecasPMP()
        {
            if (_pecas == null)
            {
                _pecas = DLM.painel.Consultas.GetPecasPGO(new List<string> { this.pep },10, tipo == Tipo_Material.Consolidado);
            }
            return _pecas;
        }

        public long id_obra { get; set; } = 0;

        public string centro { get; set; } = "";
        public double quantidade { get; set; } = 0;

        public Base_Orcamento()
        {

        }
    }
    public class Subetapa_Orcamento : Base_Orcamento
    {
        private List<PEP_Orcamento> _peps { get; set; }

        public List<PEP_Orcamento> peps
        {
            get
            {
               if(_peps==null)
                {
                    _peps = Consultas.GetPEPsPGO(new List<string> { this.pep });
                }
                return _peps;
            }
        }

        public Subetapa_Orcamento()
        {

        }

        public Subetapa_Orcamento(DLM.db.Linha l)
        {
            this.L = l;
            this.id_obra = l.Get("id_obra").Int();
            this.pep = l.Get("pep").ToString();
            this.peso_planejado = l.Get("peso_total").Double();
            this.quantidade = l.Get("quantidade").Int();
        }
    }

    public class Etapa_Orcamento : Base_Orcamento
    {
        private List<PEP_Orcamento> _peps { get; set; }

        public List<PEP_Orcamento> peps
        {
            get
            {
                if (_peps == null)
                {
                    _peps = Consultas.GetPEPsPGO(new List<string> { this.pep });
                }
                return _peps;
            }
        }
        public void Set(List<Subetapa_Orcamento> subs)
        {
            this._subetapas = subs.FindAll(x => x.pep.StartsWith(this.pep));
        }

        private List<Subetapa_Orcamento> _subetapas { get; set; }
        public List<Subetapa_Orcamento> subetapas
        {
            get
            {
                if(_subetapas==null)
                {
                    _subetapas = Consultas.GetSubEtapasPGO(new List<string> { this.pep });
                }
                return _subetapas;
            }
        }

        public Etapa_Orcamento()
        {

        }
        public Etapa_Orcamento(DLM.db.Linha l)
        {
            this.L = l;
            this.id_obra = l.Get("id_obra").Int();
            this.pep = l.Get("pep").ToString();
            this.peso_planejado = l.Get("peso_total").Double();
            this.quantidade = l.Get("quantidade").Double();
        
        }
    }

    public class PEP_Orcamento :Base_Orcamento
    {
        public string pep_inicial { get; set; } = "";
        public PEP_Orcamento()
        {

        }
        public PEP_Orcamento(DLM.db.Linha l)
        {
            this.L = l;

            this.id_obra = l.Get("id_obra").Int();
            this.pep = l.Get("pep").ToString();
            this.pep_inicial = l.Get("pep_inicial").ToString();
            this.centro = l.Get("centro").ToString();
            this.peso_planejado = l.Get("peso_total").Double();
            this.quantidade = l.Get("quantidade").Double();



        }
    }

    public class Pacote_Orcamento
    {
        public override string ToString()
        {
            return arquivo;
        }
        private List<string> peps_str { get; set; } = new List<string>();
        public Pedido_Orcamento pedido { get; set; } = new Pedido_Orcamento();
        public List<PEP_Orcamento> peps
        {
            get
            {
                return this.pedido.peps.FindAll(x => peps_str.Find(y => y == x.pep) != null);
            }
        }
        public string arquivo { get; set; } = "";
        public Pacote_Orcamento(string arquivo, List<string> peps, Pedido_Orcamento pedido)
        {
            this.arquivo = arquivo;
            this.peps_str = peps;
            this.pedido = pedido;
        }
    }

   public  class Pedido_Orcamento : Base_Orcamento
    {

        public List<Pacote_Orcamento> pacotes { get; set; } = new List<Pacote_Orcamento>();
        private List<PEP_Orcamento> _peps { get; set; }
        public List<PEP_Orcamento> peps
        {
            get
            {
                if (_peps == null)
                {
                    _peps = Consultas.GetPEPsPGO(new List<string> { this.pep }, this.tipo == Tipo_Material.Consolidado);
                }
                return _peps;
            }
        }
        private List<Subetapa_Orcamento> _subetapas { get; set; }
        public List<Subetapa_Orcamento> subetapas
        {
            get
            {
                if (_subetapas == null)
                {
                    _subetapas = Consultas.GetSubEtapasPGO(new List<string> { this.pep }, this.tipo == Tipo_Material.Consolidado);
                    if(this._etapas!=null)
                    {
                        foreach(var t in this._etapas)
                        {
                            t.Set(_subetapas);
                        }
                    }
                }
                return _subetapas;
            }
        }

        private List<Etapa_Orcamento> _etapas { get; set; }

        public List<Etapa_Orcamento> Getetapas_orcamento()
        {
            if (_etapas == null)
            {
                _etapas = DLM.painel.Consultas.GetEtapasPGO(new List<string> { this.pep }, this.tipo == Tipo_Material.Consolidado);
            }
            return _etapas;
        }
        public string numerocontrato { get; set; } = "";
        public string revisao { get; set; } = "";

        public double peso_realizado { get; set; } = 0;
        public int arquivos { get; set; } = 0;

        public Pedido_Orcamento()
        {

        }
        public override string ToString()
        {
            return "[PGO] - [" + this.pep + "] - " + this.descricao + " [" + this.numerocontrato + "." + this.revisao + "]";
        }

        public DateTime criacao { get; set; } = new DateTime();
        public Pedido_Orcamento(DLM.db.Linha l, Tipo_Material tipo)
        {
            this.L = l;
            this.id_obra = l.Get("id_obra").Int();
            this.numerocontrato = l.Get("numerocontrato").ToString();
            this.revisao = l.Get("revisao").ToString();
            this.Titulo.DESCRICAO = l.Get("descricao").ToString();
            this.pep = l.Get("pedido").ToString().ToString();
            this.quantidade = l.Get("quantidade").Double();
            this.peso_planejado = l.Get("peso_total").Double();

            this.liberado_engenharia = l.Get("liberado_engenharia").Double();
            this.peso_realizado = l.Get("peso_realizado").Double();
            this.arquivos = l.Get("arquivos").Int();
            this.criacao = l.Get("criacao").Data();
            this.tipo = tipo;
            if(this.descricao == "")
            {
                this.Titulo.DESCRICAO = l.Get("nome").Valor;
            }
        }
    }
}
