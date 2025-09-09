using Conexoes;
using DLM.sap;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace DLM.painel
{
    public class Base_PMP : Notificar
    {
        public override string ToString()
        {
            return this.pep + " - [" + this.tipo.ToString() + "]";
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
        public Base_PMP()
        {

        }
    }
    public class PEP_PMP : Base_PMP
    {


        public List<PLAN_PECA> Getpecas()
        {
            var retorno = new List<PLAN_PECA>();

            var pecas = Obra.Pecas.FindAll(x => x.PEP.StartsWith(this.pep));

            var tipos = pecas.GroupBy(x => x.Tipo).ToList();
            if (tipos.Count > 0)
            {
                var tipo_real = tipos.Find(x => x.Key == Tipo_Material.Real);
                if (tipo_real != null)
                {
                    retorno.AddRange(tipo_real.ToList());
                }
                else
                {
                    retorno.AddRange(tipos.First().ToList());
                }
            }
            else
            {
                retorno.AddRange(pecas);
            }
            return retorno;
        }

        public double peso
        {
            get
            {
                if (tipo == Tipo_Material.Real)
                {
                    return Real.peso_planejado;
                }
                else if (tipo == Tipo_Material.Consolidado)
                {
                    return Consolidada.peso_planejado;
                }
                return 0;
            }
        }
        public Pedido_PMP Obra { get; set; } = new Pedido_PMP();
        public ORC_PEP Consolidada { get; set; } = new ORC_PEP();
        public PLAN_PEP Real { get; set; } = new PLAN_PEP();
        public ZPP0100_Resumo Embarque { get; set; } = new ZPP0100_Resumo();
        public PEP_PMP()
        {

        }
        public PEP_PMP(Pedido_PMP obra, PLAN_PEP real, ORC_PEP orcamento, ORC_PEP consolidada, ZPP0100_Resumo Embarque)
        {
            if (Embarque != null)
            {
                this.Embarque = Embarque;
            }
            this.Obra = obra;

            if (real != null)
            {
                this.Real = real;
                this.Material_REAL = true;
                this.pep = real.PEP;
                this.descricao = real.descricao;
            }



            if (this.Material_REAL)
            {
                var sub = this.Obra.SupEtapas.Find(x => this.pep.StartsWith(x.pep));
                if (sub != null)
                {
                    if (sub.liberado_engenharia > 0)
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
        public DateTime? ei => Real?.engenharia_cronograma_inicio;
        public DateTime? ef => Real?.engenharia_cronograma;
        public DateTime? fi => Real?.fabrica_cronograma_inicio;
        public DateTime? ff => Real?.fabrica_cronograma;
        public DateTime? li => Real?.logistica_cronograma_inicio;
        public DateTime? lf => Real?.logistica_cronograma;
        public DateTime? mi => Real?.montagem_cronograma_inicio;
        /// <summary>
        /// LOB
        /// </summary>
        public DateTime? mi_s => Real?.mi_s;
        /// <summary>
        /// LOB
        /// </summary>
        public DateTime? mf_s => Real?.mf_s;
        public DateTime? mf => Real?.montagem_cronograma;
        public double? total_embarcado => Real?.total_embarcado;
        public double? liberado_engenharia => Real?.liberado_engenharia;
        public double? total_fabricado => this.Real?.total_fabricado;

        public double peso => Getpeps().Sum(x => x.peso).Round(2);

        public List<PEP_PMP> Getpeps()
        {
            return this.Obra.PEPs.FindAll(x => x.pep.StartsWith(this.pep) && x.tipo == this.tipo).OrderBy(x => x.pep).ToList();
        }

        public Pedido_PMP Obra { get; set; } = new Pedido_PMP();
        public ORC_SUB Consolidada { get; set; } = new ORC_SUB();
        public PLAN_SUB_ETAPA Real { get; set; } = new PLAN_SUB_ETAPA();
        public ZPP0100_Resumo Embarque { get; set; } = new ZPP0100_Resumo();
        public SubEtapa_PMP(Pedido_PMP obra, PLAN_SUB_ETAPA real, ORC_SUB orcamento, ORC_SUB consolidada, ZPP0100_Resumo Embarque)
        {
            this.Obra = obra;
            if (Embarque != null)
            {
                this.Embarque = Embarque;
            }

            if (real != null)
            {
                this.Real = real;
                this.Material_REAL = true;
                this.pep = Real.PEP;
                this.descricao = real.descricao;
            }

            if (consolidada != null)
            {
                this.Material_CONS = true;
                this.Consolidada = consolidada;
                if (this.pep == "")
                {
                    this.pep = this.Consolidada.PEP;
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
    public class Etapa_PMP : Base_PMP
    {
        public ORC_ETP Consolidada { get; set; } = new ORC_ETP();
        public PLAN_ETAPA Real { get; set; } = new PLAN_ETAPA();
        public Etapa_PMP(Pedido_PMP obra, PLAN_ETAPA real, ORC_ETP orcamento, ORC_ETP consolidada)
        {
            if (real != null)
            {
                this.Real = real;
                this.Material_REAL = true;
                this.descricao = real.descricao;
                this.pep = real.PEP;
            }


            if (consolidada != null)
            {
                this.Material_CONS = true;
                this.Consolidada = consolidada;
                if (this.pep == "")
                {
                    this.pep = this.Consolidada.PEP;
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
        public override string ToString()
        {
            return this.pep + (this.Material_CONS ? "[O]" : "") + (this.Material_REAL ? "[R]" : "") + " - " + this.descricao;
        }
        public DateTime? Ultima_Edicao { get; set; }
        public DateTime? Criado { get; set; }

        public bool SAP { get; set; } = false;
        public bool Terceirizacao { get; set; } = false;
        public bool Finalizado { get; set; } = false;
        public bool Meta { get; set; } = false;

        public List<Etapa_PMP> Etapas { get; private set; } = new List<Etapa_PMP>();
        public List<SubEtapa_PMP> SupEtapas { get; private set; } = new List<SubEtapa_PMP>();
        public List<PEP_PMP> PEPs { get; private set; } = new List<PEP_PMP>();

        public void Set(List<PLAN_PECA> itens)
        {
            this.Pecas = new List<PLAN_PECA>();
            this.Pecas.AddRange(itens.FindAll(x => x.PEP.StartsWith(this.pep)));
        }
        public void Set(List<SubEtapa_PMP> itens)
        {
            this.SupEtapas = new List<SubEtapa_PMP>();
            this.SupEtapas.AddRange(itens.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public void Set(List<PEP_PMP> itens)
        {
            this.PEPs = new List<PEP_PMP>();
            this.PEPs.AddRange(itens.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public void Set(List<Etapa_PMP> itens)
        {
            this.Etapas = new List<Etapa_PMP>();
            this.Etapas.AddRange(itens.FindAll(x => x.pep.StartsWith(this.pep)));
        }
        public List<PLAN_PECA> Pecas { get; private set; } = new List<PLAN_PECA>();
        public PLAN_PEDIDO Real { get; set; } = new PLAN_PEDIDO();
        public ORC_PED Consolidada { get; set; } = new ORC_PED();


        public Pedido_PMP(SAP_Pedido sap)
        {
            this.pep = sap.PEP;
            this.descricao = sap.Descricao;
            this.Ultima_Edicao = sap.Criado;
            this.tipo = Tipo_Material.Real;
            this.Terceirizacao = sap.Terceirizacao;
            this.Finalizado = sap.Finalizado;
            var avanco = sap.Get_SAP_Avanco();
            if (avanco != null)
            {
                this.Real = new PLAN_PEDIDO(avanco);

                this.Ultima_Edicao = this.Real.ultima_edicao;
            }
        }
        public Pedido_PMP(PLAN_PEDIDO real, ORC_PED orcamento, ORC_PED consolidado)
        {

            if (real != null)
            {
                this.Material_REAL = true;
                this.Real = real;
                this.pep = real.pedido;
                this.descricao = real.descricao;
                this.Criado = real.criado;
            }

            if (consolidado != null)
            {
                this.Meta = true;
                this.Consolidada = consolidado;
                this.Material_CONS = true;
                this.Criado = consolidado.criado;

                if (this.pep == "")
                {
                    this.pep = this.Consolidada.PEP;
                }
                if (this.descricao == "")
                {
                    this.descricao = this.Consolidada.descricao;
                }
            }

            if (this.Consolidada != null && descricao == "")
            {
                this.descricao = this.Consolidada.descricao;
            }



            if (descricao == "" | descricao == pep)
            {
                if (this.contrato.ESoNumero())
                {
                    var ped = DLM.SAP.GetPedido(this.pep);
                    if (ped != null)
                    {
                        this.descricao = ped.Descricao;
                    }
                }
                else
                {

                }
            }
            if (this.Material_REAL)
            {
                if (this.Real.liberado_engenharia > 0)
                {
                    tipo = Tipo_Material.Real;
                    Ultima_Edicao = real.ultima_consulta_sap;
                }
                else if (this.Material_CONS)
                {
                    tipo = Tipo_Material.Consolidado;
                    Ultima_Edicao = consolidado.ultima_consulta_sap;

                }
            }
            else if (this.Material_CONS)
            {
                tipo = Tipo_Material.Consolidado;
                Ultima_Edicao = consolidado.ultima_consulta_sap;
            }
        }
        public Pedido_PMP()
        {

        }
    }


    public class Pacote_PMP
    {
        private List<Etapa_PMP> _etapas { get; set; }
        private List<SubEtapa_PMP> _subetapas { get; set; }
        private List<PEP_PMP> _peps { get; set; }
        private List<PLAN_PECA> _pecas { get; set; }

        public List<Pedido_PMP> Pedidos { get; set; } = new List<Pedido_PMP>();
        public List<PLAN_PECA> GetPecas()
        {
            if (_pecas == null)
            {
                _pecas = Consultas.GetPecasPMP(this.Pedidos);
                foreach (var pedido in this.Pedidos)
                {
                    pedido.Set(_pecas);
                }
            }
            return _pecas;
        }
        public List<PEP_PMP> Getpeps()
        {
            if (_peps == null)
            {
                _peps = new List<PEP_PMP>();
                var reais = Consultas.GetPepsReal(this.Pedidos.Select(x => x.pep).Distinct().ToList());
                var cons = Consultas.GetPEPsPGO(this.Pedidos.Select(x => x.pep).Distinct().ToList(), true);
                var lista = reais.Select(x => x.PEP).Distinct().ToList();
                lista.AddRange(cons.Select(x => x.PEP).Distinct().ToList());
                var embs = Consultas.GetResumoEmbarquesPEP(lista, 24);

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var ped in this.Pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(ped.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.PEP == s);
                        var con = cons.Find(x => x.PEP == s);
                        var emb = embs.Find(x => x.PEP == s);
                        if (real != null |  con != null)
                        {
                            _peps.Add(new PEP_PMP(ped, real, null, con, emb));
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
                _subetapas = new List<SubEtapa_PMP>();

                var etp = GetEtapas();
                var reais = this.Pedidos.SelectMany(x => x.Real.GetSubEtapas()).ToList();
                var pedidos = this.Pedidos.Select(x => x.pep).Distinct().ToList();
                var cons = Consultas.GetSubEtapasPGO(pedidos, true);

                var lista = new List<string>();
                lista.AddRange(reais.Select(x => x.PEP).Distinct().ToList());
                lista.AddRange(cons.Select(x => x.PEP).Distinct().ToList());

                var lista_embs = reais.FindAll(x => x.peso_embarcado > 0).Select(x => x.PEP).Distinct().ToList();
                var embs = new List<ZPP0100_Resumo>();
                if (lista_embs.Count > 0)
                {
                    embs = Consultas.GetResumoEmbarquesPEP(lista_embs, 21);
                }

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var pedido in this.Pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(pedido.pep));
                    var etapas = etp.FindAll(x => x.pep.StartsWith(pedido.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.PEP == s);
                        var con = cons.Find(x => x.PEP == s);
                        var emb = embs.Find(x => x.PEP == s);
                        if (real != null |  con != null)
                        {
                            var nnn = new SubEtapa_PMP(pedido, real, null, con, emb);
                            _subetapas.Add(nnn);
                        }
                    }

                    pedido.Set(_subetapas);
                }

            }
            return _subetapas;
        }
        public List<Etapa_PMP> GetEtapas()
        {
            if (_etapas == null)
            {
                _etapas = new List<Etapa_PMP>();
                var reais = Consultas.GetEtapas(this.Pedidos.Select(x => x.pep).Distinct().ToList());
                var cons = DLM.painel.Consultas.GetEtapasPGO(this.Pedidos.Select(x => x.pep).Distinct().ToList(), true);

                var lista = reais.Select(x => x.PEP).Distinct().ToList();
                lista.AddRange(cons.Select(x => x.PEP).Distinct().ToList());

                lista = lista.Distinct().ToList().OrderBy(x => x).ToList();
                foreach (var pedido in this.Pedidos)
                {
                    var etps = lista.FindAll(x => x.StartsWith(pedido.pep));
                    foreach (var s in etps)
                    {
                        var real = reais.Find(x => x.PEP == s);
                        var con = cons.Find(x => x.PEP == s);
                        if (real != null | con != null)
                        {
                            _etapas.Add(new Etapa_PMP(pedido, real, null, con));
                        }
                    }
                    pedido.Set(_etapas);
                }
            }
            return _etapas;
        }
        public Pacote_PMP(List<Pedido_PMP> pedidos)
        {
            this.Pedidos = pedidos;
        }
    }
}
