using Conexoes;
using DLM.db;
using DLM.sap;
using DLM.sapgui;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
    //public class Titulo_Planejamento
    //{
    //    public override string ToString()
    //    {
    //        return CHAVE + " - " +  DESCRICAO;
    //    }
    //    public Linha Linha { get; set; } = new Linha();
    //    public string CHAVE { get; set; } = "";
    //    public string DESCRICAO { get; set; } = "";
    //    public Titulo_Planejamento()
    //    {

    //    }
    //    public Titulo_Planejamento(DLM.db.Linha L, bool orcamento = false)
    //    {
    //        if (!orcamento)
    //        {
    //            this.CHAVE = L.Get("CHAVE").Valor;
    //            this.DESCRICAO = L.Get("DESCRICAO").Valor;
    //        }
    //        else
    //        {
    //            var cont = L.Get("numerocontrato").Valor;
    //            var revisao = L.Get("revisao").Valor;
    //            var descricao = L.Get("descricao").Valor;

    //            this.CHAVE = L.Get("pedido").Valor + ".PGO";
    //            this.DESCRICAO = "[PGO] - [" + cont + "." + revisao + "] " + descricao;
    //        }
    //    }
    //}

    public class Atraso_Planejamento
    {
        public string pep { get; set; } = "";
        public DateTime? data_inicio { get; set; } = Cfg.Init.DataDummy();
        public DateTime? data_fim { get; set; } = Cfg.Init.DataDummy();
        public double previsto { get; set; } = 0;
        public double atual { get; set; } = 0;
        public double peso { get; set; } = 0;
        public Setor setor { get; set; } = Setor.ENGENHARIA;

        public PLAN_BASE fase { get; set; } = new PLAN_BASE();

        public Atraso_Planejamento(PLAN_BASE subetapa, Setor setor)
        {
            this.fase = subetapa;
            this.setor = setor;
            this.peso = this.fase.peso_planejado;
            this.pep = subetapa.PEP;

            switch (setor)
            {
                case Setor.ENGENHARIA:
                    this.data_inicio = this.fase.engenharia_cronograma_inicio;
                    this.data_fim = this.fase.engenharia_cronograma;
                    this.previsto = this.fase.engenharia_previsto;
                    this.atual = this.fase.liberado_engenharia;
                    break;
                case Setor.FÁBRICA:
                    this.data_inicio = this.fase.fabrica_cronograma_inicio;
                    this.data_fim = this.fase.fabrica_cronograma;
                    this.previsto = this.fase.fabrica_previsto;
                    this.atual = this.fase.total_produzido;
                    break;
                case Setor.LOGÍSTICA:
                    this.data_inicio = this.fase.logistica_cronograma_inicio;
                    this.data_fim = this.fase.logistica_cronograma;
                    this.previsto = this.fase.logistica_previsto;
                    this.atual = this.fase.total_embarcado;
                    break;
                case Setor.MONTAGEM:
                    this.data_inicio = this.fase.montagem_cronograma_inicio;
                    this.data_fim = this.fase.montagem_cronograma;
                    this.previsto = this.fase.montagem_previsto;
                    this.atual = this.fase.total_montado;
                    break;
            }
        }
    }

    public class Carga_Planejamento
    {
        public string data { get; set; } = "";
        public double peso
        {
            get
            {
                return Math.Round(this.Pecas_Logistica.Sum(x => x.peso),4);
            }
        }
        private List<PackList_Planejamento> _pack_lists { get; set; }
        public List<PackList_Planejamento> pack_lists
        {
            get
            {
                if(_pack_lists==null)
                {
                    _pack_lists = new List<PackList_Planejamento>();
                    var ss = Pecas_Logistica.Select(x => x.pack_list).Distinct().ToList();
                    foreach (var s in ss)
                    {
                        _pack_lists.Add(new PackList_Planejamento(s, Pecas_Logistica));
                    }
                }
               
                return _pack_lists;
            }
        }
        public override string ToString()
        {
            return num_carga;
        }
        public string num_carga { get; private set; } = "";
        public bool carga_confirmada { get; private set; } = false;
        public string placa { get; set; } = "";
        public string motorista { get; set; } = "";
        public string telefone { get; set; } = "";
        public string observacoes { get; set; } = "";

        public List<Logistica_Planejamento> Pecas_Logistica { get; set; } = new List<Logistica_Planejamento>();
        public Carga_Planejamento(string Carga,List<Logistica_Planejamento> planejamentos)
        {
            this.num_carga = Carga;
            this.Pecas_Logistica = planejamentos.FindAll(x => x.num_carga == Carga);
            this.carga_confirmada = this.Pecas_Logistica.Count == this.Pecas_Logistica.FindAll(x => x.carga_confirmada == true).Count;
            var dts = this.Pecas_Logistica.Select(x => x.data).Distinct().ToList().FindAll(x => x != "01/01/0001" && x !="");
            if(dts.Count>0)
            {
                this.data = dts.Select(x => Conexoes.Extensoes.Data(x)).Max().ToShortDateString();

            }
            if(this.Pecas_Logistica.Count>0)
            {
                this.placa = this.Pecas_Logistica[0].placa;
                this.motorista = this.Pecas_Logistica[0].motorista;
                this.telefone = this.Pecas_Logistica[0].telefone;
                this.observacoes = this.Pecas_Logistica[0].observacoes;
            }
        }
    }

    public class PackList_Planejamento
    {
        public double peso { get; set; } = 0;

        public List<Carga_Planejamento> cargas { get; set; } = new List<Carga_Planejamento>();
        public override string ToString()
        {
            return pack_list;
        }
        public string pack_list { get; private set; } = "";

        public List<Logistica_Planejamento> Pecas_Logistica { get; set; } = new List<Logistica_Planejamento>();
        public PackList_Planejamento(string Carga, List<Logistica_Planejamento> planejamentos)
        {
            this.pack_list = Carga;
            this.Pecas_Logistica = planejamentos.FindAll(x => x.pack_list == Carga);
            this.cargas = new List<Carga_Planejamento>();
            var ss = Pecas_Logistica.Select(x => x.num_carga).Distinct().ToList();
            foreach (var s in ss)
            {
                this.cargas.Add(new Carga_Planejamento(s, Pecas_Logistica));
            }

            this.peso = Math.Round(this.Pecas_Logistica.Sum(x => x.peso), 4);
        }
        public PackList_Planejamento()
        {

        }
    }

    public class ZPP0100_Resumo
    {
        public override string ToString()
        {
            return this.PEP;
        }
        public Linha Linha { get; private set; } = new Linha();
        public string PEP { get; set; } = "";
        public double Necessario { get; set; } = 0;
        public double Embarcado { get; set; } = 0;
        public double Porcentagem_Embarcado
        {
           get
            {
                if(Peso_Necessario > 0)
                {
                    return Peso_Embarcado / Peso_Necessario;
                }
                return 0;
            }
        }
        public double Peso_Necessario { get; set; } = 0;
        public double Peso_Embarcado { get; set; } = 0;
        public ZPP0100_Resumo(db.Linha L)
        {
            this.CopiarVars(L);
        }
        public ZPP0100_Resumo()
        {

        }
    }



    public class Logistica_Planejamento
    {
        public override string ToString()
        {
            return "[" + this.Tipo_Embarque.ToString() + "] - " + this.material + " - " + this.desenho + " - ";
        }
        public string subetapa { get; set; } = "";
        public string data { get; set; } = "";
        public double peso { get; set; } = 0;
        public double qtd_total
        {
            get
            {
                return peca.qtd_necessaria;
            }
        }


        public Tipo_Embarque Tipo_Embarque { get; set; } = Tipo_Embarque.ZPP0066N;
        
        private PLAN_PECA _peca { get; set; } = new PLAN_PECA();
        public PLAN_PECA peca
        {
            get
            {
                return _peca;
            }
            set
            {
                _peca = value;
                if(value!=null)
                {
                this.peso = Math.Round(peca.peso_unitario * this.quantidade, 4);

                }
            }
        }
        public string num_carga { get; private set; } = "RN0";
        public string pep { get; private set; } = "";
        public bool carga_confirmada { get; private set; } = false;
        public string material { get; private set; } = "";
        public string desenho { get; private set; } = "";
        public string pack_list { get; private set; } = "";
        public string nota_fiscal { get; private set; } = "";
        public string etiqueta { get; private set; } = "";
        public bool etiqueta_impressa { get; private set; } = false;
        public string centro { get; private set; } = "";
        public string placa { get; private set; } = "";
        public string motorista { get; private set; } = "";
        public string descricao { get; private set; } = "";
        public string telefone { get; private set; } = "";
        public string observacoes { get; private set; } = "";
        public double quantidade { get; private set; } = 0;
        public double Saldo_1202 { get; private set; } = 0;
        public double Saldo_1203 { get; private set; } = 0;
        public double Saldo_1204 { get; private set; } = 0;
        public Logistica_Planejamento()
        {

        }
        public Logistica_Planejamento(PLAN_PECA peca, DLM.db.Linha l)
        {
            this.peca = peca;
            GetDados(l);

        }
        public Logistica_Planejamento(List<PLAN_PECA> pecas, DLM.db.Linha l, Tipo_Embarque tipo)
        {
            this.Tipo_Embarque = tipo;

            if (tipo == Tipo_Embarque.ZPP0100)
            {
                GetDados_0100(l);

            }
//            if(this.material.Length>0 && this.pep.Length > 0)
//            {
//                this.peca = pecas.Find(x =>
//Conexoes.Utilz.PEP.Get.Subetapa(x.pep, true) == Conexoes.Utilz.PEP.Get.Subetapa(this.pep, true)
//&& x.material == this.material
//);
//            }



        }
        private void GetDados(DLM.db.Linha l)
        {
            this.carga_confirmada = l.Get("carga_confirmada").Boolean();
            this.num_carga = l.Get("num_carga").Valor;
            this.pack_list = l.Get("pack_list").Valor;
            this.pep = l.Get("pep").Valor;
            this.material = l.Get("material").Valor;
            this.desenho = l.Get("desenho").Valor;
            this.nota_fiscal = l.Get("nota_fiscal").Valor;
            this.quantidade = l.Get("quantidade").Double();

            this.subetapa = Conexoes.Utilz.PEP.Get.Subetapa(this.pep,true);
         
        }
        private void GetDados_0100(DLM.db.Linha l)
        {
            this.descricao = l.Get("Descricao").Valor;
            this.carga_confirmada = l.Get("St_Conf_").Valor.ToUpper() == Cfg.Init.ZPP0100_CARGA_CONFIRMADA;
            this.num_carga = "RN" + l.Get("Nro_Carga").Valor/*.PadLeft(5,'0')*/;
            this.pack_list = "PL" + l.Get("Ordem_Embarque").Valor.PadLeft(5, '0');
            this.pep = l.Get("Elemento_PEP").Valor;
            this.material = l.Get("Material").Valor;
            this.desenho = l.Get("Tamanho_dimensao").Valor;
            this.etiqueta = l.Get("etiqueta").Valor;
            this.etiqueta_impressa = l.Get("etiqueta_impressa").Valor.ToUpper() == "TRUE";
            this.centro = l.Get("Centro").Valor;
            this.quantidade = l.Get("Qtd_Embarque").Double();

            this.Saldo_1202 = l.Get("Sld_1202").Double();
            this.Saldo_1203 = l.Get("Sld_1203").Double();
            this.Saldo_1204 = l.Get("Sld_1204").Double();

            this.telefone = l.Get("telefone").Valor;
            this.placa = l.Get("placa").Valor;
            this.motorista = l.Get("motorista").Valor;
            this.observacoes = l["observacoes"].Valor;

            this.subetapa = Conexoes.Utilz.PEP.Get.Subetapa(this.pep, true);

        }
    }

    public class SubEtapa_Logistica_Planejamento
    {
        public string data_min { get; set; } = "";
        public string data_max { get; set; } = "";
        public string subetapa { get; set; } = "";
        public List<Carga_Planejamento> cargas { get; set; } = new List<Carga_Planejamento>();

        public List<Logistica_Planejamento> Pecas_Logistica { get; set; } = new List<Logistica_Planejamento>();
        public SubEtapa_Logistica_Planejamento()
        {

        }
        public SubEtapa_Logistica_Planejamento(string subetapa, List<Logistica_Planejamento> cargas)
        {
            this.subetapa = subetapa;
            this.Pecas_Logistica = cargas.FindAll(x => x.subetapa == subetapa);
            this.cargas = new List<Carga_Planejamento>();
            var ss = Pecas_Logistica.Select(x => x.num_carga).Distinct().ToList();
            foreach (var s in ss)
            {
                this.cargas.Add(new Carga_Planejamento(s, Pecas_Logistica));
            }

            var dts = this.Pecas_Logistica.Select(x => x.data).Distinct().ToList().FindAll(x=>x != "01/01/0001" && x!="");
            if(dts.Count>0)
            {
                var dtss = dts.Select(x => Conexoes.Extensoes.Data(x)).ToList();
                if(dtss.Count>0)
                {
                this.data_max = dtss.Max().ToShortDateString().Replace("01/01/0001","");
                this.data_min = dtss.Min().ToShortDateString().Replace("01/01/0001", "");

                }
            }
        }

    }

    public class Grupo_Mercadoria
    {
        public string descricao_obra { get; set; } = "";
        public DateTime fabrica_cronograma { get; set; } = Cfg.Init.DataDummy();
        public string pep { get; set; } = "";
        public string subetapa
        {
            get
            {
                if (pep.Length >= 21)
                {
                    return pep.Substring(18, 3);
                }
                return "";
            }
        }
        public string etapa
        {
            get
            {
                if (pep.Length >= 21)
                {
                    return pep.Substring(14, 3);
                }
                return "";
            }
        }
        public string contrato
        {
            get
            {
                if (pep.Length >= 12)
                {
                    return pep.Substring(3, 6);
                }
                return "";
            }
        }
        public string pedido
        {
            get
            {
                if (pep.Length >= 12)
                {
                    return pep.Substring(10, 3);
                }
                return "";
            }
        }

        public string pedido_completo { get; set; } = "";
        public double Qtd_Necessaria { get; set; } = 0;
        public double Qtd_Produzida { get; set; } = 0;
        public double Qtd_Embarcada { get; set; } = 0;
        public double Peso_Produzido { get; set; } = 0;
        public double comprimento_total { get; set; } = 0;
        public double Total_Fabricado
        {
            get
            {
                var T = Peso_Produzido / Peso_Total;
                if (T > 1)
                {
                    return 100;
                }
                else if (T < 0)
                {
                    return 0;
                }
                return T * 100;
            }
        }

        public double Total_Embarcado
        {
            get
            {
                var T = Peso_Embarcado / Peso_Total;
                if (T > 1)
                {
                    return 100;
                }
                else if (T < 0)
                {
                    return 0;
                }
                return T * 100;
            }
        }
        public double Peso_Embarcado { get; set; } = 0;
        public override string ToString()
        {
            return descricao + " - " + Peso_Total + " Kg";
        }
        public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();
        public double Peso_Total { get; set; } = 0;
        public string descricao { get; set; } = "";
        public string centro { get; set; } = "";
        public Grupo_Mercadoria(List<PLAN_PECA> pecas)
        {
            this.pecas = pecas;
            if (this.pecas.Count > 0)
            {
                this.descricao = pecas[0].grupo_mercadoria;
                this.centro = pecas[0].centro;
                this.pep = pecas[0].PEP;
                this.pedido_completo = pecas[0].pedido_completo;
            }
            this.Peso_Total = Math.Round(pecas.Sum(x => x.peso_unitario * x.qtd_necessaria), 4);
            this.Peso_Embarcado = Math.Round(pecas.Sum(x => x.peso_embarcado), 4);
            this.Peso_Produzido = Math.Round(pecas.Sum(x => x.peso_produzido), 4);


            this.Qtd_Necessaria = Math.Round(pecas.Sum(x => x.qtd_necessaria), 4);
            this.Qtd_Embarcada = Math.Round(pecas.Sum(x => x.qtd_embarcada), 4);
            this.Qtd_Produzida = Math.Round(pecas.Sum(x => x.qtd_produzida), 4);
            this.comprimento_total = Math.Round(pecas.Sum(x => x.comprimento) / 1000, 4);
            if (this.Qtd_Embarcada < 0) { this.Qtd_Embarcada = 0; }

        }
    }

    public class Resumo_Pecas
    {
        public bool etapa_bloqueada { get; set; } = false;
        public string status_sistema_pep { get; set; } = "";
        public string subetapa
        {
            get
            {
                if (pep.Length >= 21)
                {
                    return pep.Substring(18, 3);
                }
                return "";
            }
        }
        public override string ToString()
        {
            return pep + " - peso: " + peso_necessario + " - qtd: " + qtd_necessaria + " - prod: " + qtd_produzida + " - emb: " + qtd_embarcada;
        }
        public Linha Linha { get; set; } = new Linha();
        public string pep { get; set; } = "";
        public double peso_necessario { get; set; } = 0;
        public double qtd_necessaria { get; set; } = 0;
        public double qtd_produzida { get; set; } = 0;
        public double qtd_embarcada { get; set; } = 0;
        public string status_usuario_pep { get; set; } = "";
        private string centro { get; set; } = "";
        public string centro_producao
        {
            get
            {
                if (_centro_producao != "") { return _centro_producao; }
                return centro;
            }
            set
            {
                _centro_producao = value;
            }
        }
       
        private string _centro_producao { get; set; } = "";
        public int fases { get; set; } = 0;
        public int subfases { get; set; } = 0;

        public DateTime? Inicio { get; set; } = Cfg.Init.DataDummy();
        public DateTime? Fim { get; set; } = Cfg.Init.DataDummy();

        private List<PLAN_PECA> _pecas { get; set; }

        public List<PLAN_PECA> GetPecas()
        {
            if (_pecas == null)
            {
                _pecas = Consultas.GetPecasReal(new List<string> { this.pep });
            }
            return _pecas;
        }

        public Resumo_Pecas(DLM.db.Linha L)
        {
            this.Linha = L;
            this.pep = L.Get("pep").Valor;
            this.peso_necessario = L.Get("peso_necessario").Double();
            this.qtd_necessaria = L.Get("qtd_necessaria").Double();
            this.qtd_produzida = L.Get("qtd_produzida").Double();
            this.qtd_embarcada = L.Get("qtd_embarcada").Double();
            this.fases = L.Get("fases").Int();
            this.subfases = L.Get("subfases").Int();
            this.Inicio = L.Get("inicio").Data();
            this.Fim = L.Get("fim").Data();
            this.status_usuario_pep = L.Get("status_usuario_pep").Valor;
            this.etapa_bloqueada = L.Get("etapa_bloqueada").Boolean();
            this.status_sistema_pep = L.Get("status_sistema_pep").Valor;


            /*23/04/2019*/
            this.centro = L.Get("centro").Valor;
            this.centro_producao = L.Get("centro_producao").Valor;
        }
        public Resumo_Pecas(string pep)
        {
            this.pep = pep;
        }
        public Resumo_Pecas()
        {

        }
        public Resumo_Pecas(List<Resumo_Pecas> lista)
        {
            if (lista.Count == 0) { return; }
            this.fases = lista.Sum(x => x.fases);
            this.Fim = lista.Max(x => x.Fim);
            this.Inicio = lista.Min(x => x.Inicio);
            this.pep = "Resumo";
            this.peso_necessario = lista.Sum(x => x.peso_necessario);
            this.qtd_embarcada = lista.Sum(x => x.qtd_embarcada);
            this.qtd_necessaria = lista.Sum(x => x.qtd_necessaria);
            this.qtd_produzida = lista.Sum(x => x.qtd_produzida);
            this.subfases = lista.Sum(x => x.subfases);

            this.Inicio = lista.Min(x => x.Inicio);
            this.Fim = lista.Max(x => x.Fim);

        }
    }

    public class Resumo_SubEtapa
    {
        public override string ToString()
        {
            return descricao + " - " + Peso_Total + " Kg";
        }
        public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();
        public double Peso_Total { get; set; } = 0;
        public double quantidade { get; set; } = 0;
        public string descricao { get; set; } = "";
        public Resumo_SubEtapa(List<PLAN_PECA> pecas)
        {
            this.pecas = pecas;
            if (this.pecas.Count > 0)
            {
                var ss = DBases.GetPGO().Get_PEP_FERT().Find(x => x.PEP == pecas[0].subetapa && x.FAB == pecas[0].unidade);
                this.descricao = pecas[0].subetapa + "." + pecas[0].unidade;
                if (ss != null)
                {
                    this.descricao = ss.DESC;
                }
            }
            this.Peso_Total = Math.Round(pecas.Sum(x => x.peso_necessario), 2);
            this.quantidade = pecas.Sum(x => x.qtd_necessaria);
        }
    }
    public class Tipos_Pintura
    {
        public override string ToString()
        {
            return Esquema.ToString() + " - " + Superficie + "m²";
        }
        public SAP_ESQ_PIN Esquema { get; set; } = new SAP_ESQ_PIN();
        public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();

        public double Superficie { get; set; } = 0;
        public double comprimento_total { get; set; } = 0;
        public Tipos_Pintura(List<PLAN_PECA> pecas)
        {
            this.pecas = pecas;
            if (this.pecas.Count > 0)
            {
                this.Esquema = pecas[0].Esquema;
            }
            this.comprimento_total = Math.Round(pecas.Sum(x => x.comprimento) / 1000, 4);

            this.Superficie = Math.Round(pecas.Sum(x => x.superficie), 2);
        }
    }
    public class Viga
    {
        public override string ToString()
        {
            return descricao + " - " + peso_total + " Kg";
        }
        public string descricao
        {
            get
            {
                return grupo_mercadoria + " - " + this.complexidade;
            }
        }
        public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();
        public string complexidade { get; set; } = "";
        public string grupo_mercadoria { get; set; } = "";
        public double peso_total { get; set; } = 0;
        public double comprimento_total { get; set; } = 0;
        public double quantidade { get; set; } = 0;
        public int furacoes { get; set; } = 0;
        public Viga(List<PLAN_PECA> pecas)
        {
            this.pecas = pecas;
            this.peso_total = pecas.Sum(x => x.peso_necessario);
            this.comprimento_total = Math.Round(pecas.Sum(x => x.comprimento)/1000,4);
            this.quantidade = pecas.Sum(x => x.qtd_necessaria);
            this.furacoes = pecas.Sum(x => x.furacoes);
            if (pecas.Count > 0)
            {
                this.complexidade = pecas[0].Complexidade;
                this.grupo_mercadoria = pecas[0].grupo_mercadoria;

            }
        }
    }
        public class Materia_Prima
        {
            public override string ToString()
            {
                return "#" + this.espessura + (this.corte > 0 ? (" - " + this.corte) : "") + (this.codigo != "" ? (" - " + this.codigo) : "");
            }
            public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();

            public double peso_total { get; set; } = 0;
            public double comprimento_total { get; set; } = 0;
            public double espessura { get; set; } = 0;
            public int furacoes { get; set; } = 0;
        public double corte { get; set; } = 0;
            public string material { get; set; } = "";
            public string codigo { get; set; } = "";

        public double quantidade { get; set; } = 0;

            public Conexoes.Bobina Bobina { get; set; } = new Conexoes.Bobina();

            public Materia_Prima(List<PLAN_PECA> pecas)
            {
                this.pecas = pecas;
                if (pecas.Count > 0)
                {
                    this.peso_total = pecas.Sum(x => x.peso_necessario);
                    this.comprimento_total = Math.Round(pecas.Sum(x => x.comprimento)/1000,4);
                this.quantidade = pecas.Sum(x => x.qtd_necessaria);
                this.furacoes = pecas.Sum(x => x.furacoes);

                    var cortes = pecas.Select(x => x.corte_largura).Distinct().ToList();
                    var espessuras = pecas.Select(x => x.espessura).Distinct().ToList();
                    var materiais = pecas.Select(x => x.tipo_aco).Distinct().ToList();
                    var codigos = pecas.Select(x => x.codigo_materia_prima_sap).Distinct().ToList();
                    if (cortes.Count == 1)
                    {
                        corte = cortes[0];
                    }
                    if (espessuras.Count == 1)
                    {
                        this.espessura = espessuras[0];
                    }
                    if (materiais.Count == 1)
                    {
                        this.material = materiais[0];
                    }
                    if (codigos.Count == 1)
                    {
                        this.codigo = codigos[0];
                    this.Bobina = DBases.GetBancoRM().GetBobina(this.codigo);
                        if (this.Bobina == null)
                        {
                            this.Bobina = new Conexoes.Bobina();
                        }
                    }
                }
            }

            public Materia_Prima()
            {

            }

        }
        public class Unidade_fabril
        {
            public override string ToString()
            {
                return descricao + " - " + Peso_Total + " Kg";
            }
            public List<PLAN_PECA> pecas { get; set; } = new List<PLAN_PECA>();
            public double Peso_Total { get; set; } = 0;
            public string descricao { get; set; } = "";
            public Unidade_fabril(List<PLAN_PECA> pecas)
            {
                this.pecas = pecas;
                if (this.pecas.Count > 0)
                {
                    this.descricao = pecas[0].centro;
                }
                this.Peso_Total = Math.Round(pecas.Sum(x => x.peso_necessario), 2);
            }
        }


    }

