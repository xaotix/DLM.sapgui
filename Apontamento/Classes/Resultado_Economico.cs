using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Serialization;
using Conexoes;
using DLM.sapgui;
using DLM.vars;

namespace DLM.painel
{
    public class Resultado_Economico_Header
    {
        private ImageSource _imagem { get; set; } = null;
        public ImageSource Imagem
        {
            get
            {
                if (_imagem == null)
                {
                    _imagem = Conexoes.BufferImagem.moedas;

                }
                return _imagem;
            }
        }
        [XmlIgnore]
        public List<Resultado_Economico_Header> SubHeaders { get; set; } = new List<Resultado_Economico_Header>();
        [Browsable(false)]
        public string Contrato
        {
            get
            {
                return Conexoes.Utilz.PEP.Get.Contrato(this.Pedido);
            }
        }
        [ReadOnly(true)]
        [Category("Outros")]
        [DisplayName("Usuário")]
        public string user { get; set; } = "";
        [ReadOnly(true)]
        [Category("Outros")]
        [DisplayName("Última Edição")]
        public DateTime ultima_edicao { get; set; } = Cfg.Init.DataDummy();


        [ReadOnly(true)]
        [Category("Outros")]
        [DisplayName("Criado")]
        public DateTime criado { get; set; } = Cfg.Init.DataDummy();
        [Browsable(false)]
        public bool faltam_dados_preenchidos
        {
            get
            {
             return  (mp == 0 | mod == 0 | ggf == 0 | overhead == 0 | comercial==0) ;
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
        public DLM.db.Linha GetLinha()
        {
            DLM.db.Linha retorno = new DLM.db.Linha();
            retorno.Add("mp", this.mp);
            retorno.Add("mod", this.mod);
            retorno.Add("ggf", this.ggf);
            retorno.Add("terceirizacao_producao", this.terceirizacao_producao);
            retorno.Add("terceirizacao_projeto", this.terceirizacao_projeto);
            retorno.Add("overhead", this.overhead);
            retorno.Add("comercial", this.comercial);
            retorno.Add("suporte_producao", this.suporte_producao);
            retorno.Add("user", DLM.vars.Global.UsuarioAtual);

            return retorno;
        }
        public void Salvar()
        {
            if(this.id>0)
            {
                DBases.GetDB().Update(new List<DLM.db.Celula> { new DLM.db.Celula("id", this.id) }, GetLinha().Celulas, Cfg.Init.db_comum, "resultado_economico_header");
            }
            Update_Resultado_Economico();
        }
        public override string ToString()
        {
            return this.Pedido + " - " + this.descricao + (faltam_dados_preenchidos?" [FALTAM DADOS]":"");
        }

        private Resultado_Economico _Resultado_Economico { get; set; }
        [Browsable(false)]
        public Resultado_Economico GetResultado_Economico()
        {
            if (this._Resultado_Economico == null && this.Pedido.Length>5)
            {
                if(this.SubHeaders.Count>0)
                {
                    foreach (var s in this.SubHeaders)
                    {
                        s.GetResultado_Economico().SetLancamentos();
                    }

                    this._Resultado_Economico = new Resultado_Economico(this.SubHeaders.Select(x => x.GetResultado_Economico()).ToList(), this);
                 
                }
                else
                {
                    var t = DLM.painel.Consultas.GetResultados_Economicos(this.Pedido);
                    if (t.Count > 0)
                    {
                        this._Resultado_Economico = t[0];
                        this._Resultado_Economico.Header = this;
                        Update_Resultado_Economico();
                    }
                }
                
            }
            return this._Resultado_Economico;
        }

        
        private void Update_Resultado_Economico()
        {
            if (this._Resultado_Economico == null) { return; }
            this._Resultado_Economico.Custos.mp.contrato.previsto = mp;
            this._Resultado_Economico.Custos.mod.contrato.previsto = mod;
            this._Resultado_Economico.Custos.ggf.contrato.previsto = ggf;
            this._Resultado_Economico.Custos.terceiricacao_producao.contrato.previsto = terceirizacao_producao;
            this._Resultado_Economico.Custos.terceirizacao_de_projeto.contrato.previsto = terceirizacao_projeto;
            this._Resultado_Economico.Custos.demais_custos.contrato.previsto = demais_custos;
            this._Resultado_Economico.SetLancamentos();
        }

        [Browsable(false)]
        public DLM.db.Linha l { get; set; } = new DLM.db.Linha();
        [Browsable(false)]
        public long id { get; set; } = -1;
        [Browsable(false)]
        public string Pedido { get; set; } = "";
        [Browsable(false)]
        public string descricao { get; set; } = "";
        [Category("Material")]
        [DisplayName("MP")]
        public double mp
        {
            get
            {
                return _mp;
            }
            set
            {
                _mp = value;
                NotifyPropertyChanged("mp");
            }
        }
        private double _mp { get; set; } = 0;
        [Category("Material")]
        [DisplayName("MOD")]
        public double mod
        {
            get
            {
                return _mod;
            }
            set
            {
                _mod = value;
                NotifyPropertyChanged("mod");
            }
        }
        private double _mod { get; set; } = 0;
        [Category("Material")]
        [DisplayName("GGF")]
        public double ggf
        {
            get
            {
                return _ggf;
            }
            set
            {
                _ggf = value;
                NotifyPropertyChanged("ggf");
            }
        }
        private double _ggf { get; set; } = 0;
        [Category("Material")]
        [DisplayName("Terceirização Produção")]
        public double terceirizacao_producao
        {
            get
            {
                return _terceirizacao_producao;
            }
            set
            {
                _terceirizacao_producao = value;
                NotifyPropertyChanged("terceirizacao_producao");
            }
        }
        private double _terceirizacao_producao { get; set; } = 0;
        [Category("Projeto")]
        [DisplayName("Terceirização Projeto")]
        public double terceirizacao_projeto
        {
            get
            {
                return _terceirizacao_projeto;
            }
            set
            {
                _terceirizacao_projeto = value;
                NotifyPropertyChanged("terceirizacao_projeto");
                               
            }
        }
        private double _terceirizacao_projeto { get; set; } = 0;
        [Category("Demais Custos")]
        [DisplayName("Overhead")]
        public double overhead
        {
            get
            {
                return _overhead;
            }
            set
            {
                _overhead = value;
                NotifyPropertyChanged("overhead");
                NotifyPropertyChanged("demais_custos");
            }
        }
        private double _overhead { get; set; } = 0;

        [Category("Demais Custos")]
        [DisplayName("Comercial")]
        public double comercial
        {
            get
            {
                return _comercial;
            }
            set
            {
                _comercial = value;
                NotifyPropertyChanged("comercial");
                NotifyPropertyChanged("demais_custos");
            }
        }
        private double _comercial { get; set; } = 0;


        [Category("Demais Custos")]
        [DisplayName("Total Demais Custos")]
        [Browsable(false)]
        public double demais_custos
        {

            get
            {
                return overhead + suporte_producao + comercial;
            }
        }

        [Category("Demais Custos")]
        [DisplayName("Suporte Produção")]
        public double suporte_producao
        {
            get
            {
                return _suporte_producao;
            }
            set
            {
                _suporte_producao = value;
                NotifyPropertyChanged("suporte_producao");
                NotifyPropertyChanged("demais_custos");
            }
        }
        private double _suporte_producao { get; set; } = 0;

        public Resultado_Economico_Header()
        {

        }
        public Resultado_Economico_Header(DLM.db.Linha l)
        {
            this.l = l;
            this.id = this.l["id"].Int();
            this.Pedido = this.l.Get("pep").Valor;
            this.descricao = this.l.Get("descricao").Valor;

            this.mp = this.l.Get("mp").Double();
            this.mod = this.l.Get("mod").Double();
            this.ggf = this.l.Get("ggf").Double();
            this.terceirizacao_producao = this.l.Get("terceirizacao_producao").Double();
            this.terceirizacao_projeto = this.l.Get("terceirizacao_projeto").Double();

            this.overhead = this.l.Get("overhead").Double();
            this.comercial = this.l.Get("comercial").Double();
            this.suporte_producao = this.l.Get("suporte_producao").Double();

            this.ultima_edicao = this.l["ultima_edicao"].Data();
            this.criado = this.l.Get("criado").Data();
            this.user = this.l.Get("user").Valor;
        }

        public Resultado_Economico_Header(List<Resultado_Economico_Header> j)
        {
            this.comercial = j.Sum(x => x.comercial);
            this.descricao = string.Join(" - ", j.Select(x => x.descricao).Distinct().ToList());
            this.Pedido = string.Join(" - ", j.Select(x => x.Contrato).Distinct().ToList());
            this.ggf = j.Sum(x => x.ggf);
            this.mod = j.Sum(x => x.mod);
            this.mp = j.Sum(x => x.mp);
            this.overhead = j.Sum(x => x.overhead);
            this.suporte_producao = j.Sum(x => x.suporte_producao);
            this.descricao = string.Join(" - ", j.Select(x => x.descricao).Distinct().ToList());
            this.terceirizacao_producao = j.Sum(x => x.terceirizacao_producao);
            this.terceirizacao_projeto = j.Sum(x => x.terceirizacao_projeto);
            this.user = string.Join(" - ", j.Select(x => x.user).Distinct().ToList());

            this.SubHeaders = j;

          

        }
    }
    [Serializable]
    public class Resultado_Economico
    {

        //public List<List<Lancamento>> GetLancamentosDatas()
        //{
        //    ObservableCollection<ObservableCollection<Lancamento>> lancamentos = new ObservableCollection<ObservableCollection<Lancamento>>();
        //    var gr = GetGrupos();
        //    List<Task> Tarefas = new List<Task>();
        //    for (int i = 0; i < gr.Count; i++)
        //    {
        //        ObservableCollection<Lancamento> l0 = new ObservableCollection<Lancamento>();
        //        for (int a = 0; a < datas.Count; a++)
        //        {
        //            var dt = this.datas[a];
        //            var l1 = gr[i];
        //            Tarefas.Add(Task.Factory.StartNew(() =>
        //            {
        //                var l = l1.GetLancamento(dt);
        //                l0.Add(l);
        //            }));
        //        }
        //        lancamentos.Add(l0);
        //    }
        //    Task.WaitAll(Tarefas.ToArray());
        //    Tarefas.Clear();
        //    return lancamentos.Select(x=>x.ToList()).ToList();
        //}

        public int linhas
        {
            get
            {
                return this.GetGrupos().Max(x=>x.linha_realizado);
            }
        }
        public int colunas
        {
            get
            {
                return this.datas.Count + 8;
            }
        }

        public int colunasaldo
        {
            get
            {
                return this.datas.Count + 5+2-1;
            }
        }
        public int colunatotal
        {
            get
            {
                return this.datas.Count + 5 + 1-1;
            }
        }
        public int colunaaveco
        {
            get
            {
                return this.datas.Count + 5 + 3-1;
            }
        }
        [XmlIgnore]
        public DateTime ultima_edicao { get; set; } = Cfg.Init.DataDummy();
        [XmlIgnore]
        public Resultado_Economico_Header Header { get; set; } = new Resultado_Economico_Header();
        public override string ToString()
        {
            return this.Pedido + " - " + this.descricao;
        }
        public string RetornaSerializado()
        {
            return Conexoes.Extensoes.Serializar(this);
        }
        public List<Grupo> GetGrupos()
        {
            List<Grupo> retorno = new List<Grupo>();
            retorno.Add(this.Receitas.Getreceita_bruta());
            retorno.Add(this.Receitas.receita_bruta_projeto);
            retorno.Add(this.Receitas.receita_bruta_materiais);
            retorno.Add(this.Receitas.receita_bruta_montagem);
            retorno.Add(this.Receitas.deducoes);
            retorno.Add(this.Receitas.Getreceita_liquida());
            retorno.Add(this.Custos.projeto);
            retorno.Add(this.Custos.Getmaterial());
            retorno.Add(this.Custos.mp);
            retorno.Add(this.Custos.mod);
            retorno.Add(this.Custos.ggf);
            retorno.Add(this.Custos.terceiricacao_producao);
            retorno.Add(this.Custos.logistica);
            retorno.Add(this.Custos.Getmontagem());
            retorno.Add(this.Custos.mo_di);
            retorno.Add(this.Custos.equipamentos);
            retorno.Add(this.Custos.supervisor_medabil);
            retorno.Add(this.Custos.equipe_propria);
            retorno.Add(this.Custos.seguros);
            retorno.Add(this.Custos.terceirizacao_de_projeto);
            retorno.Add(this.Custos.demais_custos);
            retorno.Add(this.Custos.ebitida);

            retorno.Add(this.Engenharia.peso);
            retorno.Add(this.Engenharia.custo);
            retorno.Add(this.Engenharia.custo_kg);

            retorno.Add(this.Fabricacao.peso);
            retorno.Add(this.Fabricacao.custo);
            retorno.Add(this.Fabricacao.custo_kg);

            retorno.Add(this.Logistica.peso);
            retorno.Add(this.Logistica.custo);
            retorno.Add(this.Logistica.custo_kg);


            return retorno;
        }

        public double peso_total_previsto { get; set; } = 0;
   
        public bool datas_com_problema
        {
            get
            {
                return LancamentosAgrupados.Select(x => x.datasys).Distinct().ToList().OrderBy(x => x).ToList().FindAll(X => X.Year < 2001).Count>0;
            }
        }
        public double GetDeducaoParcial(Lancamento receita_bruta)
        {
            var total_projeto = receita_bruta.SubLancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Receita_Bruta_Projeto).Sum(x => x.realizado);
            var total_mp = receita_bruta.SubLancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Receita_Bruta_Materiais).Sum(x => x.realizado);
            var total_mo = receita_bruta.SubLancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Receita_Bruta_Montagem).Sum(x => x.realizado);

            var deducoes_mp = total_mp * (
                  this.FolhaMargem.impostos.ICMS_Material.porcentagem 
                + this.FolhaMargem.impostos.CPRB_Material.porcentagem 
                + this.FolhaMargem.impostos.IPI_Material.porcentagem 
                + this.FolhaMargem.impostos.PIS_COFINS_Material.porcentagem
                );

            var deducoes_projeto = total_projeto * (
                this.FolhaMargem.impostos.ISS_Projeto.porcentagem 
              + this.FolhaMargem.impostos.PIS_COFINS_Projeto.porcentagem
              );

            var deducoes_equipamentos = total_mo * (
                this.FolhaMargem.impostos.ISS_Locacao_Equipamentos.porcentagem * 
                (
                this.FolhaMargem.custosmontagem.equipamentos.valor / this.FolhaMargem.custosmontagem.total)
                )
                ;
            var deducoes_supervisao = total_mo * (
                this.FolhaMargem.impostos.ISS_Supervisao.porcentagem *
                (
                this.FolhaMargem.custosmontagem.supervisao.valor / this.FolhaMargem.custosmontagem.total)
                )
                ;

            var deducoes_montagem = total_mo * (
                this.FolhaMargem.impostos.ISS_Montagem.porcentagem *
                (
                (this.FolhaMargem.custosmontagem.empreiteiros_despesas.valor) / this.FolhaMargem.custosmontagem.total)
                )
                ;

            var deducoes_montagem_equipe_propria = total_mo * (
                this.FolhaMargem.impostos.ISS_Montagem.porcentagem *
                (
                (this.FolhaMargem.custosmontagem.equipe_propria.valor) / this.FolhaMargem.custosmontagem.total)
                )
                ;

            var deducoes_montagem_pis = total_mo * (
              this.FolhaMargem.impostos.PIS_COFINS_Montagem.porcentagem *
              (
              (this.FolhaMargem.custosmontagem.empreiteiros_despesas.valor) / this.FolhaMargem.custosmontagem.total)
              )
              ;

            var deducoes_montagem_equipe_propria_pis = total_mo * (
                this.FolhaMargem.impostos.PIS_COFINS_Montagem.porcentagem *
                (
                (this.FolhaMargem.custosmontagem.equipe_propria.valor) / this.FolhaMargem.custosmontagem.total)
                )
                ;

            return deducoes_equipamentos + deducoes_montagem + deducoes_mp + deducoes_projeto + deducoes_supervisao + deducoes_montagem_equipe_propria + deducoes_montagem_equipe_propria_pis + deducoes_montagem_pis;
        }
        [XmlIgnore]
        public List<string> datas
        {
            get
            {
                List<string> retorno = new List<string>();
                var lancs = LancamentosAgrupados.FindAll(x=>x.previsto>0 || x.realizado>0).Select(x => x.datasys).Distinct().ToList().OrderBy(x => x).ToList().FindAll(X=>X.Year>2001);
                var min = lancs.Min();
                var max = lancs.Max();

                var meses = Conexoes.Utilz.Calendario.GetMeses(min, max);
                var dt = new DateTime(min.Year, min.Month, 01);
                for (int i = 0; i < meses+1; i++)
                {
                    retorno.Add(dt.Year + "/" + dt.Month.ToString().PadLeft(2, '0'));
                    dt = dt.AddMonths(1);
                }

                return retorno;
            }
        }
        public List<DLM.sapgui.Lancamento> Lancamentos { get; set; } = new List<Lancamento>();
        public List<DLM.sapgui.Lancamento> LancamentosAgrupados
        {
            get
            {
                List<DLM.sapgui.Lancamento> retorno = new List<Lancamento>();
                List<DLM.sapgui.Lancamento> lista = new List<Lancamento>();
                lista.AddRange(Lancamentos);

                var chaves = lista.Select(x => x.Chave).Distinct().ToList();

                foreach(var chave in chaves)
                {
                    var subs = lista.FindAll(x => x.Chave == chave).ToList();
                    retorno.Add(new Lancamento(subs));
                }

                return retorno;
            }
        }
        public void SetLancamentos()
        {
            SetFolhaMargem();
            var lancamentos = this.LancamentosAgrupados;
            this.Custos.equipamentos.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipamentos);
            this.Custos.equipe_propria.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipe_Propria);
            this.Custos.logistica.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Logistica);
            this.Custos.mo_di.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_MO_DI);
            this.Custos.projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Projeto);
            this.Custos.seguros.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Seguros);

            this.Custos.mp.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_MP).Select(X => new Lancamento(X)).ToList();
            this.Custos.mod.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_MOD).Select(X => new Lancamento(X)).ToList();
            this.Custos.ggf.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_GGF).Select(X => new Lancamento(X)).ToList();
            this.Custos.terceiricacao_producao.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_Terceirizacao).Select(X => new Lancamento(X)).ToList();

            this.Engenharia.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Engenharia_Peso).Select(X => new Lancamento(X)).ToList();
            this.Fabricacao.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Fabricação_Peso).Select(X => new Lancamento(X)).ToList();
            this.Logistica.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Logística_Peso).Select(X => new Lancamento(X)).ToList();

            this.Custos.Reset();

            foreach (var p in this.Custos.mp.meses)
            {
                var realizado = this.Custos.Getmaterial().GetLancamento(p.data).realizado;
                p.previsto = this.Custos.mp.contrato.previsto * p.porcentagem_previsto;
                if (realizado == 0)
                {
                    p.realizado = p.montante_moeda_interna * (this.Custos.mp.contrato.previsto / this.Custos.Getmaterial().contrato.previsto);
                    p.Tipo_Valor = Tipo_Valor.Empenhado;
                }
            }
            foreach (var p in this.Custos.mod.meses)
            {
                var realizado = this.Custos.Getmaterial().GetLancamento(p.data).realizado;

                p.previsto = this.Custos.mod.contrato.previsto * p.porcentagem_previsto;
                if (realizado == 0)
                {
                    p.realizado = p.montante_moeda_interna * (this.Custos.mod.contrato.previsto / this.Custos.Getmaterial().contrato.previsto);
                    p.Tipo_Valor = Tipo_Valor.Empenhado;
                }
            }
            foreach (var p in this.Custos.ggf.meses)
            {
                var realizado = this.Custos.Getmaterial().GetLancamento(p.data).realizado;

                p.previsto = this.Custos.ggf.contrato.previsto * p.porcentagem_previsto;
                if (realizado == 0)
                {
                    p.realizado = p.montante_moeda_interna * (this.Custos.ggf.contrato.previsto / this.Custos.Getmaterial().contrato.previsto);
                    p.Tipo_Valor = Tipo_Valor.Empenhado;
                }
            }
            foreach (var p in this.Custos.terceiricacao_producao.meses)
            {
                var realizado = this.Custos.Getmaterial().GetLancamento(p.data).realizado;

                p.previsto = this.Custos.terceiricacao_producao.contrato.previsto * p.porcentagem_previsto;
                if (realizado == 0)
                {
                    p.realizado = p.montante_moeda_interna * (this.Custos.terceiricacao_producao.contrato.previsto / this.Custos.Getmaterial().contrato.previsto);
                    p.Tipo_Valor = Tipo_Valor.Empenhado;
                }
            }

            this.Custos.Reset();

            this.Custos.supervisor_medabil.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Supervisor);
            this.Custos.terceirizacao_de_projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Terceirizacao_Projeto);

            this.Receitas.receita_bruta_materiais.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Materiais);
            this.Receitas.receita_bruta_montagem.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Montagem);
            this.Receitas.receita_bruta_projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Projeto);
            this.Receitas.deducoes.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Deducoes);



            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Receitas.receita_bruta_materiais.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.receitabruta.material.valor - this.FolhaMargem.FaturamentoDireto.receitabrutamateriais - this.FolhaMargem.Saving.receitabrutamateriais);
            }
            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Receitas.receita_bruta_montagem.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.receitabruta.montagem.valor - this.FolhaMargem.FaturamentoDireto.receitabrutamontagem - this.FolhaMargem.Saving.receitabrutamontagem);
            }

            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Receitas.receita_bruta_projeto.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.receitabruta.projeto.valor - this.FolhaMargem.FaturamentoDireto.receitabrutaprojeto - this.FolhaMargem.Saving.receitabrutaprojeto);
            }

            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Custos.mp.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.Header.mp - this.FolhaMargem.FaturamentoDireto.custosmp - this.FolhaMargem.Saving.custosmp);
            }

            foreach (var p in this.Custos.logistica.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.gastoslogisticos.total.valor - this.FolhaMargem.FaturamentoDireto.custoslogistica - this.FolhaMargem.Saving.custoslogistica);
            }

            foreach (var p in this.Custos.mo_di.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.custosmontagem.empreiteiros_despesas.valor - this.FolhaMargem.FaturamentoDireto.custosmontagem_mo_di - this.FolhaMargem.Saving.custosmontagem_mo_di);
            }

            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Custos.equipamentos.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.custosmontagem.equipamentos.valor - this.FolhaMargem.FaturamentoDireto.custosmontagemequipamentos - this.FolhaMargem.Saving.custosmontagemequipamentos);
            }
            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Custos.supervisor_medabil.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.custosmontagem.supervisao.valor - this.FolhaMargem.FaturamentoDireto.custosmontagemsupervisor - this.FolhaMargem.Saving.custosmontagemsupervisor);
            }

            /*recalcula usando o faturamento direto*/
            foreach (var p in this.Custos.equipe_propria.meses)
            {
                p.previsto = p.porcentagem_previsto * (this.FolhaMargem.custosmontagem.equipe_propria.valor - this.FolhaMargem.FaturamentoDireto.custosmontagemequipe_propria - this.FolhaMargem.Saving.custosmontagemequipe_propria);
            }

            this.Receitas.receita_bruta_materiais.faturamento_direto = this.FolhaMargem.FaturamentoDireto.receitabrutamateriais;
            this.Receitas.receita_bruta_montagem.faturamento_direto = this.FolhaMargem.FaturamentoDireto.receitabrutamontagem;
            this.Receitas.receita_bruta_projeto.faturamento_direto = this.FolhaMargem.FaturamentoDireto.receitabrutaprojeto;
            this.Custos.mp.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custosmp;
            this.Custos.logistica.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custoslogistica;
            this.Custos.mo_di.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custosmontagem_mo_di;
            this.Custos.equipamentos.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custosmontagemequipamentos;
            this.Custos.supervisor_medabil.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custosmontagemsupervisor;
            this.Custos.equipe_propria.faturamento_direto = this.FolhaMargem.FaturamentoDireto.custosmontagemequipe_propria;

            this.Receitas.receita_bruta_materiais.saving = this.FolhaMargem.Saving.receitabrutamateriais;
            this.Receitas.receita_bruta_montagem.saving = this.FolhaMargem.Saving.receitabrutamontagem;
            this.Receitas.receita_bruta_projeto.saving = this.FolhaMargem.Saving.receitabrutaprojeto;
            this.Custos.mp.saving = this.FolhaMargem.Saving.custosmp;
            this.Custos.logistica.saving = this.FolhaMargem.Saving.custoslogistica;
            this.Custos.mo_di.saving = this.FolhaMargem.Saving.custosmontagem_mo_di;
            this.Custos.equipamentos.saving = this.FolhaMargem.Saving.custosmontagemequipamentos;
            this.Custos.supervisor_medabil.saving = this.FolhaMargem.Saving.custosmontagemsupervisor;
            this.Custos.equipe_propria.saving = this.FolhaMargem.Saving.custosmontagemequipe_propria;


            this.Receitas.Reset();
            this.Custos.Reset();

            /*cálculo de deduções*/
            List<DLM.sapgui.Lancamento> deducoes_realizado = new List<DLM.sapgui.Lancamento>();
            foreach (var p in this.Receitas.Getreceita_bruta().meses)
            {
                var deducao = this.GetDeducaoParcial(p);
                DLM.sapgui.Lancamento ss = new DLM.sapgui.Lancamento() { mes = p.mes, ano = p.ano, realizado = deducao, Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Deducoes };
                deducoes_realizado.Add(ss);
            }
            deducoes_realizado.AddRange(this.Receitas.deducoes.meses.SelectMany(x => x.SubLancamentos));
            this.Receitas.deducoes.meses = DLM.sapgui.Funcoes.Agrupar(deducoes_realizado);


            /*cálculo de demais custos*/
            List<DLM.sapgui.Lancamento> demais_custos = new List<DLM.sapgui.Lancamento>();
            foreach (var p in this.Custos.Getmaterial().meses)
            {
                var previsto = this.Header.demais_custos * p.porcentagem_previsto / p.SubLancamentos.Count;
                var mp = this.Custos.mp.GetLancamento(p.data);
                var mod = this.Custos.mod.GetLancamento(p.data);
                var ggf = this.Custos.ggf.GetLancamento(p.data);
                var terceirizacao = this.Custos.terceiricacao_producao.GetLancamento(p.data);
                double porcentagem = 0;
                if (Math.Abs(p.realizado) > 0)
                {
                    porcentagem = (mp.realizado + mod.realizado + ggf.realizado + terceirizacao.realizado) / this.Custos.Getmaterial().contrato.previsto;
                }
                var realizado = this.Header.demais_custos * porcentagem;
                DLM.sapgui.Lancamento ss = new DLM.sapgui.Lancamento() { mes = p.mes, ano = p.ano, realizado = realizado, previsto = previsto, Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Demais };
                demais_custos.Add(ss);
            }

            this.Custos.demais_custos.meses = demais_custos;


            this.Custos.ebitida.contrato.previsto = this.Receitas.Getreceita_liquida().contrato.previsto -
                (
                this.Custos.seguros.contrato.previsto +
                this.Custos.projeto.contrato.previsto +
                this.Custos.Getmaterial().contrato.previsto +
                this.Custos.Getmontagem().contrato.previsto +
                this.Custos.terceirizacao_de_projeto.contrato.previsto +
                this.Custos.logistica.contrato.previsto +
                this.Custos.demais_custos.contrato.previsto
                );


            if (this.peso_total_previsto == 0)
            {
                if (this.Custos.projeto.meses.Count > 0)
                {
                    this.peso_total_previsto = this.Custos.projeto.meses[0].peso_total_previsto;
                }
            }

            this.Custos.demais_custos.contrato.peso = this.peso_total_previsto;
            this.Custos.ebitida.contrato.peso = this.peso_total_previsto;
            this.Custos.equipamentos.contrato.peso = this.peso_total_previsto;
            this.Custos.equipe_propria.contrato.peso = this.peso_total_previsto;
            this.Custos.ggf.contrato.peso = this.peso_total_previsto;
            this.Custos.logistica.contrato.peso = this.peso_total_previsto;
            this.Custos.mod.contrato.peso = this.peso_total_previsto;
            this.Custos.mo_di.contrato.peso = this.peso_total_previsto;
            this.Custos.mp.contrato.peso = this.peso_total_previsto;
            this.Custos.projeto.contrato.peso = this.peso_total_previsto;
            this.Custos.seguros.contrato.peso = this.peso_total_previsto;
            this.Custos.supervisor_medabil.contrato.peso = this.peso_total_previsto;
            this.Custos.terceiricacao_producao.contrato.peso = this.peso_total_previsto;
            this.Custos.terceirizacao_de_projeto.contrato.peso = this.peso_total_previsto;

            this.Engenharia.peso.contrato.previsto = peso_total_previsto * 1000;
            this.Fabricacao.peso.contrato.previsto = peso_total_previsto * 1000;
            this.Logistica.peso.contrato.previsto = peso_total_previsto * 1000;

            this.Engenharia.custo_kg.contrato.previsto = (this.Custos.projeto.contrato.previsto - this.Custos.projeto.faturamento_direto - this.Custos.projeto.saving) / (peso_total_previsto * 1000);
            this.Fabricacao.custo_kg.contrato.previsto = (this.Custos.Getmaterial().contrato.previsto - this.Custos.Getmaterial().faturamento_direto - this.Custos.Getmaterial().saving) / (peso_total_previsto * 1000);
            this.Logistica.custo_kg.contrato.previsto = (this.Custos.logistica.contrato.previsto - this.Custos.logistica.faturamento_direto - this.Custos.logistica.saving) / (peso_total_previsto * 1000);

            this.Engenharia.custo.contrato.previsto = this.Custos.projeto.contrato.previsto;
            this.Fabricacao.custo.contrato.previsto = this.Custos.Getmaterial().contrato.previsto;
            this.Logistica.custo.contrato.previsto = this.Custos.logistica.contrato.previsto;

            this.Engenharia.custo.saving = this.Custos.projeto.saving;
            this.Fabricacao.custo.saving = this.Custos.Getmaterial().saving;
            this.Logistica.custo.saving = this.Custos.logistica.saving;

            this.Engenharia.custo.faturamento_direto = this.Custos.projeto.faturamento_direto;
            this.Fabricacao.custo.faturamento_direto = this.Custos.Getmaterial().faturamento_direto;
            this.Logistica.custo.faturamento_direto = this.Custos.logistica.faturamento_direto;

            this.Engenharia.custo_kg.titulo = "kg";
            this.Fabricacao.custo_kg.titulo = "kg";
            this.Logistica.custo_kg.titulo = "kg";

            this.Engenharia.custo.meses.Clear();

            foreach (var t in this.Engenharia.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.previsto = this.Engenharia.custo_kg.contrato.previsto * s.previsto;
                s.realizado = this.Engenharia.custo_kg.contrato.previsto * s.realizado;
                s.Tipo_Lancamento = Tipo_Lancamento.Engenharia_Custo;

                this.Engenharia.custo.meses.Add(s);
            }
            this.Custos.projeto.meses.Clear();
            foreach (var t in this.Engenharia.custo.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.Tipo_Lancamento = Tipo_Lancamento.Custos_Projeto;
                this.Custos.projeto.meses.Add(s);

            }

            this.Fabricacao.custo.meses.Clear();
            foreach (var t in this.Fabricacao.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.previsto = this.Fabricacao.custo_kg.contrato.previsto * s.previsto;
                s.realizado = this.Custos.Getmaterial().GetLancamento(t.data).realizado;
                s.Tipo_Lancamento = Tipo_Lancamento.Fabricação_Custo;

                this.Fabricacao.custo.meses.Add(s);
            }

            this.Logistica.custo.meses.Clear();
            foreach (var t in this.Logistica.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.previsto = this.Logistica.custo_kg.contrato.previsto * s.previsto;
                s.realizado = this.Custos.logistica.GetLancamento(t.data).realizado;
                s.Tipo_Lancamento = Tipo_Lancamento.Logística_Custo;

                this.Logistica.custo.meses.Add(s);
            }


            this.Engenharia.custo_kg.meses.Clear();
            foreach (var t in this.Engenharia.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.Tipo_Lancamento = Tipo_Lancamento.Engenharia_Custo_KG;
                s.previsto = this.Engenharia.custo_kg.contrato.previsto;
                s.realizado = this.Engenharia.custo.GetLancamento(t.data).realizado / this.Engenharia.peso.GetLancamento(t.data).realizado;
                this.Engenharia.custo_kg.meses.Add(s);
            }

            this.Fabricacao.custo_kg.meses.Clear();
            foreach (var t in this.Fabricacao.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.Tipo_Lancamento = Tipo_Lancamento.Fabricação_Custo_KG;

                s.previsto = this.Fabricacao.custo_kg.contrato.previsto;
                s.realizado = this.Fabricacao.custo.GetLancamento(t.data).realizado / this.Fabricacao.peso.GetLancamento(t.data).realizado;
                this.Fabricacao.custo_kg.meses.Add(s);
            }

            this.Logistica.custo_kg.meses.Clear();
            foreach (var t in this.Logistica.peso.meses)
            {
                var s = new DLM.sapgui.Lancamento(t);
                s.previsto = this.Logistica.custo_kg.contrato.previsto;
                s.Tipo_Lancamento = Tipo_Lancamento.Logística_Custo_KG;

                s.realizado = this.Logistica.custo.GetLancamento(t.data).realizado / this.Logistica.peso.GetLancamento(t.data).realizado;
                this.Logistica.custo_kg.meses.Add(s);
            }

            this.Receitas.Reset();
            SetTitulos();
        }

        private void SetTitulos()
        {
            this.Receitas.Getreceita_liquida().titulo = "Receita Líquida";
            this.Receitas.Getreceita_bruta().titulo = "Receita Bruta";
            this.Receitas.deducoes.titulo = "Deduções";
            this.Receitas.receita_bruta_materiais.titulo = "Receita Bruta Materiais";
            this.Receitas.receita_bruta_montagem.titulo = "Receita Bruta Montagem";
            this.Receitas.receita_bruta_projeto.titulo = "Receita Bruta Projeto";

            this.Custos.demais_custos.titulo = "Demais Custos";
            this.Custos.ebitida.titulo = "EBITIDA";
            this.Custos.equipamentos.titulo = "Equipamentos";
            this.Custos.equipe_propria.titulo = "Equipe própria";
            this.Custos.ggf.titulo = "GGF";
            this.Custos.logistica.titulo = "Logística";
            this.Custos.mod.titulo = "MOD";
            this.Custos.mo_di.titulo = "MO + DI";
            this.Custos.mp.titulo = "MP";
            this.Custos.projeto.titulo = "Projeto";
            this.Custos.seguros.titulo = "Seguros";
            this.Custos.supervisor_medabil.titulo = "Supervisor Medabil";
            this.Custos.terceiricacao_producao.titulo = "Terceirização de Produção";
            this.Custos.terceirizacao_de_projeto.titulo = "Terceirização de Projeto";
            this.Custos.Getmaterial().titulo = "Material";
            this.Custos.Getmontagem().titulo = "Montagem";

            this.Engenharia.custo.titulo = "Engenharia Custo";
            this.Engenharia.custo_kg.titulo = "Engenharia Custo / KG";
            this.Engenharia.peso.titulo = "Engenharia Peso";

            this.Fabricacao.custo.titulo = "Fábricação Custo";
            this.Fabricacao.custo_kg.titulo = "Fábricação Custo / KG";
            this.Fabricacao.peso.titulo = "Fábricação Peso";

            this.Logistica.custo.titulo = "Logística Custo";
            this.Logistica.custo_kg.titulo = "Logística Custo / KG";
            this.Logistica.peso.titulo = "Logística Peso";
        }

        public void SetFolhaMargem(DLM.sapgui.FolhaMargem folhamargem)
        {
            //this.FolhaMargem = folhamargem;
            /*folha margem*/
            SetFolhaMargem();
        }
        private void SetFolhaMargem()
        {
            this.Receitas.receita_bruta_projeto.contrato.previsto = this.FolhaMargem.receitabruta.projeto.valor;
            this.Receitas.receita_bruta_materiais.contrato.previsto = this.FolhaMargem.receitabruta.material.valor;
            this.Receitas.receita_bruta_montagem.contrato.previsto = this.FolhaMargem.receitabruta.montagem.valor;
            //this.Receitas.Getreceita_liquida().contrato.previsto = this.FolhaMargem.receitaliquida.total.valor;

            this.Receitas.deducoes.contrato.previsto = this.FolhaMargem.impostos.total_deducoes;


            this.Custos.projeto.contrato.previsto = this.FolhaMargem.total_custo_projeto.valor;

            /*falta o explodido*/
            this.Custos.Getmaterial().contrato.previsto = this.FolhaMargem.custosmateriais.materiais.valor;

            this.Custos.logistica.contrato.previsto = this.FolhaMargem.gastoslogisticos.total.valor;

            this.Custos.mo_di.contrato.previsto = this.FolhaMargem.custosmontagem.empreiteiros_despesas.valor;
            this.Custos.equipamentos.contrato.previsto = this.FolhaMargem.custosmontagem.equipamentos.valor;
            this.Custos.supervisor_medabil.contrato.previsto = this.FolhaMargem.custosmontagem.supervisao.valor;
            this.Custos.equipe_propria.contrato.previsto = this.FolhaMargem.custosmontagem.equipe_propria.valor;

            this.Custos.seguros.contrato.previsto = this.FolhaMargem.despesasgerais.seguro.valor;

        }
        [XmlIgnore]
        public FolhaMargem FolhaMargem
        {
            get
            {
                if(_FolhaMargem==null)
                {
                    _FolhaMargem = Consultas.GetFolhaMargem(this.Pedido);
                }
                return _FolhaMargem;
            }
        }
        private FolhaMargem _FolhaMargem { get; set; }
        [XmlIgnore]
        public List<Pedido_PMP> pedidos { get; set; } = new List<Pedido_PMP>();
        public string Pedido { get; set; } = "";
        public string descricao { get; set; } = "";
        [XmlIgnore]
        public Receitas Receitas = new Receitas();
        [XmlIgnore]
        public Custos Custos = new Custos();
        [XmlIgnore]
        public Outros Engenharia { get; set; } = new Outros(55, 56, 57, 58,59,60);
        [XmlIgnore]
        public Outros Fabricacao { get; set; } = new Outros(62, 63, 64, 65,66,67);
        [XmlIgnore]
        public Outros Logistica { get; set; } = new Outros(69, 70, 71, 72,73,74);
        public Resultado_Economico()
        {

        }

        public Resultado_Economico(List<Resultado_Economico> juntar, Resultado_Economico_Header header)
        {
           
            this.Lancamentos = juntar.SelectMany(x => x.Lancamentos.Select(y=> new Lancamento(y))).ToList();
            this._FolhaMargem = new FolhaMargem(juntar.Select(x => x.FolhaMargem).ToList());
            this.Header = header;
            this.peso_total_previsto = juntar.Sum(x => x.peso_total_previsto);

            this.descricao = string.Join(" - ", juntar.Select(x => x.descricao).Distinct().ToList());
            this.Pedido = string.Join(" - ", juntar.Select(x => x.Pedido).Distinct().ToList());
           
            this.ultima_edicao = juntar.Max(x => x.ultima_edicao);

            var lancamentos = this.Lancamentos;

            this.Custos.equipamentos.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipamentos);
            this.Custos.equipe_propria.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipe_Propria);
            this.Custos.logistica.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Logistica);
            this.Custos.mo_di.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_MO_DI);
            this.Custos.projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Projeto);
            this.Custos.seguros.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Seguros);

            this.Custos.mp.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_MP).Select(X => new Lancamento(X)).ToList();
            this.Custos.mod.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_MOD).Select(X => new Lancamento(X)).ToList();
            this.Custos.ggf.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_GGF).Select(X => new Lancamento(X)).ToList();
            this.Custos.terceiricacao_producao.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Material_Terceirizacao).Select(X => new Lancamento(X)).ToList();

            this.Engenharia.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Engenharia_Peso).Select(X => new Lancamento(X)).ToList();
            this.Fabricacao.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Fabricação_Peso).Select(X => new Lancamento(X)).ToList();
            this.Logistica.peso.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Logística_Peso).Select(X => new Lancamento(X)).ToList();


            this.Engenharia.custo.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Engenharia_Custo).Select(X => new Lancamento(X)).ToList();
            this.Fabricacao.custo.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Fabricação_Custo).Select(X => new Lancamento(X)).ToList();
            this.Logistica.custo.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Logística_Custo).Select(X => new Lancamento(X)).ToList();

            this.Engenharia.custo_kg.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Engenharia_Custo_KG).Select(X => new Lancamento(X)).ToList();
            this.Fabricacao.custo_kg.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Fabricação_Custo_KG).Select(X => new Lancamento(X)).ToList();
            this.Logistica.custo_kg.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == Tipo_Lancamento.Logística_Custo_KG).Select(X => new Lancamento(X)).ToList();

            this.Custos.supervisor_medabil.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Supervisor);
            this.Custos.terceirizacao_de_projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Custos_Terceirizacao_Projeto);

            this.Receitas.receita_bruta_materiais.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Materiais);
            this.Receitas.receita_bruta_montagem.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Montagem);
            this.Receitas.receita_bruta_projeto.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Projeto);
            this.Receitas.deducoes.meses = lancamentos.FindAll(x => x.Tipo_Lancamento == DLM.sapgui.Tipo_Lancamento.Deducoes);


            this.Engenharia.custo.contrato = new Lancamento(juntar.Select(x => x.Engenharia.custo.contrato).ToList());
            this.Engenharia.custo_kg.contrato = new Lancamento(juntar.Select(x => x.Engenharia.custo.contrato).ToList());
            this.Engenharia.peso.contrato = new Lancamento(juntar.Select(x => x.Engenharia.custo.contrato).ToList());

            this.Fabricacao.custo.contrato = new Lancamento(juntar.Select(x => x.Fabricacao.custo.contrato).ToList());
            this.Fabricacao.custo_kg.contrato = new Lancamento(juntar.Select(x => x.Fabricacao.custo.contrato).ToList());
            this.Fabricacao.peso.contrato = new Lancamento(juntar.Select(x => x.Fabricacao.custo.contrato).ToList());

            this.Logistica.custo.contrato = new Lancamento(juntar.Select(x => x.Logistica.custo.contrato).ToList());
            this.Logistica.custo_kg.contrato = new Lancamento(juntar.Select(x => x.Logistica.custo.contrato).ToList());
            this.Logistica.peso.contrato = new Lancamento(juntar.Select(x => x.Logistica.custo.contrato).ToList());


            this.Custos.demais_custos.contrato = new Lancamento(juntar.Select(x => x.Custos.demais_custos.contrato).ToList());
            this.Custos.ebitida.contrato = new Lancamento(juntar.Select(x => x.Custos.ebitida.contrato).ToList());
            this.Custos.equipamentos.contrato = new Lancamento(juntar.Select(x => x.Custos.equipamentos.contrato).ToList());
            this.Custos.equipe_propria.contrato = new Lancamento(juntar.Select(x => x.Custos.equipe_propria.contrato).ToList());
            this.Custos.ggf.contrato = new Lancamento(juntar.Select(x => x.Custos.ggf.contrato).ToList());
            this.Custos.logistica.contrato = new Lancamento(juntar.Select(x => x.Custos.logistica.contrato).ToList());
            this.Custos.mod.contrato = new Lancamento(juntar.Select(x => x.Custos.mod.contrato).ToList());
            this.Custos.mo_di.contrato = new Lancamento(juntar.Select(x => x.Custos.mo_di.contrato).ToList());
            this.Custos.mp.contrato = new Lancamento(juntar.Select(x => x.Custos.mp.contrato).ToList());
            this.Custos.projeto.contrato = new Lancamento(juntar.Select(x => x.Custos.projeto.contrato).ToList());
            this.Custos.seguros.contrato = new Lancamento(juntar.Select(x => x.Custos.seguros.contrato).ToList());
            this.Custos.supervisor_medabil.contrato = new Lancamento(juntar.Select(x => x.Custos.supervisor_medabil.contrato).ToList());
            this.Custos.terceiricacao_producao.contrato = new Lancamento(juntar.Select(x => x.Custos.terceiricacao_producao.contrato).ToList());
            this.Custos.terceirizacao_de_projeto.contrato = new Lancamento(juntar.Select(x => x.Custos.terceirizacao_de_projeto.contrato).ToList());

            this.Receitas.deducoes.contrato = new Lancamento(juntar.Select(x => x.Receitas.deducoes.contrato).ToList());
            this.Receitas.receita_bruta_materiais.contrato = new Lancamento(juntar.Select(x => x.Receitas.receita_bruta_materiais.contrato).ToList());
            this.Receitas.receita_bruta_montagem.contrato = new Lancamento(juntar.Select(x => x.Receitas.receita_bruta_montagem.contrato).ToList());
            this.Receitas.receita_bruta_projeto.contrato = new Lancamento(juntar.Select(x => x.Receitas.receita_bruta_projeto.contrato).ToList());


            this.Custos.demais_custos.faturamento_direto = juntar.Sum(x => x.Custos.demais_custos.faturamento_direto);
            this.Custos.ebitida.faturamento_direto = juntar.Sum(x => x.Custos.ebitida.faturamento_direto);
            this.Custos.equipamentos.faturamento_direto = juntar.Sum(x => x.Custos.equipamentos.faturamento_direto);
            this.Custos.equipe_propria.faturamento_direto = juntar.Sum(x => x.Custos.equipe_propria.faturamento_direto);
            this.Custos.ggf.faturamento_direto = juntar.Sum(x => x.Custos.ggf.faturamento_direto);
            this.Custos.logistica.faturamento_direto = juntar.Sum(x => x.Custos.logistica.faturamento_direto);
            this.Custos.mod.faturamento_direto = juntar.Sum(x => x.Custos.mod.faturamento_direto);
            this.Custos.mo_di.faturamento_direto = juntar.Sum(x => x.Custos.mo_di.faturamento_direto);
            this.Custos.mp.faturamento_direto = juntar.Sum(x => x.Custos.mp.faturamento_direto);
            this.Custos.projeto.faturamento_direto = juntar.Sum(x => x.Custos.projeto.faturamento_direto);
            this.Custos.seguros.faturamento_direto = juntar.Sum(x => x.Custos.seguros.faturamento_direto);
            this.Custos.supervisor_medabil.faturamento_direto = juntar.Sum(x => x.Custos.supervisor_medabil.faturamento_direto);
            this.Custos.terceiricacao_producao.faturamento_direto = juntar.Sum(x => x.Custos.terceiricacao_producao.faturamento_direto);
            this.Custos.terceirizacao_de_projeto.faturamento_direto = juntar.Sum(x => x.Custos.terceirizacao_de_projeto.faturamento_direto);

            this.Receitas.deducoes.faturamento_direto = juntar.Sum(x => x.Receitas.receita_bruta_materiais.faturamento_direto);
            this.Receitas.receita_bruta_materiais.faturamento_direto = juntar.Sum(x => x.Receitas.receita_bruta_materiais.faturamento_direto);
            this.Receitas.receita_bruta_montagem.faturamento_direto = juntar.Sum(x => x.Receitas.receita_bruta_montagem.faturamento_direto);
            this.Receitas.receita_bruta_projeto.faturamento_direto = juntar.Sum(x => x.Receitas.receita_bruta_projeto.faturamento_direto);

            this.Custos.demais_custos.saving = juntar.Sum(x => x.Custos.demais_custos.saving);
            this.Custos.ebitida.saving = juntar.Sum(x => x.Custos.ebitida.saving);
            this.Custos.equipamentos.saving = juntar.Sum(x => x.Custos.equipamentos.saving);
            this.Custos.equipe_propria.saving = juntar.Sum(x => x.Custos.equipe_propria.saving);
            this.Custos.ggf.saving = juntar.Sum(x => x.Custos.ggf.saving);
            this.Custos.logistica.saving = juntar.Sum(x => x.Custos.logistica.saving);
            this.Custos.mod.saving = juntar.Sum(x => x.Custos.mod.saving);
            this.Custos.mo_di.saving = juntar.Sum(x => x.Custos.mo_di.saving);
            this.Custos.mp.saving = juntar.Sum(x => x.Custos.mp.saving);
            this.Custos.projeto.saving = juntar.Sum(x => x.Custos.projeto.saving);
            this.Custos.seguros.saving = juntar.Sum(x => x.Custos.seguros.saving);
            this.Custos.supervisor_medabil.saving = juntar.Sum(x => x.Custos.supervisor_medabil.saving);
            this.Custos.terceiricacao_producao.saving = juntar.Sum(x => x.Custos.terceiricacao_producao.saving);
            this.Custos.terceirizacao_de_projeto.saving = juntar.Sum(x => x.Custos.terceirizacao_de_projeto.saving);

            this.Receitas.deducoes.saving = juntar.Sum(x => x.Receitas.receita_bruta_materiais.saving);
            this.Receitas.receita_bruta_materiais.saving = juntar.Sum(x => x.Receitas.receita_bruta_materiais.saving);
            this.Receitas.receita_bruta_montagem.saving = juntar.Sum(x => x.Receitas.receita_bruta_montagem.saving);
            this.Receitas.receita_bruta_projeto.saving = juntar.Sum(x => x.Receitas.receita_bruta_projeto.saving);

            this.Custos.Reset();
            this.Receitas.Reset();

      

            SetTitulos();


        }
    }
}
