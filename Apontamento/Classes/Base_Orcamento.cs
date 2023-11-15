using Conexoes;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;

namespace DLM.painel
{

    public class ORC_BASE : PLAN_BASE
    {
        public void GetDatas()
        {
            this.engenharia_cronograma_inicio = Linha["ei"].Data();
            this.engenharia_cronograma = Linha["ef"].Data();

            this.fabrica_cronograma_inicio = Linha["fi"].Data();
            this.fabrica_cronograma = Linha["ff"].Data();

            this.logistica_cronograma_inicio = Linha["li"].Data();
            this.logistica_cronograma = Linha["lf"].Data();

            this.montagem_cronograma_inicio = Linha["mi"].Data();
            this.montagem_cronograma = Linha["mf"].Data();

            this.mi_s = Linha["mi_s"].Data();
            this.mf_s = Linha["mf_s"].Data();
        }


        public string unidade_fabril
        {
            get
            {
                if (PEP.Length >= 24)
                {
                    return PEP.Substring(22, 2);
                }
                return "";
            }
        }



        public override string ToString()
        {
            return this.PEP + "[" + this.peso_planejado + "]";
        }
        public Tipo_Material tipo { get; set; } = Tipo_Material.Orçamento;
        private List<PLAN_PECA> _pecas { get; set; }
        public List<PLAN_PECA> GetPecasPMP()
        {
            if (_pecas == null)
            {
                _pecas = DLM.painel.Consultas.GetPecasPGO(new List<string> { this.PEP },10, tipo == Tipo_Material.Consolidado);
            }
            return _pecas;
        }

        public long id_obra { get; set; } = 0;

        public string centro { get; set; } = "";
        public double quantidade { get; set; } = 0;

        public ORC_BASE()
        {

        }
    }
    public class ORC_SUB : ORC_BASE
    {
        private List<ORC_PEP> _peps { get; set; }

        public List<ORC_PEP> peps
        {
            get
            {
               if(_peps==null)
                {
                    _peps = Consultas.GetPEPsPGO(new List<string> { this.PEP });
                }
                return _peps;
            }
        }

        public ORC_SUB()
        {

        }

        public ORC_SUB(DLM.db.Linha linha)
        {
            this.Linha = linha;
            this.id_obra = linha["id_obra"].Int();
            this.PEP = linha["pep"].Valor;
            this.peso_planejado = linha["peso_total"].Double();
            this.quantidade = linha["quantidade"].Int();
        }
    }

    public class ORC_ETP : ORC_BASE
    {
        private List<ORC_PEP> _peps { get; set; }

        public List<ORC_PEP> peps
        {
            get
            {
                if (_peps == null)
                {
                    _peps = Consultas.GetPEPsPGO(new List<string> { this.PEP });
                }
                return _peps;
            }
        }
        public void Set(List<ORC_SUB> subs)
        {
            this._subetapas = subs.FindAll(x => x.PEP.StartsW(this.PEP));
        }

        private List<ORC_SUB> _subetapas { get; set; }
        public List<ORC_SUB> subetapas
        {
            get
            {
                if(_subetapas==null)
                {
                    _subetapas = Consultas.GetSubEtapasPGO(new List<string> { this.PEP });
                }
                return _subetapas;
            }
        }

        public ORC_ETP()
        {

        }
        public ORC_ETP(DLM.db.Linha linha)
        {
            this.Linha = linha;
            this.id_obra =          linha["id_obra"].Int();
            this.PEP =              linha["pep"].Valor;
            this.peso_planejado =   linha["peso_total"].Double();
            this.quantidade =       linha["quantidade"].Double();
        
        }
    }

    public class ORC_PEP :ORC_BASE
    {
        public string pep_inicial { get; set; } = "";
        public ORC_PEP()
        {

        }
        public ORC_PEP(DLM.db.Linha linha)
        {
            this.Linha = linha;

            this.id_obra =          linha["id_obra"].Int();
            this.PEP =              linha["pep"].Valor;
            this.pep_inicial =      linha["pep_inicial"].Valor;
            this.centro =           linha["centro"].Valor;
            this.peso_planejado =   linha["peso_total"].Double();
            this.quantidade =       linha["quantidade"].Double();



        }
    }

    public class ORC_PCK
    {
        public override string ToString()
        {
            return arquivo;
        }
        private List<string> peps_str { get; set; } = new List<string>();
        public ORC_PED pedido { get; set; } = new ORC_PED();
        public List<ORC_PEP> peps
        {
            get
            {
                return this.pedido.GetPEPs().FindAll(x => peps_str.Find(y => y == x.PEP) != null);
            }
        }
        public string arquivo { get; set; } = "";
        public ORC_PCK(string arquivo, List<string> peps, ORC_PED pedido)
        {
            this.arquivo = arquivo;
            this.peps_str = peps;
            this.pedido = pedido;
        }
    }

   public  class ORC_PED : ORC_BASE
    {

        public List<ORC_PCK> pacotes { get; set; } = new List<ORC_PCK>();
        private List<ORC_PEP> _peps { get; set; }

        public List<ORC_PEP> GetPEPs()
        {
            if (_peps == null)
            {
                _peps = Consultas.GetPEPsPGO(new List<string> { this.PEP }, this.tipo == Tipo_Material.Consolidado);
            }
            return _peps;
        }
        private List<ORC_SUB> _subetapas { get; set; }

        public List<ORC_SUB> GetSubetapas()
        {
            if (_subetapas == null)
            {
                _subetapas = Consultas.GetSubEtapasPGO(new List<string> { this.PEP }, this.tipo == Tipo_Material.Consolidado);
                if (this._etapas != null)
                {
                    foreach (var t in this._etapas)
                    {
                        t.Set(_subetapas);
                    }
                }
            }
            return _subetapas;
        }

        private List<ORC_ETP> _etapas { get; set; }

        public List<ORC_ETP> Getetapas_orcamento()
        {
            if (_etapas == null)
            {
                _etapas = DLM.painel.Consultas.GetEtapasPGO(new List<string> { this.PEP }, this.tipo == Tipo_Material.Consolidado);
            }
            return _etapas;
        }
        public string numerocontrato { get; set; } = "";
        public string revisao { get; set; } = "";

        public double peso_realizado { get; set; } = 0;
        public int arquivos { get; set; } = 0;

        public ORC_PED()
        {

        }
        public override string ToString()
        {
            return "[PGO] - [" + this.PEP + "] - " + this.descricao + " [" + this.numerocontrato + "." + this.revisao + "]";
        }

        public DateTime criacao { get; set; } = Cfg.Init.DataDummy();
        public ORC_PED(DLM.db.Linha linha, Tipo_Material tipo)
        {
            this.Linha = linha;
            this.id_obra =                  linha["id_obra"].Int();
            this.numerocontrato =           linha["numerocontrato"].Valor;
            this.revisao =                  linha["revisao"].Valor;
            this.Titulo.Descricao =         linha["descricao"].Valor;
            this.PEP =                      linha["pedido"].Valor;
            this.quantidade =               linha["quantidade"].Double();
            this.peso_planejado =           linha["peso_total"].Double();
            this.liberado_engenharia =      linha["liberado_engenharia"].Double();
            this.peso_realizado =           linha["peso_realizado"].Double();
            this.arquivos =                 linha["arquivos"].Int();
            this.criacao =                  linha["criacao"].Data();
            this.Titulo.Descricao =         linha["nome"].Valor;
            this.tipo = tipo;
        }
    }
}
