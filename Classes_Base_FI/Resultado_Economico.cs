using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DLM.sapgui
{

    [Serializable]
    public class Lancamento
    {
        [XmlIgnore]
        [Browsable(false)]
        public string previstostr
        {
            get
            {
                if(previsto<0)
                {
                    return "(" + Math.Abs(previsto).ToString("N0") + ")";
                }
                return previsto !=0 ? previsto.ToString("N0") :"";
            }
        }
        [XmlIgnore]
        [Browsable(false)]
        public string realizadostr
        {
            get
            {
                if (realizado < 0)
                {
                    return "(" + Math.Abs(previsto).ToString("N0") + ")";
                }

                return realizado != 0 ? realizado.ToString("N0") : "";
            }
        }
        [XmlIgnore]
        [Browsable(false)]
        public DateTime datasys
        {
            get
            {
                if(dia>0)
                {
                return new DateTime(ano, mes, dia);

                }
                else
                {
                    return new DateTime(ano, mes, 01);

                }
            }
        }
        [XmlIgnore]
        [Browsable(false)]
        public string data
        {
            get
            {
                return this.ano + "/" + this.mes.ToString().PadLeft(2, '0');
            }
        }
        [Browsable(false)]
        [XmlIgnore]
        public string data_completa
        {
            get
            {
                return this.ano + "/" + this.mes.ToString().PadLeft(2, '0') + "/" + this.dia.ToString().PadLeft(2, '0');
            }
        }
        [Browsable(false)]
        public List<Lancamento> SubLancamentos { get; set; } = new List<Lancamento>();
        public override string ToString()
        {
            return Chave + " - Prev.: " + previsto + " - Real.:" + realizado + " - Desc.:" + descricao;
        }
        [Browsable(false)]
        public string Chave
        {
            get
            {
                return ano + "/" + mes.ToString().PadLeft(2,'0') + " - " + Tipo_Lancamento.ToString();
            }
        }
        [ReadOnly(true)]
        public Tipo_Lancamento Tipo_Lancamento { get; set; } = Tipo_Lancamento._;
        [Browsable(false)]
        public Tipo_Valor Tipo_Valor { get; set; } = Tipo_Valor.Realizado;
        [Category("Data")]
        [DisplayName("Ano")]
        public int ano { get; set; } = 0;
        [Category("Data")]
        [DisplayName("Mês")]
        public int mes { get; set; } = 0;
        [Category("Data")]
        [DisplayName("Dia")]
        public int dia { get; set; } = 0;
        [Category("Valor")]
        [DisplayName("Previsto")]
        public double previsto { get; set; } = 0;
        [Category("Valor")]
        [DisplayName("Realizado")]
        public double realizado { get; set; } = 0;
        [Browsable(false)]
        public double montante_moeda_interna { get; set; } = 0;
        [Browsable(false)]
        public double custo_unitario_previsto
        {
            get
            {
                if (peso_total_previsto == 0) { return 0; }

                return previsto / peso;
            }
        }
        [Browsable(false)]
        public double custo_unitario_realizado
        {
            get
            {
                if (peso_total_realizado == 0) { return 0; }
                return realizado / peso;
            }
        }
        [Browsable(false)]
        public double peso_total_previsto { get; set; } = 0;
        [Browsable(false)]
        public double peso_total_realizado { get; set; } = 0;
        [Browsable(false)]
        public double valor_maximo_previsto { get; set; } = 0;
        [Browsable(false)]
        public double valor_maximo_realizado { get; set; } = 0;
        [Category("Valor")]
        [DisplayName("Peso")]
        public double peso { get; set; } = 0;
        [Browsable(false)]
        public double porcentagem_previsto
        {
            get
            {
                if(peso_total_previsto>0)
                {
                    return peso / peso_total_previsto;
                }
                return 0;
            }
        }
        [Browsable(false)]
        public double porcentagem_realizado
        {
            get
            {
                if (peso_total_realizado > 0)
                {
                    return peso / peso_total_realizado;
                }
                return 0;
            }
        }
        [Category("Valor")]
        [DisplayName("Descrição")]
        public string descricao
        {
            get
            {
                if(_descricao=="" && SubLancamentos.Count>0)
                {
                    _descricao = string.Join(" - ", SubLancamentos.Select(x => x.descricao).Distinct().ToList()).CortarString( 100);
                }

                return _descricao;
            }
            set
            {
                _descricao = value;
            }
        }
        private string _descricao { get; set; } = "";

        public Lancamento(List<Lancamento> sublancamentos)
        {
            this.SubLancamentos = sublancamentos;
            if(this.SubLancamentos.Count>0)
            {
                this.ano = this.SubLancamentos[0].ano;
                this.mes = this.SubLancamentos[0].mes;
                this.Tipo_Lancamento = this.SubLancamentos[0].Tipo_Lancamento;
                this.Tipo_Valor = this.SubLancamentos[0].Tipo_Valor;

                this.descricao = this.SubLancamentos[0].descricao;

                this.peso_total_previsto = this.SubLancamentos.Max(x=>x.peso_total_previsto);
                this.peso_total_realizado = this.SubLancamentos.Max(x=>x.peso_total_realizado);

                this.valor_maximo_previsto = this.SubLancamentos.Max(x=>x.valor_maximo_previsto);
                this.valor_maximo_realizado = this.SubLancamentos.Max(x=>x.valor_maximo_realizado);

                //this.previsto = this.SubLancamentos.Sum(x=>x.previsto);
                //this.realizado = this.SubLancamentos.Sum(x=>x.realizado);
                //this.peso = this.SubLancamentos.Sum(x=>x.peso);

                //this.montante_moeda_interna = this.SubLancamentos.Sum(x=>x.montante_moeda_interna);


                this.previsto = Math.Abs(this.SubLancamentos.Sum(x => x.previsto));
                this.realizado = Math.Abs(this.SubLancamentos.Sum(x => x.realizado));
                this.peso = Math.Abs(this.SubLancamentos.Sum(x => x.peso));

                this.montante_moeda_interna = Math.Abs(this.SubLancamentos.Sum(x => x.montante_moeda_interna));
            }
        }

        public void CalcularPrevistoRealizado()
        {
            if (this.valor_maximo_previsto > 0)
            {
                this.previsto = this.valor_maximo_previsto * this.porcentagem_previsto;

            }
            if (this.valor_maximo_realizado > 0)
            {
                this.realizado = this.valor_maximo_realizado * this.porcentagem_realizado;

            }
        }

        public Lancamento()
        {

        }
        public Lancamento(Lancamento clonar)
        {
            this.ano = clonar.ano;
            this.mes = clonar.mes;
            this.descricao = clonar.descricao;
            this.dia = clonar.dia;
            this.montante_moeda_interna = clonar.montante_moeda_interna;
            this.peso = clonar.peso;
            this.peso_total_previsto = clonar.peso_total_previsto;
            this.peso_total_realizado = clonar.peso_total_realizado;
            this.previsto = clonar.previsto;
            this.realizado = clonar.realizado;
            //this.SubLancamentos = clonar.SubLancamentos;
            this.Tipo_Lancamento = clonar.Tipo_Lancamento;
            this.Tipo_Valor = clonar.Tipo_Valor;
            this.valor_maximo_previsto = clonar.valor_maximo_previsto;
            this.valor_maximo_realizado = clonar.valor_maximo_realizado;

            foreach(var SUB in clonar.SubLancamentos)
            {
                this.SubLancamentos.Add(new Lancamento(SUB));
            }
        }




    }
    [Serializable]
    public class  Receitas
    {
        public void Reset()
        {
            _receita_liquida = null;
            _receita_bruta = null;
        }
        [XmlIgnore]
        private Grupo _receita_bruta { get; set; }
        public Grupo Getreceita_bruta()
        {
            if(_receita_bruta==null)
            {
                Grupo retorno = new Grupo();
                var cor = Brushes.LightGreen.Clone();
                cor.Opacity = 0.8;
                retorno.Cor = cor;
                retorno.linha_previsto = 6;
                retorno.linha_realizado = 7;
                retorno.titulo = "Receita Bruta";
                retorno.contrato = new Lancamento(new List<Lancamento> { receita_bruta_projeto.contrato, receita_bruta_materiais.contrato, receita_bruta_montagem.contrato });

                retorno.faturamento_direto = receita_bruta_projeto.faturamento_direto + receita_bruta_materiais.faturamento_direto + receita_bruta_montagem.faturamento_direto;
                retorno.saving = receita_bruta_projeto.saving + receita_bruta_materiais.saving + receita_bruta_montagem.saving;

                retorno.meses.AddRange(receita_bruta_projeto.meses);
                retorno.meses.AddRange(receita_bruta_materiais.meses);
                retorno.meses.AddRange(receita_bruta_montagem.meses);
                retorno.meses = Funcoes.Agrupar(retorno.meses,false);
                foreach(var m in retorno.meses)
                {
                    m.Tipo_Lancamento = Tipo_Lancamento.Receita_Bruta;
                }
                _receita_bruta = retorno;
            }
            return _receita_bruta;
        }
        public Grupo receita_bruta_projeto { get; set; } = new Grupo() { linha_previsto = 8, linha_realizado = 9 };
        public Grupo receita_bruta_materiais { get; set; } = new Grupo() { linha_previsto = 10, linha_realizado = 11 };
        public Grupo receita_bruta_montagem { get; set; } = new Grupo() { linha_previsto = 12, linha_realizado = 13 };
        public Grupo deducoes { get; set; } = new Grupo() { linha_previsto = 14, linha_realizado = 15 };


        [XmlIgnore]
        private Grupo _receita_liquida { get; set; }
        public Grupo Getreceita_liquida()
        {
            if(_receita_liquida==null)
            {
                var retorno = new Grupo() { linha_previsto = 16, linha_realizado = 17 };
                var cor = Brushes.LightGreen.Clone();
                cor.Opacity = 0.8;
                retorno.Cor = cor;
                retorno.contrato.previsto = Getreceita_bruta().contrato.previsto - deducoes.contrato.previsto;
                retorno.titulo = "Receita Receita líquida";
                foreach (var receita_bruta in Getreceita_bruta().meses)
                {
                    var deducao = deducoes.GetLancamento(receita_bruta.data);

                    var n = new Lancamento();
                    n.ano = receita_bruta.ano;
                    n.mes = receita_bruta.mes;
                    if(receita_bruta.previsto>0)
                    {
                    n.previsto = receita_bruta.previsto - deducao.previsto;
                    }
                    if(receita_bruta.realizado>0)
                    {
                    n.realizado = receita_bruta.realizado - deducao.realizado;
                    }
                    n.Tipo_Lancamento = Tipo_Lancamento.Receita_Liquida;

                    retorno.meses.Add(n);
                    

                }
                _receita_liquida = retorno;
            }
            return _receita_liquida;
        }

        public Receitas()
        {
            var cor = Brushes.LightGreen.Clone();
            cor.Opacity = 0.5;
            var cor2 = Brushes.LightSalmon.Clone();
            cor2.Opacity = 0.5;
            this.deducoes.Cor = cor2;
            this.receita_bruta_materiais.Cor = cor;
            this.receita_bruta_montagem.Cor = cor;
            this.receita_bruta_projeto.Cor = cor;
            
        }
    }
    public class Outros
    {
        public Grupo peso { get; set; } = new Grupo();
        public Grupo custo { get; set; } = new Grupo();
        public Grupo custo_kg { get; set; } = new Grupo();
        public Outros()
        {

        }
        public Outros(int lprev_peso, int lreal_peso, int lPrev_custo, int lreal_custo, int lPrev_custo_kg, int lreal_custo_kg)
        {
            this.peso.linha_previsto = lprev_peso;
            
            this.peso.linha_realizado = lreal_peso;
            this.custo.linha_previsto = lPrev_custo;
            this.custo.linha_realizado = lreal_custo;

            this.custo_kg.linha_previsto = lPrev_custo_kg;
            this.custo_kg.linha_realizado = lreal_custo_kg;
            this.custo_kg.titulo = "kg";


            var cor = Brushes.LightCyan.Clone();
            cor.Opacity = 0.5;
            this.peso.Cor = cor;
            this.custo.Cor = cor;
            this.custo_kg.Cor = cor;
        }
    }

    [Serializable]
    public class Custos
    {
        public void Reset()
        {
            _material = null;
            _montagem = null;
        }
        public Grupo projeto { get; set; } = new Grupo() { linha_previsto = 20, linha_realizado = 21 };
        private Grupo _material { get; set; }
        public Grupo Getmaterial()
        {
           if(_material ==null)
            {
                Grupo retorno = new Grupo() { linha_previsto = 22, linha_realizado = 23 };
                var cor = Brushes.LightSalmon.Clone();
                cor.Opacity = 0.8;
                retorno.Cor = cor;
                retorno.titulo = "Material";
                retorno.contrato = new Lancamento(new List<Lancamento> { mp.contrato, mod.contrato, ggf.contrato, terceiricacao_producao.contrato });
                retorno.faturamento_direto = mp.faturamento_direto + mod.faturamento_direto + ggf.faturamento_direto + terceiricacao_producao.faturamento_direto;
                retorno.saving = mp.saving + mod.saving + ggf.saving + terceiricacao_producao.saving;
                retorno.meses.AddRange(mp.meses);
                retorno.meses.AddRange(mod.meses);
                retorno.meses.AddRange(ggf.meses);
                retorno.meses.AddRange(terceiricacao_producao.meses);
                retorno.meses = Funcoes.Agrupar(retorno.meses,false);
                foreach (var m in retorno.meses)
                {
                    m.Tipo_Lancamento = Tipo_Lancamento.Custos_Material;
                }
                _material = retorno;
            }
            return _material;
        }
        public Grupo mp { get; set; } = new Grupo() { linha_previsto = 24, linha_realizado = 25 };
        public Grupo mod { get; set; } = new Grupo() { linha_previsto = 26, linha_realizado = 27 };
        public Grupo ggf { get; set; } = new Grupo() { linha_previsto = 28, linha_realizado = 29 };
        public Grupo terceiricacao_producao { get; set; } = new Grupo() { linha_previsto = 30, linha_realizado = 31 };
        public Grupo logistica { get; set; } = new Grupo() { linha_previsto = 32, linha_realizado = 33 };
        private Grupo _montagem { get; set; }
        public Grupo Getmontagem()
        {
            if(_montagem ==null)
            {
                Grupo retorno = new Grupo() { linha_previsto = 34, linha_realizado = 35 };
                var cor = Brushes.LightSalmon.Clone();
                cor.Opacity = 0.8;
                retorno.Cor = cor;
                retorno.titulo = "Montagem";
                retorno.contrato = new Lancamento(new List<Lancamento> { mo_di.contrato, equipamentos.contrato, supervisor_medabil.contrato, equipe_propria.contrato });
                retorno.faturamento_direto = mo_di.faturamento_direto + equipamentos.faturamento_direto + supervisor_medabil.faturamento_direto + equipe_propria.faturamento_direto;
                retorno.saving = mo_di.saving + equipamentos.saving + supervisor_medabil.saving + equipe_propria.saving;
                retorno.meses.AddRange(mo_di.meses);
                retorno.meses.AddRange(equipamentos.meses);
                retorno.meses.AddRange(supervisor_medabil.meses);
                retorno.meses.AddRange(equipe_propria.meses);
                retorno.meses = Funcoes.Agrupar(retorno.meses,false);
                foreach (var m in retorno.meses)
                {
                    m.Tipo_Lancamento = Tipo_Lancamento.Custos_Montagem;
                }
                _montagem = retorno;
            }
            return _montagem;
        }
        public Grupo mo_di { get; set; } = new Grupo() { linha_previsto = 36, linha_realizado = 37 };
        public Grupo equipamentos { get; set; } = new Grupo() { linha_previsto = 38, linha_realizado = 39 };
        public Grupo supervisor_medabil { get; set; } = new Grupo() { linha_previsto = 40, linha_realizado = 41 };
        public Grupo equipe_propria { get; set; } = new Grupo() { linha_previsto = 42, linha_realizado = 43 };
        public Grupo seguros { get; set; } = new Grupo() { linha_previsto = 45, linha_realizado = 46 };
        public Grupo terceirizacao_de_projeto { get; set; } = new Grupo() { linha_previsto = 47, linha_realizado = 48 };
        public Grupo demais_custos { get; set; } = new Grupo() { linha_previsto = 49, linha_realizado = 50 };
        public Grupo ebitida { get; set; } = new Grupo() { linha_previsto = 52, linha_realizado = 53 };
        public Custos()
        {
            var cor = Brushes.LightSalmon.Clone();
            cor.Opacity = 0.5;
            this.demais_custos.Cor = cor;
            this.ebitida.Cor = Brushes.LightGray.Clone();
            this.ebitida.Cor.Opacity = 0.5;
            this.equipamentos.Cor = cor;
            this.equipe_propria.Cor = cor;
            this.ggf.Cor = cor;
            this.logistica.Cor = cor;
            this.mod.Cor = cor;
            this.mo_di.Cor = cor;
            this.mp.Cor = cor;
            this.projeto.Cor = cor;
            this.seguros.Cor = cor;
            this.supervisor_medabil.Cor = cor;
            this.terceiricacao_producao.Cor = cor;
            this.terceirizacao_de_projeto.Cor = cor;
          
        }
    }
    [Serializable]
    public class Grupo
    {
        [XmlIgnore]
        [Browsable(false)]
        public Brush Cor { get; set; } = Brushes.LightCyan;
        public double faturamento_direto { get; set; } = 0;
        public double saving { get; set; } = 0;
        public Lancamento GetLancamento(string data)
        {
            var retorno = this.meses.FindAll(x => x.data == data);
            if(retorno.Count>1)
            {
                List<DLM.sapgui.Lancamento> ret = new List<Lancamento>();
                foreach(var t in retorno)
                {
                    if(t.SubLancamentos.Count>0)
                    {
                        ret.AddRange(t.SubLancamentos);
                    }
                    else
                    {
                        ret.Add(t);
                    }
                }
                return new Lancamento(ret);
            }
            if (retorno.Count==1) { return retorno[0]; }
            return new Lancamento();
        }
        public int linha_previsto { get; set; } = 1;
        public int linha_realizado { get; set; } = 1;
        public string titulo { get; set; } = "";
        public Lancamento contrato { get; set; } = new Lancamento();
        public Lancamento acumulado { get; set; } = new Lancamento();

        public Lancamento Gettotal()
        {
            Lancamento pp = new Lancamento();
            pp.descricao = "Total";
            pp.previsto = meses.Sum(x => x.previsto);
            pp.realizado = meses.Sum(x => x.realizado);
            return pp;
        }

        public Lancamento Getsaldo()
        {
            Lancamento pp = new Lancamento();
            pp.descricao = "Saldo";
            pp.previsto = contrato.previsto - Gettotal().previsto;
            pp.realizado = contrato.previsto - Gettotal().realizado;
            return pp;

        }

        public Lancamento Getav_eco()
        {
            Lancamento reto = new Lancamento();
            if (contrato.previsto > 0)
            {
                reto.previsto = Gettotal().previsto / contrato.previsto *100;
            }

            if (contrato.previsto > 0)
            {
                reto.realizado = Gettotal().realizado / contrato.previsto * 100;
            }

            return reto;
        }

        public List<Lancamento> meses { get; set; } = new List<Lancamento>();

        public Grupo()
        {

        }

    }


}
