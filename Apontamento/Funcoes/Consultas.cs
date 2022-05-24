using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using Conexoes;
using DLM.sapgui;
using DLM.vars;

namespace DLM.painel
{
    public class Util
    {
        public static string tostr(double previsto, bool porcentagem = false, int decimais = 0)
        {
            if(previsto<0)
            {
                return "(" + previsto.ToString("N" + decimais.ToString()) + (porcentagem ? " %" : "") + ")";
            }
            return previsto != 0 ? previsto.ToString("N" + decimais.ToString()) + (porcentagem?" %":"") : "";
            //return previsto != 0 ? previsto.ToString("##,#", CultureInfo.GetCultureInfo("pt-BR")) + (porcentagem ? " %":"") : "";
        }
    }
    public static class Consultas
    {
        
        public static void Salvar(Resultado_Economico resultado)
        {
            if (resultado.Pedido.Length < 6) { return; }
            DLM.db.Linha l = new DLM.db.Linha();
            DLM.db.Linha l2 = new DLM.db.Linha();
            l.Add("pep", resultado.Pedido);
            l.Add("descricao", resultado.descricao);
            l.Add("xml", resultado.RetornaSerializado());

            l2.Add("pep", resultado.Pedido);
            l2.Add("descricao", resultado.descricao);

            var existe = GetResultados_Economicos_Headers(resultado.Pedido);
            if(existe.Count>0)
            {
                var ped = existe[0];
                var id = ped.id;
                if(id>0)
                {
                    DBases.GetDB().Update(new List<DLM.db.Celula> { new DLM.db.Celula("id", id) }, l.Celulas, Cfg.Init.db_comum, "resultado_economico");
                    //DBases.GetDB().Update(new List<DLM.db.Celula> { new DLM.db.Celula("id", id) }, l2.Celulas, Cfg.Init.db_comum, "resultado_economico_header");
                }
                return;
            }

            DBases.GetDB().Apagar("pep", $"%{resultado.Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico_header, true);
            DBases.GetDB().Apagar("pep", $"%{resultado.Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico, true);



            DBases.GetDB().Cadastro(l.Celulas, Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico);
            DBases.GetDB().Cadastro(l2.Celulas, Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico_header);


        }
        public static void Salvar(Resultado_Economico_Header resultado)
        {
            if(resultado== null) { return; }
            if (resultado.Pedido.Length < 6) { return; }
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", resultado.Pedido);
            l.Add("descricao", resultado.descricao);
            l.Add("xml", resultado.GetResultado_Economico().RetornaSerializado());


      
            if (resultado.id > 0)
            {
                DBases.GetDB().Update(new List<DLM.db.Celula> { new DLM.db.Celula("id", resultado.id) }, l.Celulas, Cfg.Init.db_comum, "resultado_economico");
                DLM.painel.Consultas.Salvar(resultado.GetResultado_Economico().FolhaMargem, resultado.Pedido);
                resultado.Salvar();
            }
        }

        public static List<Pedido_PMP> GetObrasPMP(string pedido, bool consolidacao = true, bool orcamento = true)
        {
            if (DLM.painel.Buffer._Obras_PMP != null)
            {
                return DLM.painel.Buffer._Obras_PMP.FindAll(x => x.pep.Contains(pedido));
            }
            List<Pedido_PMP> retorno = new List<Pedido_PMP>();
            List<PLAN_PEDIDO> reais = Consultas.GetPedidos(new List<string> { pedido });
            List<ORC_PED> cons = new List<ORC_PED>();
            List<ORC_PED> orcs = new List<ORC_PED>();

            if (consolidacao)
            {
                cons = Consultas.GetObrasPGO(pedido, true, false);

            }
            if (orcamento)
            {
                orcs = Consultas.GetObrasPGO(pedido, false, false);
            }

            var contratos = reais.Select(x => x.pedido).Distinct().ToList();
            contratos.AddRange(orcs.Select(x => x.pep).Distinct().ToList());
            contratos.AddRange(cons.Select(x => x.pep).Distinct().ToList());
            contratos = contratos.OrderBy(x => x).Distinct().ToList();
            foreach (var ct in contratos)
            {
                var real = reais.Find(x => x.pedido == ct);
                var orc = orcs.Find(x => x.pep == ct);
                var con = cons.Find(x => x.pep == ct);
                if (real != null | orc != null | con != null)
                {
                    retorno.Add(new Pedido_PMP(real, orc, con));
                }
                else
                {

                }
            }
            return retorno;
        }
        public static Resultado_Economico CalcularResultado_Economico(string Pedido, bool salvar = false)
        {
            if (Pedido.Length < 5) { return new Resultado_Economico(); }

            Resultado_Economico pp = new Resultado_Economico();
            pp.Pedido = Pedido;
            int max = 6;
            var w = Conexoes.Utilz.Wait(max, "Carregando..." + Pedido);
            w.Show();
            w.somaProgresso();
            var folha = GetFolhasMargens(Pedido);
            w.somaProgresso();
            DLM.sapgui.FolhaMargem folhamargem = new DLM.sapgui.FolhaMargem();
            if(folha.Count>0)
            {
                folhamargem = folha[0];
            }
            pp.SetFolhaMargem(folhamargem);

            pp.Lancamentos = GetLancamentos(Pedido);

            w.somaProgresso();
            pp.pedidos = Consultas.GetObrasPMP(Pedido, true,false);
            foreach(var p in pp.pedidos)
            {
                if(p.Material_CONS)
                {
                    //p.mudartipo(Tipo_Material.Consolidado);
                }
                w.somaProgresso();
            }

            if(pp.pedidos.Count>0)
            {
                pp.descricao = pp.pedidos[0].descricao;
            }

            /*esse cara precisa que a folha margem esteja carregada*/
            pp.Lancamentos.AddRange(GetLancamentosEtapas(pp.pedidos, pp));
            w.somaProgresso();
            w.Close();


            
            w.somaProgresso();
            pp.Header = Consultas.GetResultado_Economico_Header(Pedido);
            pp.SetLancamentos();
            w.somaProgresso();

            if(salvar)
            {
                Salvar(pp);
            }
            w.Close();
            return pp;
        }

        public static List<DLM.sapgui.Lancamento> GetLancamentosEtapas(List<Pedido_PMP> pedidos, Resultado_Economico resultado)
        {
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();
            var etapas = pedidos.SelectMany(x => x.Getsubetapas()).OrderBy(X => X.ei).ToList();
            double peso_total_previsto = etapas.Sum(x => x.Consolidada.peso_planejado);

            resultado.peso_total_previsto = peso_total_previsto;
            foreach(var etapa in etapas.ToList())
            {
                DLM.sapgui.Lancamento eng = new DLM.sapgui.Lancamento();
                eng.ano = etapa.ef.Year;
                eng.mes = etapa.ef.Month;
                eng.dia = etapa.ef.Day;
                eng.peso = etapa.Consolidada.peso_planejado;
                eng.peso_total_previsto = peso_total_previsto;
                eng.descricao = etapa.pep;
                eng.Tipo_Valor = DLM.sapgui.Tipo_Valor.Previsto;
                eng.Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Projeto;
                eng.valor_maximo_previsto = resultado.Custos.projeto.contrato.previsto;
                eng.previsto = eng.valor_maximo_previsto * eng.porcentagem_previsto;

                DLM.sapgui.Lancamento fab = new DLM.sapgui.Lancamento();
                fab.ano = etapa.ff.Year;
                fab.mes = etapa.ff.Month;
                fab.dia = etapa.ff.Day;
                fab.peso = etapa.Consolidada.peso_planejado;
                fab.peso_total_previsto = peso_total_previsto;
                fab.descricao = etapa.pep;
                fab.Tipo_Valor = DLM.sapgui.Tipo_Valor.Previsto;
                fab.Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_MP;
                fab.valor_maximo_previsto = resultado.Custos.mp.contrato.previsto;
                fab.previsto = fab.valor_maximo_previsto * fab.porcentagem_previsto;

                DLM.sapgui.Lancamento log = new DLM.sapgui.Lancamento();
                log.ano = etapa.lf.Year;
                log.mes = etapa.lf.Month;
                log.dia = etapa.lf.Month;
                log.peso = etapa.Consolidada.peso_planejado;
                log.peso_total_previsto = peso_total_previsto;
                log.descricao = etapa.pep;
                log.Tipo_Valor = DLM.sapgui.Tipo_Valor.Previsto;
                log.Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Logistica;
                log.valor_maximo_previsto = resultado.Custos.logistica.contrato.previsto;
                log.previsto = log.valor_maximo_previsto * log.porcentagem_previsto;

                DLM.sapgui.Lancamento mon = new DLM.sapgui.Lancamento();
                mon.ano = etapa.mf.Year;
                mon.mes = etapa.mf.Month;
                mon.dia = etapa.mf.Month;

                if (mon.ano < 2005)
                {
                    mon.ano = etapa.lf.Year;
                    mon.mes = etapa.lf.AddDays(4).Month;
                    mon.dia = etapa.lf.AddDays(4).Day;
                }

                mon.peso = etapa.Consolidada.peso_planejado;
                mon.peso_total_previsto = peso_total_previsto;
                mon.descricao = etapa.pep;
                mon.Tipo_Valor = DLM.sapgui.Tipo_Valor.Previsto;
                mon.Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Montagem_MO_DI;
                mon.valor_maximo_previsto = resultado.Custos.mo_di.contrato.previsto;
                mon.previsto = mon.valor_maximo_previsto * mon.porcentagem_previsto;

                if (etapa.Material_CONS)
                {


                #region custos
                /*Custos - Projeto*/
                retorno.Add(eng);

                /*Receitas - Receita Bruta Projeto*/
                retorno.Add(new DLM.sapgui.Lancamento(eng)
                {
                    valor_maximo_previsto = resultado.Receitas.receita_bruta_projeto.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Projeto,
                    previsto = resultado.Receitas.receita_bruta_projeto.contrato.previsto * eng.porcentagem_previsto
                });


                /*Custos - MP*/
                retorno.Add(fab);

                /*Custos - MOD*/
                retorno.Add(new DLM.sapgui.Lancamento(fab)
                {
                    valor_maximo_previsto = resultado.Custos.mod.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_MOD,
                    previsto = resultado.Custos.mod.contrato.previsto * fab.porcentagem_previsto
                });

                /*Custos - GGF*/
                retorno.Add(new DLM.sapgui.Lancamento(fab)
                {
                    valor_maximo_previsto = resultado.Custos.ggf.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_GGF,
                    previsto = resultado.Custos.ggf.contrato.previsto * fab.porcentagem_previsto
                });


                /*Custos - Material Terceirização*/
                retorno.Add(new DLM.sapgui.Lancamento(fab)
                {
                    valor_maximo_previsto = resultado.Custos.terceiricacao_producao.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_Terceirizacao,
                    previsto = resultado.Custos.terceiricacao_producao.contrato.previsto * fab.porcentagem_previsto
                });

                /*Custos - Receita Bruta Materiais*/
                retorno.Add(new DLM.sapgui.Lancamento(fab)
                {
                    valor_maximo_previsto = resultado.Receitas.receita_bruta_materiais.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Materiais,
                    previsto = resultado.Receitas.receita_bruta_materiais.contrato.previsto * fab.porcentagem_previsto
                });

                /*Custos - Logística*/
                retorno.Add(log);


                /*Custos - Seguro*/
                retorno.Add(new DLM.sapgui.Lancamento(log)
                {
                    valor_maximo_previsto = resultado.Custos.seguros.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Seguros,
                    previsto = resultado.Custos.seguros.contrato.previsto * log.porcentagem_previsto
                });

                /*Custos - Deduções*/
                retorno.Add(new DLM.sapgui.Lancamento(log)
                {
                    valor_maximo_previsto = resultado.Receitas.deducoes.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Deducoes,
                    previsto = resultado.Receitas.deducoes.contrato.previsto * log.porcentagem_previsto
                });

                /*Custos - MO + DI*/
                retorno.Add(mon);

                /*Custos - Equipamentos*/
                retorno.Add(new DLM.sapgui.Lancamento(mon)
                {
                    valor_maximo_previsto = resultado.Custos.equipamentos.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipamentos,
                    previsto = resultado.Custos.equipamentos.contrato.previsto * mon.porcentagem_previsto
                });

                /*Custos - Supervisor*/
                retorno.Add(new DLM.sapgui.Lancamento(mon)
                {
                    valor_maximo_previsto = resultado.Custos.supervisor_medabil.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Supervisor,
                    previsto = resultado.Custos.supervisor_medabil.contrato.previsto * mon.porcentagem_previsto
                });

                /*Custos - Equipe Própria*/
                retorno.Add(new DLM.sapgui.Lancamento(mon)
                {
                    valor_maximo_previsto = resultado.Custos.equipe_propria.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipe_Propria,
                    previsto = resultado.Custos.equipe_propria.contrato.previsto * mon.porcentagem_previsto
                });


                /*Custos - Receita Bruta Montagem*/
                retorno.Add(new DLM.sapgui.Lancamento(mon)
                {
                    valor_maximo_previsto = resultado.Receitas.receita_bruta_montagem.contrato.previsto,
                    Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Montagem,
                    previsto = resultado.Receitas.receita_bruta_montagem.contrato.previsto * mon.porcentagem_previsto
                });

                    #endregion

                    #region pesos previstos
                    retorno.Add(new DLM.sapgui.Lancamento(eng)
                    {
                        Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Engenharia_Peso,
                        realizado = 0,
                        previsto = etapa.Consolidada.peso_planejado *1000
                    });

                    retorno.Add(new DLM.sapgui.Lancamento(fab)
                    {
                        Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Fabricação_Peso,
                        realizado = 0,
                        previsto = etapa.Consolidada.peso_planejado * 1000
                    });

                    retorno.Add(new DLM.sapgui.Lancamento(log)
                    {
                        Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Logística_Peso,

                        realizado = 0,
                        previsto = etapa.Consolidada.peso_planejado * 1000
                    });
                    #endregion
                }

                if (etapa.Material_REAL)
                {
                    #region peso
                    /*Pesos - Previsto e Realizado*/
                    var dt_eng = etapa.Real.engenharia_liberacao;
                    //if(dt_eng< new DateTime(2001,01,01))
                    //{
                    //    dt_eng = etapa.Real.data_transsap;
                    //}
                    if (dt_eng < Conexoes.Utilz.Calendario.DataDummy() && etapa.ef<=DateTime.Now)
                    {
                        dt_eng = etapa.ef;
                    }
                    var dt_fab = etapa.Real.resumo_pecas.fim;
                    if (dt_fab < Conexoes.Utilz.Calendario.DataDummy() && etapa.ff <= DateTime.Now)
                    {
                        dt_fab = etapa.ff;
                    }
                    if (dt_fab < Conexoes.Utilz.Calendario.DataDummy() && etapa.fi <= DateTime.Now)
                    {
                        dt_fab = etapa.fi;
                    }
                    if (dt_fab < Conexoes.Utilz.Calendario.DataDummy())
                    {
                        dt_fab = etapa.Real.ultima_edicao;
                    }
                    var dt_emb = dt_fab.AddDays(3);

                    /*se a data de embarque passar o mes, volta para o mes anterior*/
                    if(dt_emb.Month> dt_fab.Month || dt_emb.Month == 1 && dt_fab.Month == 12)
                    {
                        dt_emb = new DateTime(dt_fab.Year, dt_fab.Month, dt_fab.Day);
                    }

                    if (etapa.Real.liberado_engenharia > 0)
                    {
                        retorno.Add(new DLM.sapgui.Lancamento(eng)
                        {
                            dia = dt_eng.Day,
                            mes = dt_eng.Month,
                            ano = dt_eng.Year,
                            Tipo_Valor = DLM.sapgui.Tipo_Valor.Realizado,
                            Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Engenharia_Peso,
                            peso = etapa.Real.peso_planejado,
                            previsto = 0,
                            realizado = etapa.Real.peso_planejado * 1000
                        });
                    }
                if (etapa.Real.total_fabricado > 0)
                {
                        retorno.Add(new DLM.sapgui.Lancamento(fab)
                        {
                            dia = dt_fab.Day,
                            mes = dt_fab.Month,
                            ano = dt_fab.Year,
                            Tipo_Valor = DLM.sapgui.Tipo_Valor.Realizado,
                            Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Fabricação_Peso,
                            previsto = 0,
                            peso = etapa.Real.peso_planejado,
                            realizado = etapa.Real.peso_produzido * 1000

                        });
                }
                    
                if (etapa.Real.total_embarcado > 0)
                {
                        retorno.Add(new DLM.sapgui.Lancamento(log)
                        {
                            dia = dt_emb.Day,
                            mes = dt_emb.Month,
                            ano = dt_emb.Year,
                            Tipo_Valor = DLM.sapgui.Tipo_Valor.Realizado,
                            Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Logística_Peso,
                            peso = etapa.Real.peso_planejado,
                        previsto = 0,
                        realizado = etapa.Real.peso_embarcado * 1000
                        });
                }
                #endregion

                }
            }
            return DLM.sapgui.Funcoes.Agrupar(retorno);
        }
        public static List<DLM.sapgui.Lancamento> GetLancamentos(string Pedido)
        {
            var receitas_brutas_realizadas = GetReceitasBrutasrRealizadas(Pedido);
            var custos_montagem = GetCustosMontagem(Pedido);
            var custos_materiais = GetCustosMaterial(Pedido);
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();
            retorno.AddRange(receitas_brutas_realizadas);
            retorno.AddRange(custos_montagem);
            retorno.AddRange(custos_materiais);
             return retorno;
        }
        public static List<DLM.sapgui.Lancamento> GetReceitasBrutasrRealizadas(string Pedido)
        {
            if (Pedido.Length < 5) { return new List<DLM.sapgui.Lancamento>(); }
            List<DLM.sapgui.Lancamento> lancamentos = new List<DLM.sapgui.Lancamento>();
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();

            /*realizado*/
            var resultado = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_zcontratos_notas_fiscais} as pr where pr.Elemento_PEP like '%{Pedido}% ' and pr.Situacao = 'FATURADA' and pr.Devolucoes = '' and Receita = 'SIM'");
            var fab = resultado.Linhas.FindAll(x => 
             x["Elemento_PEP"].Valor.Contains(".F2")
            |x["Elemento_PEP"].Valor.Contains(".F3")
            |x["Elemento_PEP"].Valor.Contains(".F4")
            |x["Elemento_PEP"].Valor.Contains(".FO")
            ).ToList();
            var eng = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".EN")).ToList();
            var mo = resultado.Linhas.FindAll(x =>
             x["Elemento_PEP"].Valor.Contains(".MO")
            |x["Elemento_PEP"].Valor.Contains(".EQ")
            |x["Elemento_PEP"].Valor.Contains(".SU")
            |x["Elemento_PEP"].Valor.Contains(".EP")
            ).ToList();

            foreach(var f in fab)
            {
                DLM.sapgui.Lancamento l = LancamentoZCONTRATOS(f, DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Materiais);
                lancamentos.Add(l);
            }

            foreach (var f in eng)
            {
                DLM.sapgui.Lancamento l = LancamentoZCONTRATOS(f, DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Projeto);
                lancamentos.Add(l);
            }

            foreach (var f in mo)
            {
                DLM.sapgui.Lancamento l = LancamentoZCONTRATOS(f, DLM.sapgui.Tipo_Lancamento.Receita_Bruta_Montagem);
                lancamentos.Add(l);
            }

            var meses = lancamentos.GroupBy(x => x.Chave).Select(x => x.First()).ToList();

            foreach(var mes in meses)
            {
                var subs = lancamentos.FindAll(x => x.Chave == mes.Chave).ToList();
                retorno.Add(new DLM.sapgui.Lancamento(subs));
            }

            return retorno.OrderBy(x=>x.ToString()).ToList();
        }
        public static List<DLM.sapgui.Lancamento> GetCustosMontagem(string Pedido)
        {
            if (Pedido.Length < 5) { return new List<DLM.sapgui.Lancamento>(); }
            List<DLM.sapgui.Lancamento> lancamentos = new List<DLM.sapgui.Lancamento>();
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();

            /*realizado*/
            /*adicionado filtro para remover itens 31 milhoes*/
            var resultado = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_cji3} as pr where pr.Elemento_PEP like '%{Pedido}% ' and pr.Classe_de_custo not like '31%'");

            var mod_di = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".MO")).ToList();
            var equipamentos = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".EQ")).ToList();
            var supervisor = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".SU")).ToList();
            var equipe_propria = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".EP")).ToList();
            var logistica = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".LOG")).ToList();
            var seguro = resultado.Linhas.FindAll(x => x.Get("Elemento_PEP").ToString().Contains(".SEG")).ToList();
          

            foreach (var f in mod_di)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Montagem_MO_DI);
                lancamentos.Add(l);
            }
            foreach (var f in equipamentos)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipamentos);
                lancamentos.Add(l);
            }
            foreach (var f in supervisor)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Supervisor);
                lancamentos.Add(l);
            }
            foreach (var f in equipe_propria)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Montagem_Equipe_Propria);
                lancamentos.Add(l);
            }
            foreach (var f in logistica)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Logistica);
                lancamentos.Add(l);
            }
            foreach (var f in seguro)
            {
                DLM.sapgui.Lancamento l = LancamentosCJI3(f, DLM.sapgui.Tipo_Lancamento.Custos_Seguros);
                lancamentos.Add(l);
            }


            var meses = lancamentos.GroupBy(x => x.Chave).Select(x => x.First()).ToList();

            foreach (var mes in meses)
            {
                var subs = lancamentos.FindAll(x => x.Chave == mes.Chave).ToList();
                retorno.Add(new DLM.sapgui.Lancamento(subs));
            }

            return retorno.OrderBy(x => x.ToString()).ToList();
        }
        
        public static List<DLM.sapgui.Lancamento> GetCustosMaterial(string Pedido)
        {
            if (Pedido.Length < 5) { return new List<DLM.sapgui.Lancamento>(); }
            List<DLM.sapgui.Lancamento> lancamentos = new List<DLM.sapgui.Lancamento>();
            List<DLM.sapgui.Lancamento> retorno = new List<DLM.sapgui.Lancamento>();

            /*realizado*/
            var resultado = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_fagll03} as pr where pr.Elemento_PEP like '%{Pedido}% '");



            foreach (var l in resultado.Linhas)
            {
               var s = LancamentoFAGLL03(l);
                lancamentos.AddRange(s);
            }



            var meses = lancamentos.GroupBy(x => x.Chave).Select(x => x.First()).ToList();

            foreach (var mes in meses)
            {
                var subs = lancamentos.FindAll(x => x.Chave == mes.Chave).ToList();
                retorno.Add(new DLM.sapgui.Lancamento(subs));
            }

            return retorno.OrderBy(x => x.ToString()).ToList();
        }

        private static List<DLM.sapgui.Lancamento> LancamentoFAGLL03(DLM.db.Linha l)
        {
            var pep = l.Get("Elemento_PEP").ToString();
            var data = l.Get("Data_de_lancamento").Data();
            var BeneficiamentoCO = l.Get("BeneficiamentoCO").Double();
            var GGF_CO = l.Get("GGF_CO").Double();
            var MaterialCO = l.Get("MaterialCO").Double();
            var MOD_CO = l.Get("MOD_CO").Double();
            var SubContratacaoCO = l.Get("SubContratacaoCO").Double();
            var Montante_moeda_interna = l.Get("Montante_em_moeda_interna").Double();

            DLM.sapgui.Lancamento MP = new DLM.sapgui.Lancamento() { Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_MP, descricao = pep, ano = data.Year, mes = data.Month, dia = data.Day };
            DLM.sapgui.Lancamento MOD = new DLM.sapgui.Lancamento() { Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_MOD, descricao = pep, ano = data.Year, mes = data.Month, dia = data.Day }; ;
            DLM.sapgui.Lancamento GGF = new DLM.sapgui.Lancamento() { Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_GGF, descricao = pep, ano = data.Year, mes = data.Month, dia = data.Day }; ;
            DLM.sapgui.Lancamento Terceirizacao = new DLM.sapgui.Lancamento() { Tipo_Lancamento = DLM.sapgui.Tipo_Lancamento.Custos_Material_Terceirizacao, descricao = pep, ano = data.Year, mes = data.Month, dia = data.Day };


            //MP.realizado = Math.Abs(MaterialCO + BeneficiamentoCO);
            //MOD.realizado = Math.Abs(MOD_CO);
            //GGF.realizado = Math.Abs(GGF_CO);
            //Terceirizacao.realizado = Math.Abs(SubContratacaoCO);

            //MP.montante_moeda_interna =  Math.Abs(Montante_moeda_interna);
            //MOD.montante_moeda_interna = Math.Abs(Montante_moeda_interna);
            //GGF.montante_moeda_interna = Math.Abs(Montante_moeda_interna);
            //Terceirizacao.montante_moeda_interna = Math.Abs(Montante_moeda_interna);

            MP.realizado = MaterialCO + BeneficiamentoCO;
            MOD.realizado = MOD_CO;
            GGF.realizado = GGF_CO;
            Terceirizacao.realizado = SubContratacaoCO;

            MP.montante_moeda_interna = Montante_moeda_interna;
            MOD.montante_moeda_interna = Montante_moeda_interna;
            GGF.montante_moeda_interna = Montante_moeda_interna;
            Terceirizacao.montante_moeda_interna = Montante_moeda_interna;

            return new List<DLM.sapgui.Lancamento> { MP, MOD, GGF, Terceirizacao };
        }

        private static DLM.sapgui.Lancamento LancamentoZCONTRATOS(DLM.db.Linha f, DLM.sapgui.Tipo_Lancamento tipo_lancamento)
        {
            DLM.sapgui.Lancamento l = new DLM.sapgui.Lancamento();
            DateTime data = f.Get("Data_emissao").Data();
            l.ano = data.Year;
            l.mes = data.Month;
            l.dia = data.Day;
            l.realizado = f.Get("Valor_total_NF").Double();
            l.descricao = f.Get("Elemento_PEP").ToString();
            l.Tipo_Lancamento = tipo_lancamento;
            return l;
        }
        private static DLM.sapgui.Lancamento LancamentosCJI3(DLM.db.Linha f, DLM.sapgui.Tipo_Lancamento tipo_lancamento)
        {
            DLM.sapgui.Lancamento l = new DLM.sapgui.Lancamento();
            DateTime data = f.Get("Data_de_lancamento").Data();
            l.ano = data.Year;
            l.mes = data.Month;
            l.dia = data.Day;
            l.realizado = f.Get("Valor_moeda_ACC").Double();
            l.descricao = f.Get("Elemento_PEP").ToString();
            l.Tipo_Lancamento = tipo_lancamento;
            return l;
        }

        public static void Salvar(DLM.sapgui.FolhaMargem folha, string pedido)
        {
            if(pedido.Length<5)
            {
                return;
            }
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_folhamargem, true);
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", pedido);

            l.Add("dados", folha.RetornaSerializado());
            DBases.GetDB().Cadastro(l.Celulas, Cfg.Init.db_comum, Cfg.Init.tb_folhamargem);
        }

        public class   Get
        {
            public static DLM.sapgui.FolhaMargem FolhaMargem(string XML)
            {
                var serializer = new XmlSerializer(typeof(DLM.sapgui.FolhaMargem));
                try
                {
                    DLM.sapgui.FolhaMargem result;
                    using (TextReader reader = new StringReader(XML))
                    {

                        result = (DLM.sapgui.FolhaMargem)serializer.Deserialize(reader);
                        return result;
                    }
                }
                catch (Exception )
                {
                    //MessageBox.Show(ex.Message);
                    return null;
                }

            }

            public static Resultado_Economico Resultado_Economico(string XML)
            {
                var serializer = new XmlSerializer(typeof(Resultado_Economico));
                try
                {
                    Resultado_Economico result;
                    using (TextReader reader = new StringReader(XML))
                    {

                        result = (Resultado_Economico)serializer.Deserialize(reader);
                        return result;
                    }
                }
                catch (Exception )
                {
                    //MessageBox.Show(ex.Message);
                    return null;
                }

            }
        }
        public static List<Carga_Planejamento> getCargas(List<Logistica_Planejamento> logs)
        {
            List<Carga_Planejamento> retorno = new List<Carga_Planejamento>();

            foreach(var s in logs.Select(x=>x.num_carga).Distinct().ToList())
            {
                retorno.Add(new Carga_Planejamento(s, logs));
            }

            return retorno;
        }
        public static List<SubEtapa_Logistica_Planejamento> getSubEtapasLogistica(List<Logistica_Planejamento> logs)
        {
            List<SubEtapa_Logistica_Planejamento> retorno = new List<SubEtapa_Logistica_Planejamento>();

            foreach (var s in logs.Select(x => x.subetapa).Distinct().ToList())
            {
                retorno.Add(new SubEtapa_Logistica_Planejamento(s, logs));
            }

            return retorno;
        }
        public static List<PackList_Planejamento> getPackLists(List<Logistica_Planejamento> logs)
        {
            List<PackList_Planejamento> retorno = new List<PackList_Planejamento>();

            foreach (var s in logs.Select(x => x.pack_list).Distinct().ToList())
            {
                retorno.Add(new PackList_Planejamento(s, logs));
            }

            return retorno;
        }
        public static void Apagar_peps(string contrato)
        {
            if(contrato.Length>5)
            {
                DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento, true);
            }
        }
        public static List<ORC_PED> GetObrasPGO( bool consolidadas = false)
        {
            List<ORC_PED> retorno = new List<ORC_PED>();
            string tab = $"{Cfg.Init.db_orcamento}.pmp_orc_resumo";
            if(consolidadas)
            {
                tab = $"{Cfg.Init.db_orcamento}.pmp_orc_resumo_consolidada";
            }
            var s = DBases.GetDBPGO().Consulta($"SELECT * from {tab}");
            foreach(var p in s.Linhas)
            {
                var ped = new ORC_PED(p, consolidadas ? Tipo_Material.Consolidado : Tipo_Material.Orçamento);
                if(!consolidadas)
                {
                    ped.GetDatas();
                }
                if(ped.pep.Length>0)
                {
                retorno.Add(ped);
                }
            }

            if(consolidadas)
            {
                var pacotes = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada_arquivos_peps).Linhas;
               foreach(var pedido in retorno)
                {
                    var pcks = pacotes.FindAll(x => x.Get("pedido").Valor == pedido.pep);
                    var arqs = pcks.Select(x => x.Get("arquivo").Valor).Distinct().ToList();
                    foreach (var arq in arqs)
                    {
                        var peps = pcks.FindAll(x => x.Get("arquivo").Valor == arq).Select(x => x.Get("pep").Valor).Distinct().ToList();
                        pedido.pacotes.Add(new ORC_PCK(arq, peps, pedido));
                    }

                }
            }
            return retorno;
        }
        public static List<ORC_PED> GetObrasPGO(string chave,bool consolidadas = false, bool consolidadas_x_real = false)
        {
            if (chave.Length < 5) { return new List<ORC_PED>(); }
            List<ORC_PED> retorno = new List<ORC_PED>();
            string tab = $"{Cfg.Init.db_orcamento}.pmp_orc_resumo as pr";
            if (consolidadas)
            {
                tab = $"{Cfg.Init.db_orcamento}.pmp_orc_resumo_consolidada as pr";
                if (consolidadas_x_real)
                {
                    tab = $"{Cfg.Init.db_comum}.painel_x_consolidada as pr";
                }
            }
            var s = DBases.GetDBPGO().Consulta($"SELECT * from {tab} where pr.pedido like '%{chave}% '");
            foreach (var p in s.Linhas)
            {
                var ped = new ORC_PED(p, consolidadas ? Tipo_Material.Consolidado : Tipo_Material.Orçamento);
                if (!consolidadas)
                {
                    ped.GetDatas();
                }
                retorno.Add(ped);
            }

            if (consolidadas)
            {
                var pacotes = DBases.GetDBPGO().Consulta($"SELECT * from {Cfg.Init.db_orcamento}.{Cfg.Init.tb_pmp_orc_consolidada_arquivos_peps} where pr.pedido like '%{chave}% '").Linhas;
                foreach (var pedido in retorno)
                {
                    var pcks = pacotes.FindAll(x => x.Get("pedido").Valor == pedido.pep);
                    var arqs = pcks.Select(x => x.Get("arquivo").Valor).Distinct().ToList();
                    foreach (var arq in arqs)
                    {
                        var peps = pcks.FindAll(x => x.Get("arquivo").Valor == arq).Select(x => x.Get("pep").Valor).Distinct().ToList();
                        pedido.pacotes.Add(new ORC_PCK(arq, peps, pedido));
                    }

                }
            }
            return retorno;
        }
        public static List<PLAN_PECA> GetPecasPMP(List<Pedido_PMP> pedidos)
        {
            List<PLAN_PECA> retorno = new List<PLAN_PECA>();
            List<string> reais = pedidos.FindAll(x => x.Material_REAL).Select(x=>x.pep).Distinct().ToList();
            List<string> orcamentos = pedidos.FindAll(x => x.Material_ORC).Select(x => x.pep).Distinct().ToList();
            List<string> consolidadas = pedidos.FindAll(x => x.Material_CONS).Select(x => x.pep).Distinct().ToList();

            var w = Conexoes.Utilz.Wait(reais.Count * 4 + 3, $"Mapeando peças...{pedidos.Count} pedidos...");

            int tam_pacote = 20;
            
            var reais_pecas = GetPecasReal(reais, tam_pacote);
            w.somaProgresso();
            var orc_pecas = GetPecasPGO(orcamentos, tam_pacote);
            w.somaProgresso();
            var cons_pecas = GetPecasPGO(consolidadas, tam_pacote, true);
            w.somaProgresso();
         
            foreach (var real in reais)
            {
                var ped = pedidos.Find(x => x.pep == real);
                if(ped!=null)
                {
                    ped.Real.Set(reais_pecas);
                }
                w.somaProgresso();
            }
            foreach (var orc in orcamentos)
            {
                var ped = pedidos.Find(x => x.pep == orc);
                if (ped != null)
                {
                    ped.Orcamento.Set(orc_pecas);
                }
                w.somaProgresso();
            }
            foreach (var orc in consolidadas)
            {
                var ped = pedidos.Find(x => x.pep == orc);
                if (ped != null)
                {
                    ped.Consolidada.Set(cons_pecas);
                }
                w.somaProgresso();

            }
            retorno.AddRange(reais_pecas);
            retorno.AddRange(cons_pecas);
            retorno.AddRange(orc_pecas);
            retorno = retorno.OrderBy(x => x.pep).ToList();
            foreach (var ped in pedidos)
            {
                ped.Set(retorno);
                w.somaProgresso();
            }
            w.Close();
            return retorno;
        }
        public static List<PLAN_PECA> GetPecasPGO(List<string> pedido, int max_pacote, bool consolidada = false)
        {
            if(pedido.Count==0)
            {
                return new List<PLAN_PECA>();
            }
            List<PLAN_PECA> retorno = new List<PLAN_PECA>();
            var sub_lista = quebrar_lista(pedido, max_pacote);
            var w = Conexoes.Utilz.Wait(sub_lista.Count, "Procurando Peças ..." + (consolidada ? "(2/3 - Consolidadas)" : "(3/3 - Orcamentos)"));
            w.Show();
            foreach (var s in sub_lista)
            {
                retorno.AddRange(GetPecasPGOF(s, consolidada));
                w.somaProgresso();
            }
            w.Close();
            return retorno;
        }
        private static List<PLAN_PECA> GetPecasPGOF(List<string> pedido, bool consolidada = false)
        {
            List<PLAN_PECA> retorno = new List<PLAN_PECA>();

            string tab = "pmp_orc";
            if (consolidada)
            {
                tab = $"{Cfg.Init.tb_pmp_orc_consolidada}";
            }

            string chave = "";
            pedido = pedido.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "");
            if (pedido.Count == 0)
            {
                return new List<PLAN_PECA>();
            }
            for (int i = 0; i < pedido.Count; i++)
            {
                if (i == 0)
                {
                    chave = "prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
                else
                {
                    chave = chave + " or prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
            }

            var s = DBases.GetDBPGO().Consulta($"SELECT * from {Cfg.Init.db_orcamento}.{tab} as prod where {chave}");
            foreach (var ss in s.Linhas)
            {
                PLAN_PECA pc = new PLAN_PECA(ss, true);

                if (consolidada)
                {
                    pc.Tipo = Tipo_Material.Consolidado;
                }
                retorno.Add(pc);
            }
            return retorno;
        }
        public static List<DLM.sapgui.FolhaMargem> GetFolhasMargens(string ChavePesquisa = "")
        {
            List<DLM.sapgui.FolhaMargem> retorno = new List<DLM.sapgui.FolhaMargem>();

            var s = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_folhamargem} as pr where pr.pep like '%{ChavePesquisa}% '");
            foreach (var ss in s.Linhas)
            {
                var xml = ss.Get("dados").ToString();
                var folha = Get.FolhaMargem(xml);
                if(folha!=null)
                {
                    folha.id = ss["id"].Int();
                    retorno.Add(folha);
                }
            }
            
            return retorno;
        }
        public static DLM.sapgui.FolhaMargem GetFolhaMargem(string chavepesquisa)
        {
            var sel = GetFolhasMargens(chavepesquisa);
            if(sel.Count>0)
            {
                return sel[0];
            }
            return new DLM.sapgui.FolhaMargem();
        }
        public static List<Resultado_Economico_Header> GetResultado_Economico_Headers_Obras(string Chavepesquisa)
        {
            var pedidos = GetResultados_Economicos_Headers(Chavepesquisa);
            List<Resultado_Economico_Header> retorno = new List<Resultado_Economico_Header>();
            foreach(var p in pedidos.Select(x=>x.Contrato).Distinct().ToList())
            {
                var peds = pedidos.FindAll(x => x.Contrato == p);
                retorno.Add(new Resultado_Economico_Header(peds));
            }
            return retorno;
        }
        public static Resultado_Economico_Header GetResultado_Economico_Header(string chavepesquisa = "")
        {
            if(chavepesquisa.Length<6)
            {
                return new Resultado_Economico_Header();
            }
            var s = GetResultados_Economicos_Headers(chavepesquisa);
            if(s.Count>0)
            {
                return s[0];
            }
            return new Resultado_Economico_Header();
        }
        public static List<Resultado_Economico_Header> GetResultados_Economicos_Headers(string ChavePesquisa = "")
        {
            List<Resultado_Economico_Header> retorno = new List<Resultado_Economico_Header>();

            var s = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resultado_economico_header} as pr where pr.pep like '%{ChavePesquisa}% '");
            foreach (var ss in s.Linhas)
            {
                retorno.Add(new Resultado_Economico_Header(ss));
            }

            return retorno;
        }
        public static List<Resultado_Economico> GetResultados_Economicos(string ChavePesquisa = "")
        {
            List<Resultado_Economico> retorno = new List<Resultado_Economico>();

            var s = DBases.GetDB().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resultado_economico} as pr where pr.pep like '%{ChavePesquisa}% '");
            foreach (var ss in s.Linhas)
            {
                var xml = ss.Get("xml").ToString();
                var resultado = Get.Resultado_Economico(xml);
                if (resultado != null)
                {
                    resultado.ultima_edicao = ss["ultima_edicao"].Data();
                    retorno.Add(resultado);
                }
            }

            return retorno;
        }
        public static List<ORC_ETP> GetEtapasPGO(List<string> pedidos, bool consolidada = false)
        {

            string chave_pedidos = "";
            List<ORC_ETP> retorno = new List<ORC_ETP>();
            string tab = "pmp_orc_etapas";
            if (consolidada)
            {
                tab = "pmp_orc_etapas_consolidada";
            }


            foreach (var pedido in pedidos)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_orcamento}.{tab} as pr where ") + $" pr.pep like '%{pedido}%'";
            }



            var s = DBases.GetDBPGO().Consulta(chave_pedidos);
            foreach (var ss in s.Linhas)
            {
                ORC_ETP pc = new ORC_ETP(ss);
                if(!consolidada)
                {
                    pc.GetDatas();
                }
                retorno.Add(pc);
            }
            if (consolidada)
            {
                foreach (var ss in retorno)
                {
                    ss.tipo = Tipo_Material.Consolidado;
                }
            }
            return retorno;
        }
        public static List<ORC_SUB> GetSubEtapasPGO(List<string> pedidos, bool consolidada = false)
        {
            List<ORC_SUB> retorno = new List<ORC_SUB>();
            string tab = "pmp_orc_sub_etapas";
            if (consolidada)
            {
                tab = "pmp_orc_sub_etapas_consolidada";
            }
            string chave_pedidos = "";
            foreach (var pedido in pedidos)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_orcamento}.{tab} as pr where ") + $" pr.pep like '%{pedido}%'";
            }


            var s = DBases.GetDBPGO().Consulta(chave_pedidos);
            foreach (var ss in s.Linhas)
            {
                ORC_SUB pc = new ORC_SUB(ss);
                if (!consolidada)
                {
                    pc.GetDatas();
                }
                retorno.Add(pc);
            }
            if (consolidada)
            {
                foreach (var ss in retorno)
                {
                    ss.tipo = Tipo_Material.Consolidado;
                }
            }
            return retorno;
        }
        public static List<ORC_PEP> GetPEPsPGO(List<string> pedidos, bool consolidada = false)
        {

            List<ORC_PEP> retorno = new List<ORC_PEP>();
            string tab = "pmp_orc_peps";
            if(consolidada)
            {
                tab = "pmp_orc_peps_consolidada";
            }


            string chave_pedidos = "";
            foreach (var pedido in pedidos)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_orcamento}.{tab} as pr where ") + $" pr.pep like '%{pedido}%'";
            }


            var s = DBases.GetDBPGO().Consulta(chave_pedidos);



            foreach (var ss in s.Linhas)
            {
                ORC_PEP pc = new ORC_PEP(ss);
                if(!consolidada)
                {
                    pc.GetDatas();
                }
                retorno.Add(pc);
            }

            if(consolidada)
            {
                foreach(var ss in retorno)
                {
                    ss.tipo = Tipo_Material.Consolidado;
                }
            }

            return retorno;
        }


        public static void ApagarObra(string contrato_ou_pedido)
        {
            contrato_ou_pedido = contrato_ou_pedido.Replace("*", "").Replace(" ","").Replace("%","");

            if (contrato_ou_pedido.Length < 5)
            {
                return;
            }
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0066n_logistica, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn, true);
            DBases.GetDB().Apagar("CHAVE", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_titulos_planejamento, true);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zcontratos_notas_fiscais, true);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_folhamargem, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico_header, true);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_cji3, true);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_fagll03, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_avanco_pecas, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n, true);

            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_datas, true);




            ApagarCopiaViews(contrato_ou_pedido);

            BufferObrasPesquisa.Clear();
        }
 
        public static void ApagarCopiaViews(string contrato_ou_pedido)
        {
            if(contrato_ou_pedido.Length<5)
            {
                return;
            }
            DBases.GetDB().Apagar("pedido_principal", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pmp_orc_datas, true);
            DBases.GetDB().Apagar("pedido", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pedidos_planejamento_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento_m_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_obra_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pedido_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_fabrica_copia, true);
            DBases.GetDB().Apagar("pep", $"%{contrato_ou_pedido}%", Cfg.Init.db_painel_de_obras2, "pecas", true);
        
        }

        public static List<StatusSAP_Planejamento> GetStatus(List<string> descricoes)
        {
            List<string> fim = new List<string>();
            foreach(var desc in descricoes)
            {
                fim.AddRange(desc.ToUpper().Split(' ').ToList().FindAll(x => x != "").Distinct().ToList());
            }
            fim = fim.Distinct().ToList().OrderBy(x=>x).ToList();
            List<StatusSAP_Planejamento> retorno = new List<StatusSAP_Planejamento>();
            foreach (var ss in fim)
            {
                var tt = Buffer.GetStatus().Find(x => x.status == ss);
                if (tt != null)
                {
                    retorno.Add(tt);
                }

            }
            return retorno;
        }

        public static List<Meta> GetMeta(List<PLAN_SUB_ETAPA> lista, Range_Meta range = Range_Meta.Mes, Tipo_Meta Tipo = Tipo_Meta.Tudo, Tipo_Filtro_Meta Filtro = Tipo_Filtro_Meta.Etapa)
        {
            List<Meta> retorno = new List<Meta>();
            DateTime min_sistema = Conexoes.Utilz.Calendario.DataDummy();


            if (Tipo == Tipo_Meta.Engenharia)
            {
                var datas = lista.Select(x => x.engenharia_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList();

                datas.AddRange(lista.Select(x => x.engenharia_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
                if (datas.Count > 0)
                {
                    DateTime inicio = datas.Min();
                    DateTime fim = datas.Max();
                    fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
                    DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
                    f0 = GetRanges(lista, range, Tipo,Filtro, retorno, fim, f0);

                }


            }
            else if (Tipo == Tipo_Meta.Fabrica)
            {
                var datas = lista.Select(x => x.resumo_pecas.fim).ToList().FindAll(x => x > min_sistema).ToList();
                datas.AddRange(lista.Select(x => x.resumo_pecas.inicio).ToList().FindAll(x => x > min_sistema).ToList());
                datas.AddRange(lista.Select(x => x.fabrica_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList());
                datas.AddRange(lista.Select(x => x.fabrica_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
                if (datas.Count > 0)
                {
                    DateTime inicio = datas.Min();
                    DateTime fim = datas.Max();
                    fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
                    DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
                    f0 = GetRanges(lista, range, Tipo,Filtro, retorno, fim, f0);

                }
            }


            return retorno.FindAll(x=>x.SubEtapas.Count>0).ToList();
        }
        private static DateTime GetRanges(List<PLAN_SUB_ETAPA> lista, Range_Meta range, Tipo_Meta Tipo, Tipo_Filtro_Meta Filtro, List<Meta> retorno, DateTime fim, DateTime f0)
        {
            if (range == Range_Meta.Mes)
            {
                while (f0 < fim)
                {

                    DateTime f0_fim = new DateTime(f0.Year, f0.Month, DateTime.DaysInMonth(f0.Day, f0.Month));
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo,Filtro);
                    if (mm.Metas.Count > 0)
                    {
                        retorno.Add(mm);
                    }
                    f0 = f0.AddMonths(1);
                }
            }
            else if (range == Range_Meta.Semana)
            {
                while (f0 < fim)
                {

                    DateTime f0_fim = f0.AddDays(7);
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo,Filtro);
                    if (mm.Metas.Count > 0)
                    {
                        retorno.Add(mm);
                    }
                    f0 = f0.AddDays(7);
                }
            }

            else if (range == Range_Meta.Ano)
            {
                while (f0 < fim)
                {

                    DateTime f0_fim = f0.AddYears(1);
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo,Filtro);
                    if (mm.Metas.Count > 0)
                    {
                        retorno.Add(mm);
                    }
                    f0 = f0.AddYears(1);
                }
            }

            return f0;
        }
        public static int Imagens(int id)
        {
            string pasta = @"\\10.51.0.69\medabil\Medabil_GCM\ArqsMontagem\$id$\Imagens\".Replace("$id$", id.ToString());
            if(!Directory.Exists(pasta))
            {
                return 0;
            }
            DirectoryInfo robotImageDir = new DirectoryInfo(pasta);

            return robotImageDir.GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList().FindAll(x => x.Extension.ToUpper() == ".JPG" | x.Extension.ToUpper() == ".JPEG" | x.Extension.ToUpper() == ".PNG").Count;
        }
        public static void setImagens(ListBox lista, int id)
        {
            lista.Items.Clear();
            if (id <= 0) { return; };
            List<BitmapImage> retorno = new List<BitmapImage>();
            string pasta = @"\\10.51.0.69\medabil\Medabil_GCM\ArqsMontagem\$id$\Imagens\".Replace("$id$", id.ToString());
            if (!Directory.Exists(pasta))
            {
                return;
            }
            DirectoryInfo robotImageDir = new DirectoryInfo(pasta);

            var imagens = robotImageDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            if(imagens.Count()>0)
            {
                lista.Visibility = Visibility.Visible;
            }
              
            foreach (FileInfo robotImageFile in imagens.ToList().FindAll(x=>x.Extension.ToUpper() == ".JPG"| x.Extension.ToUpper() == ".JPEG"| x.Extension.ToUpper() == ".PNG"))
            {

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(() =>
                            {
                                try
                                {
                                    Uri uri = new Uri(robotImageFile.FullName);
                                    var bmp = new BitmapImage(uri);
                                  
                                    lista.Items.Add(bmp);
                                }
                                catch (Exception)
                                {
                            
                            
                                }
                            }
                            ));

            }
        }
        public static List<PLAN_PECA> getResumo_Grupo_Mercadoria(DateTime ini, DateTime fim)
        {

            string consulta1 = "select " +
                $"concat('00-000000.P00.000.',substr(pr.pep,-6,6)) as pep," +
                $"sum(pr.peso_necessario) as peso_necessario,sum(pr.peso_produzido) as peso_produzido,sum(pr.qtd_necessaria) as qtd_necessaria, pr.grupo_mercadoria as grupo_mercadoria" +
                $" from {Cfg.Init.db_comum}.zpmp_producao as pr where pr.material not like '300%' " +
                $"and str_to_date(pr.fim_engenharia_base, '%d/%m/%Y') >=str_to_date('$ini$', '%d/%m/%Y') " +
                $"and str_to_date(pr.fim_engenharia_base, '%d/%m/%Y') <=str_to_date('$fim$', '%d/%m/%Y') group by pr.grupo_mercadoria, substr(pr.pep,-6,6)";
            consulta1 = consulta1.Replace("$ini$", ini.ToShortDateString()).Replace("$fim$", fim.ToShortDateString());

            var lista_fab = DBases.GetDB().Consulta(consulta1);
            ConcurrentBag<PLAN_PECA> retorno = new ConcurrentBag<PLAN_PECA>();
            List<Task> Tarefas = new List<Task>();
            foreach (var s in lista_fab.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new PLAN_PECA(s,new List<DLM.db.Linha>()))));
            }
            Task.WaitAll(Tarefas.ToArray());

            return retorno.OrderBy(x => x.pep + "-" + x.grupo_mercadoria).ToList().FindAll(x=>x.peso_necessario>0);
        }
        public static List<Resumo_Pecas> _resumo_pecas_obras { get; set; }
        private static List<Resumo_Pecas> _resumo_pecas_pedido { get; set; }
        private static List<Resumo_Pecas> _resumo_pecas_peps { get; set; }
        private static List<Resumo_Pecas> _resumo_pecas_subetapas { get; set; }
        private static List<PLAN_OBRA> _obras { get; set; }
        private static List<PLAN_PEDIDO> _pedidos { get; set; }
        private static List<Titulo_Planejamento> _titulos_etapas { get; set; }
        private static List<Titulo_Planejamento> _titulos_subetapas { get; set; }
        private static List<Titulo_Planejamento> _titulos_pedidos { get; set; }
        private static List<Titulo_Planejamento> _titulos_obras { get; set; }


        public static List<Titulo_Planejamento> GetTitulosObras()
        {
            if (_titulos_obras == null)
            {
                _titulos_obras = new List<Titulo_Planejamento>();

                var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_titulos_obras);
                ConcurrentBag<Titulo_Planejamento> retorno = new ConcurrentBag<Titulo_Planejamento>();
                List<Task> Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new Titulo_Planejamento(s))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());

                _titulos_obras = retorno.OrderBy(x => x.CHAVE).ToList();
            }
            return _titulos_obras;
        }
        public static List<Titulo_Planejamento> GetTitulosPedidos()
        {
            if (_titulos_pedidos == null)
            {
                _titulos_pedidos = new List<Titulo_Planejamento>();

                var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_titulos_pedidos);
                ConcurrentBag<Titulo_Planejamento> retorno = new ConcurrentBag<Titulo_Planejamento>();
                List<Task> Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new Titulo_Planejamento(s))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());

                _titulos_pedidos = retorno.OrderBy(x => x.CHAVE).ToList();
            }
            return _titulos_pedidos;
        }
        public static List<Titulo_Planejamento> GetTitulosEtapas()
        {
            if (_titulos_etapas == null)
            {
                _titulos_etapas = new List<Titulo_Planejamento>();

                var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_titulos_etapas);
                ConcurrentBag<Titulo_Planejamento> retorno = new ConcurrentBag<Titulo_Planejamento>();
                List<Task> Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new Titulo_Planejamento(s))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());

                _titulos_etapas = retorno.OrderBy(x => x.CHAVE).ToList();
            }
            return _titulos_etapas;
        }
        public static List<Titulo_Planejamento> GetTitulosSubEtapas()
        {
            if (_titulos_subetapas == null)
            {
                _titulos_subetapas = new List<Titulo_Planejamento>();

                var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_titulos_pedidos);
                ConcurrentBag<Titulo_Planejamento> retorno = new ConcurrentBag<Titulo_Planejamento>();
                List<Task> Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new Titulo_Planejamento(s))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());

                _titulos_subetapas = retorno.OrderBy(x => x.CHAVE).ToList();
            }
            return _titulos_subetapas;
        }
        public static List<Resumo_Pecas> getresumo_pecas_obras()
        {
           if(_resumo_pecas_obras == null)
            {
                DLM.db.Tabela lista_fab = new DLM.db.Tabela();
                lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_obra_copia);

                ConcurrentBag<Resumo_Pecas> retorno = new ConcurrentBag<Resumo_Pecas>();
                List<Task> Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Resumo_Pecas(s))));
                }
                Task.WaitAll(Tarefas.ToArray());

                _resumo_pecas_obras = retorno.OrderBy(x => x.pep).ToList();
            }

            return _resumo_pecas_obras;
        }
        public static List<Resumo_Pecas> getresumo_pecas_pedidos()
        {
            if(_resumo_pecas_pedido==null)
            {
                try
                {
                    _resumo_pecas_pedido = new List<Resumo_Pecas>();
                    var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pedido_copia);
                    foreach (var t in lista_fab.Linhas)
                    {
                        _resumo_pecas_pedido.Add(new Resumo_Pecas(t));
                    }
                }
                catch (Exception)
                {


                }

            }
            return _resumo_pecas_pedido;
        }
        public static List<Resumo_Pecas> getresumo_pecas_peps()
        {
            if (_resumo_pecas_peps == null)
            {
                _resumo_pecas_peps = new List<Resumo_Pecas>();
                var lista_fab = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_fabrica_copia);
                ConcurrentBag<Resumo_Pecas> retorno = new ConcurrentBag<Resumo_Pecas>();
                foreach (var t in DLM.painel.Consultas.quebrar_lista(lista_fab.Linhas, 300))
                {
                    List<Task> Tarefas = new List<Task>();
                    foreach (var s in t)
                    {
                        Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Resumo_Pecas(s))));
                    }
                    Task.WaitAll(Tarefas.ToArray());
                    Tarefas.Clear();
                }
                _resumo_pecas_peps.AddRange(retorno);
            }
            return _resumo_pecas_peps;
        }
        public static List<Resumo_Pecas> getresumo_pecas_subetapas()
        {
            if(_resumo_pecas_subetapas==null)
            {
                _resumo_pecas_subetapas = new List<Resumo_Pecas>();
                var lista_fab = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_copia);
                ConcurrentBag <Resumo_Pecas> retorno = new ConcurrentBag<Resumo_Pecas>();
                foreach (var t in DLM.painel.Consultas.quebrar_lista(lista_fab.Linhas, 300))
                {
                    List<Task> Tarefas = new List<Task>();
                    foreach (var s in t)
                    {
                        Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Resumo_Pecas(s))));
                    }
                    Task.WaitAll(Tarefas.ToArray());
                    Tarefas.Clear();
                }
                _resumo_pecas_subetapas.AddRange(retorno);
            }
            return _resumo_pecas_subetapas;
        }

        public static List<Resumo_Pecas> getresumo_pecas_subetapas(List<string> pedidos)
        {


            var ret = new List<Resumo_Pecas>();
            foreach(var ped in pedidos)
            {
                ret.AddRange(getresumo_pecas_subetapas().FindAll(x => x.pep.Contains(ped.Replace("%", ""))));
            }

            return ret;
        }
        public static List<Resumo_Pecas> getresumo_pecas_peps(List<string> pedidos)
        {


            var ret = new List<Resumo_Pecas>();
            foreach (var ped in pedidos)
            {
                ret.AddRange(getresumo_pecas_peps().FindAll(x => x.pep.Contains(ped.Replace("%", ""))));
            }

            return ret;

        }



        public static List<PLAN_OBRA> GetObras(bool copia, bool reset)
        {

            if(_obras==null | reset)
            {
                _obras = new List<PLAN_OBRA>();
                var consulta = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, (copia ? Cfg.Init.tb_obras_planejamento_copia :Cfg.Init.tb_obras_planejamento));
                List<Task> Tarefas = new List<Task>();

                ConcurrentBag<PLAN_OBRA> lista = new ConcurrentBag<PLAN_OBRA>();

                foreach (var t in consulta.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => lista.Add(new PLAN_OBRA(t))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();


                var st_base = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_04_obra);
                var titulos = GetTitulosObras();
                var lista_resumos = Consultas.getresumo_pecas_obras();

                foreach (var t in lista)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        var t0 = lista_resumos.Find(x => x.pep == t.pep);
                        if (t0 != null)
                        {
                            t.resumo_pecas = t0;
                        }
                        t.Set(titulos, true);

                        db.Tabela igual = st_base.Filtrar("pep", t.pep, true);
                        if (igual.Count > 0)
                        {
                            t.SetBase(igual.Linhas.First());
                        }

                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();              


                _obras.AddRange(lista.ToList());
            }
            return _obras;
        }
        public static void CopiarViewParaDbase(string contrato, bool embarques = true)
        {
            contrato = contrato.Replace(" ", "").Replace("*", "");
            if (contrato.Length < 4) { return; }


            DLM.db.Tabela peps = new DLM.db.Tabela();
            DLM.db.Tabela pedidos = new DLM.db.Tabela();
            DLM.db.Tabela contratos = new DLM.db.Tabela();
            DLM.db.Tabela resumo_pecas_obra = new DLM.db.Tabela();
            DLM.db.Tabela resumo_pecas_pedido = new DLM.db.Tabela();
            DLM.db.Tabela resumo_pecas_pep = new DLM.db.Tabela();
            DLM.db.Tabela resumo_pecas_pep_fabrica = new DLM.db.Tabela();
            List<Task> Tarefas = new List<Task>();
            Tarefas.Add(Task.Factory.StartNew(() => peps = DBases.GetDB().Clonar().Consulta($"call {Cfg.Init.db_comum}.getpeps('{contrato}')")));
            Tarefas.Add(Task.Factory.StartNew(() => pedidos = DBases.GetDB().Clonar().Consulta($"call {Cfg.Init.db_comum}.getpedidos({contrato})")));
            Tarefas.Add(Task.Factory.StartNew(() => contratos = DBases.GetDB().Clonar().Consulta($"call {Cfg.Init.db_comum}.getobras({contrato})")));
            Tarefas.Add(Task.Factory.StartNew(() => resumo_pecas_obra = DBases.GetDB().Clonar().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resumo_pecas_obra} as pr where pr.pep like '%{contrato}%'")));
            Tarefas.Add(Task.Factory.StartNew(() => resumo_pecas_pedido = DBases.GetDB().Clonar().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resumo_pecas_pedido} as pr where pr.pep like '%{contrato}%'")));
            Tarefas.Add(Task.Factory.StartNew(() => resumo_pecas_pep = DBases.GetDB().Clonar().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resumo_pecas_pep} as pr where pr.pep like '%{contrato}%'")));
            Tarefas.Add(Task.Factory.StartNew(() => resumo_pecas_pep_fabrica = DBases.GetDB().Clonar().Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_resumo_pecas_pep_fabrica} as pr where pr.pep like '%{contrato}%'")));
            Task.WaitAll(Tarefas.ToArray());

            Consultas.ApagarCopiaViews(contrato);

            DBases.GetDB().Cadastro(peps.Linhas,Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento_m_copia);
            DBases.GetDB().Cadastro(contratos.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_obras_planejamento_copia);
            DBases.GetDB().Cadastro(pedidos.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_pedidos_planejamento_copia);
            DBases.GetDB().Cadastro(resumo_pecas_obra.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_obra_copia);
            DBases.GetDB().Cadastro(resumo_pecas_pedido.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pedido_copia);
            DBases.GetDB().Cadastro(resumo_pecas_pep.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_copia);
            DBases.GetDB().Cadastro(resumo_pecas_pep_fabrica.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_resumo_pecas_pep_fabrica_copia);

            if (embarques)
            {
                var pcs = Consultas.GetPecasReal(new List<string> { contrato });
                DLM.painel.Relatorios.ExportarEmbarque(pcs, false, null, null, true, false);
            }
        }
        public static List<PLAN_OBRA> GetObras(List<string> contrato, bool copia, bool reset)
        {
            contrato = contrato.Distinct().ToList().FindAll(x => x.Length > 0);
            if (contrato.Count == 0)
            {
                return GetObras(copia, reset);
            }

            List<PLAN_OBRA> retorno = new List<PLAN_OBRA>();
            foreach (var s in contrato)
            {
                retorno.AddRange(GetObras(copia,reset).FindAll(x => x.pep.Contains(s)));
            }
            retorno = retorno.GroupBy(x => x.pep).Select(x => x.First()).ToList();

            return retorno;
        }
        public static List<PLAN_PEDIDO> GetPedidos()
        {
            if (_pedidos != null) { return _pedidos; }
            _pedidos = new List<PLAN_PEDIDO>();
            DLM.db.Tabela s = new DLM.db.Tabela();
            List<PLAN_PEDIDO> lista = new List<PLAN_PEDIDO>();

            s = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_pedidos_planejamento_copia);


            List<Task> Tarefas = new List<Task>();
            foreach (var t in s.Linhas)
            {
                lista.Add(new PLAN_PEDIDO(t, new PLAN_OBRA()));
            }
            Task.WaitAll(Tarefas.ToArray());
            _pedidos.AddRange(lista);
            _pedidos = _pedidos.OrderBy(x => x.pedido).ToList();

            foreach (var o in GetObras(true,false))
            {
                o.Set(_pedidos);
            }

            var titulos = GetTitulosPedidos();
            var st_base = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_03_pedido);
            var lista_resumos = Consultas.getresumo_pecas_pedidos();


            foreach (var t in _pedidos)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var t0 = lista_resumos.Find(x => x.pep == t.pep);
                    if (t0 != null)
                    {
                        t.resumo_pecas = t0;
                    }
                    t.Set(titulos, false);

                    var igual = st_base.Filtrar("pep", t.pep, true);
                    if (igual.Count > 0)
                    {
                        t.SetBase(igual.Linhas.First());
                    }

                }));
            }
            Task.WaitAll(Tarefas.ToArray());


           
            return _pedidos;
        }
        
        
        public static List<PLAN_PEDIDO> GetPedidos(List<string> contrato)
        {

            contrato = contrato.Distinct().ToList().FindAll(x => x.Length > 0);
            if(contrato.Count==0)
            {
                return GetPedidos();
            }

            List<PLAN_PEDIDO> pedidos = new List<PLAN_PEDIDO>();
            foreach(var s in contrato)
            {
                pedidos.AddRange(GetPedidos().FindAll(x => x.pep.Contains(s.Replace("%","").Replace("*",""))));
            }
            pedidos = pedidos.GroupBy(x => x.pep).Select(x => x.First()).ToList();

            return pedidos;
        }
        public static List<PLAN_ETAPA> GetEtapas(List<string> pedidos)
        {
            var _etapas = new List<PLAN_ETAPA>();
            var subetapas = GetSubEtapas(pedidos);

            foreach (var et in subetapas.Select(x => x.etapa).OrderBy(x => x).Distinct().ToList())
            {
                _etapas.Add(new PLAN_ETAPA(subetapas.FindAll(x => x.etapa == et)));
            }

            var titulos = GetTitulosEtapas();
            var st_base = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_02_etapa);
           

            List<Task> Tarefas = new List<Task>();

            foreach (var t in _etapas)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                   
                    t.Set(titulos, false);

                    var igual = st_base.Filtrar("pep", t.pep.ToUpper().TrimEnd(".P00".ToCharArray()), true);
                    if (igual.Count > 0)
                    {
                        t.SetBase(igual.Linhas.First());
                    }

                }));
            }
            Task.WaitAll(Tarefas.ToArray());


            if (_pedidos!=null)
            {
                foreach(var p in _pedidos)
                {
                    p.Set(_etapas);
                }
            }

            return _etapas;
        }
        public static List<PLAN_SUB_ETAPA> GetSubEtapas(List<string> pedidos)
        {
            var _subetapas = new List<PLAN_SUB_ETAPA>();
            var peps = GetPeps(pedidos);

            string chave_pedidos = "";

            List<string> consultas = new List<string>();
            foreach (var pedido in pedidos)
            {
                var ss = Buffer_Subetapas.FindAll(x => x.pep.Contains(pedido.Replace("*", "")));
                _subetapas.AddRange(ss.FindAll(x => _subetapas.Find(y => y.pep == x.pep) == null));
                if (ss.Count == 0)
                {
                    consultas.Add(pedido);
                }
            }

            foreach (var pedido in consultas)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento_m_copia} as pr where ") + $" pr.pep like '%{pedido}%'";
            }




            var s = DBases.GetDB().Consulta(chave_pedidos.Replace("%%", "%"));
            ConcurrentBag<PLAN_SUB_ETAPA> lista = new ConcurrentBag<PLAN_SUB_ETAPA>();

            List<Task> Tarefas = new List<Task>();

            foreach (var t in DLM.painel.Consultas.quebrar_lista(s.Linhas, 500))
            {
                foreach (var ss in t)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                                  lista.Add(new PLAN_SUB_ETAPA(ss, peps))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            _subetapas.AddRange(lista);
            _subetapas = _subetapas.OrderBy(x => x.subetapa).ToList();
            var lista_resumos = Consultas.getresumo_pecas_subetapas(pedidos);
            var st_base = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_01_subetapa);
            var titulos = GetTitulosSubEtapas();


            foreach (var t in _subetapas)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var t0 = lista_resumos.Find(x => x.pep == t.pep);
                    if (t0 != null)
                    {
                        t.resumo_pecas = t0;
                    }
                    t.Set(titulos, false);

                    var igual = st_base.Filtrar("pep", t.pep.ToUpper().TrimEnd(".P00".ToCharArray()), true);
                    if (igual.Count > 0)
                    {
                        t.SetBase(igual.Linhas.First());
                    }

                }));
            }
            Task.WaitAll(Tarefas.ToArray());


            return _subetapas;
        }
        public static List<PLAN_PEP> GetPeps(List<string> chaves)
        {
            List<PLAN_PEP> retorno = new List<PLAN_PEP>();

            string tabela = $"{Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento}";
            var sub = quebrar_lista(chaves, 300);

            foreach (var sublista in sub)
            {
                var chaves_consulta = sublista.Select(x => x.Replace(" ", "").Replace("*", "")).ToList().FindAll(x => x != "").Distinct().ToList();
                string chave = "";
                if (chaves_consulta.Count == 0)
                {
                    continue;
                }
                for (int i = 0; i < chaves_consulta.Count; i++)
                {
                    if (i == 0)
                    {
                        chave = $"{tabela}.pep like '%{chaves_consulta[i] }%'";
                    }
                    else
                    {
                        chave = chave + $" or {tabela}.pep like '%{chaves_consulta[i] }%'";
                    }
                }



                string comando = $"select *  from {tabela} where {chave}";

                var s = DBases.GetDB().Consulta(comando);

                ConcurrentBag<PLAN_PEP> lista = new ConcurrentBag<PLAN_PEP>();
                foreach (var ts in DLM.painel.Consultas.quebrar_lista(s.Linhas, 300))
                {
                    List<Task> Tarefas = new List<Task>();
                    foreach (var t in ts)
                    {
                        Tarefas.Add(Task.Factory.StartNew(() => lista.Add(new PLAN_PEP(t))));
                    }
                    Task.WaitAll(Tarefas.ToArray());
                    Tarefas.Clear();
                }


                retorno.AddRange(lista);




            }

            setResumos(retorno);


            var st_base = DBases.GetDB().Clonar().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_00_pep);
            foreach (var p in retorno)
            {
                var igual = st_base.Filtrar("pep", p.pep, true);
                if (igual.Count > 0)
                {
                    p.SetBase(igual.Linhas.First());
                }
            }


            return retorno;
        }

        public static List<Logistica_Planejamento> GetLogistica(List<string> pedidos, List<PLAN_PECA> pecas, out List<PLAN_PECA> orfas)
        {
            orfas = new List<PLAN_PECA>();
            if(pedidos==null && pecas == null)
            {
                return new List<Logistica_Planejamento>();
            }
            else if(pedidos==null && pecas!=null)
            {
                pedidos = pecas.Select(x => x.pedido_completo).Distinct().ToList();
            }
            pedidos = pedidos.FindAll(x => x.Length > 5).Distinct().ToList();
            if (pedidos.Count == 0) { return new List<Logistica_Planejamento>(); }
            if(pecas==null)
            {
            pecas = GetPecasReal(pedidos);
            }
            List<Logistica_Planejamento> retorno = new List<Logistica_Planejamento>();
            var sub = quebrar_lista(pedidos, 50);

            ConcurrentBag<PLAN_PECA> orfs = new ConcurrentBag<PLAN_PECA>();
            List<Task> Tarefas = new List<Task>();
            foreach (var t in sub)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    List<PLAN_PECA> orrfas = new List<PLAN_PECA>();
                    retorno.AddRange(getLogisticaF(t, pecas,out orrfas));
                    foreach(var o in orrfas)
                    {
                        orfs.Add(o);
                    }
                }));
                if (Tarefas.Count > 5)
                {
                    Task.WaitAll(Tarefas.ToArray());
                    Tarefas.Clear();
                }

            }
            Task.WaitAll(Tarefas.ToArray());

            orfas.AddRange(orfs);

            return retorno.FindAll(x=>x.peca!=null);
        }
        public static List<PLAN_PECA> GetPecasReal(List<string> pedidos,int max_pacote = 10)
        {
            pedidos = pedidos.FindAll(x => x.Length > 5).Distinct().ToList();
            if (pedidos.Count == 0) { return new List<PLAN_PECA>(); }
            List<PLAN_PECA> retorno = new List<PLAN_PECA>();
            var pacote_pedidos = quebrar_lista(pedidos, max_pacote);


            List<Task> Tarefas = new List<Task>();

            foreach (var peds in pacote_pedidos)
            {

                var pecas = getPecasF(peds);
               Tarefas.Add(Task.Factory.StartNew(() =>
                {

                    var codigos_bobinas = pecas.Select(x => x.codigo_materia_prima_sap).Distinct().ToList().FindAll(x => x.Replace(" ", "").Replace("0", "") != "");

                    foreach (var cod in codigos_bobinas)
                    {
                        var bobina = DBases.GetBancoRM().GetBobina(cod);
                        if (bobina == null)
                        {
                            bobina = new Conexoes.Bobina() { SAP = cod };
                        }
                        if (bobina != null)
                        {
                            foreach (var ss in pecas.ToList().FindAll(x => x.codigo_materia_prima_sap == cod))
                            {
                                ss.bobina = bobina;
                            }
                        }
                    }
                }));

                retorno.AddRange(pecas);


            }







            return retorno;
        }
        public static List<PLAN_PECA> GetPecasReal(List<PLAN_SUB_ETAPA> subetapas)
        {
            var subs_sem_pecas = subetapas.FindAll(x => !x.carregou_pecas).ToList();
           var pcs = DLM.painel.Consultas.GetPecasReal(subs_sem_pecas.Select(x => x.subetapa).Distinct().ToList());
            List<Task> Tarefas = new List<Task>();
            foreach (var t in subs_sem_pecas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => t.Set(pcs)));
            }
            Task.WaitAll(Tarefas.ToArray());

            return subetapas.SelectMany(x => x.GetPecas()).ToList();
        }


        private static List<Conexoes.Bobina> _bobinas { get; set; }
        public static List<Conexoes.Bobina> GetBobinas()
        {
            if(_bobinas ==null)
            {
                _bobinas = new List<Conexoes.Bobina>();
                _bobinas.AddRange(DBases.GetBancoRM().GetBobinast());
            }
            return _bobinas;
        }
        private static List<Logistica_Planejamento> getLogisticaF(List<string> pedido, List<PLAN_PECA> pecas, out List<PLAN_PECA> orfas)
        {
            orfas = new List<PLAN_PECA>();
            if(pecas == null)
            {
                pecas = GetPecasReal(pedido);
            }
            string chave = "";
            pedido = pedido.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "");
            if (pedido.Count == 0)
            {
                return new List<Logistica_Planejamento>();
            }
            for (int i = 0; i < pedido.Count; i++)
            {
                if (i == 0)
                {
                    chave = "prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
                else
                {
                    chave = chave + " or prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
            }

            string consulta =  $"select * from {Cfg.Init.db_comum}.zpp0066n_logistica  as prod where " + chave;
            string consulta2 = $"select * from {Cfg.Init.db_comum}.cargas  as prod where " + chave.Replace("pep", "Elemento_PEP");
            var dbase = DBases.GetDB().Clonar();

            //var lista_fab = dbase.Consulta(consulta);
            var lista_fab_0100 = dbase.Clonar().Consulta(consulta2);
            ConcurrentBag<Logistica_Planejamento> retorno = new ConcurrentBag<Logistica_Planejamento>();

            List<Task> Tarefas = new List<Task>();
     

            foreach (var linha in lista_fab_0100.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Logistica_Planejamento(pecas, linha, Tipo_Embarque.ZPP0100))));
            }
            Task.WaitAll(Tarefas.ToArray());
            Tarefas.Clear();

            var retorn = retorno.OrderBy(x => x.peca + "-" + x.desenho).ToList();
            getNotasFiscais(retorn);

            for (int i = 0; i < pecas.Count; i++)
            {
                if (pecas[i].pep.Length>0 && pecas[i].material!="")
                {
                    var logs = retorno.ToList().FindAll(x => x.pep == pecas[i].pep && x.material == pecas[i].material);
                    pecas[i].SetLogistica(logs);
                }
                else
                {

                }
            }

            var sem_peca = retorn.FindAll(x => x.peca == null).ToList();

            var grp_sem_peca = sem_peca.GroupBy(x => x.pep + "/" + x.material + "/" + x.desenho).ToList().Select(x=> x.ToList()).ToList();
            List<PLAN_PECA> pcas_orfas = new List<PLAN_PECA>();
            foreach(var s in grp_sem_peca)
            {
                var fils = s.ToList();
                var pc = new PLAN_PECA(fils[0]);
                pc.SetLogistica(fils);
                
                orfas.Add(pc);
            }

            return retorn;
        }
        public static void getNotasFiscais(List<Logistica_Planejamento> pedido)
        {
            string chave = "";
            var NFS = pedido.Select(x => x.nota_fiscal.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "").Distinct().ToList();
            if(NFS.Count==0)
            {
                return;
            }
            if (pedido.Count == 0)
            {
                return;
            }
            for (int i = 0; i < NFS.Count; i++)
            {
                if (i == 0)
                {
                    chave = "prod.NF = '$P$'".Replace("$P$", NFS[i]);
                }
                else
                {
                    chave = chave + " or prod.NF = '$P$'".Replace("$P$", NFS[i]);
                }
            }


            var dbase = DBases.GetDB().Clonar();

            var lista_fab = dbase.Consulta($"SELECT * from {Cfg.Init.db_comum}.{Cfg.Init.tb_zcontratos_notas_fiscais} as prod where " + chave);

            foreach (var s in NFS)
            {
                var pcs = pedido.FindAll(x => x.nota_fiscal == s);
                var info = lista_fab.Filtrar("NF", s, true);
                if(info.Count>0)
                {
                    var t = info.Linhas.First().Get("Data_emissao").Data();
                    if(t> new DateTime(2001,01,01))
                    {
                        foreach(var pc in pcs)
                        {
                            pc.data = t.ToShortDateString().ToString();
                        }
                    }
                }
            }
        }
        private static List<PLAN_PECA> getPecasF(List<string> pedido, string material = null)
        {
            string chave = "";
            pedido = pedido.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x !="");
            if(pedido.Count==0)
            {
                return new List<PLAN_PECA>();
            }
            for (int i = 0; i < pedido.Count; i++)
            {
            if(i==0)
                {
                    chave = "prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
            else
                {
                    chave = chave + " or prod.pep like '%$P$%'".Replace("$P$", pedido[i]);
                }
            }

            if(material!=null)
            {
                chave = "(" + chave + ") and prod.material like'" + material + "%')";
            }


   string consulta =
                $"select prod.pep AS pep,"+
                $"coois.pep as pep_cooisn," +
                $"substr(prod.pep, -24, 21) as subetapa," +
                $"logi.desenho as desenho," +
                $"prod.material as material," +
                $"prod.texto_breve as texto_breve," +
                $"prod.peso_necessario / prod.qtd_necessaria as peso_unitario," +
                $"prod.peso_necessario as peso_necessario," +
                $"prod.peso_produzido as peso_produzido," +
                $"prod.qtd_necessaria AS qtd_necessaria," +
                $"prod.peso_produzido / (prod.peso_necessario / prod.qtd_necessaria) AS qtd_produzida," +
                $"sum(if ((logi.carga_confirmada = 'True'),logi.quantidade,0)) AS qtd_embarcada," +
                $"prod.grupo_mercadoria as grupo_mercadoria," +
                $"prod.denominacao as denominacao," +
                $"prod.centro as centro," +
                $"prod.centro_producao as centro_producao," +
                $"coois.DENOMINDSTAND as DENOMINDSTAND," +
                $"coois.ULTIMO_STATUS as ULTIMO_STATUS," +
                $"str_to_date(coois.DATA_INICIO, '%d/%m/%Y') AS DATA_INICIO," +
                $"str_to_date(coois.DATA_FIM, '%d/%m/%Y') AS DATA_FIM," +
                $"coois.DESENHO_1 as DESENHO_1," +
                $"coois.TIPO_DE_PINTURA as TIPO_DE_PINTURA," +
                $"coois.CORTE_LARGURA as CORTE_LARGURA," +
                $"coois.COMPRIMENTO as COMPRIMENTO," +
                $"coois.ESQ_DE_PINTURA as ESQ_DE_PINTURA," +
                $"coois.SUPERFICIE as SUPERFICIE," +
                $"coois.FURACOES as FURACOES," +
                $"coois.ESPESSURA as ESPESSURA," +
                $"coois.TIPO_ACO as TIPO_ACO," +
                $"coois.CODIGO_MATERIA_PRIMA_SAP as CODIGO_MATERIA_PRIMA_SAP," +
                $"coois.MARCA as MARCA," +
                $"prod.ultima_edicao as ultima_edicao," +
                $"coois.ultimo_update as cooisn_ultima_edicao" +
                $" from" +
                $"(" +
                $"(" +
                $"({Cfg.Init.db_comum}.{Cfg.Init.tb_zpmp_producao} as prod" +
                $" left join {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0066n_logistica} as logi on(prod.pep = logi.pep and prod.material = logi.material))" +
                $" left join {Cfg.Init.db_comum}.{Cfg.Init.tb_zppcooisn} as coois on(substr(prod.pep, -24, 21) = substr(coois.pep, -24, 21)  and prod.material = coois.material)" +
                $")" +
                $"left join {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento} as pps on(prod.pep = pps.pep) " +
                $")" +
                $" where" +
                $" (not((prod.material like '300%')) and " +
                $" ($P$)".Replace("$P$", chave) +
                $" and prod.status_sistema_pep not like '%BLOQ%'" +
                $")" +
                $" and pps.pep is not null" +
                $"  group by prod.pep,prod.material";

            var dbase = DBases.GetDB().Clonar();
            var maximo = 10000;
            string consulta_emb = "" +
                "select prod.Elemento_PEP as Elemento_PEP," +
                "prod.Material as Material," +
                "prod.St_Embarque as St_Embarque," +
                "prod.Qtd_Embarque as Qtd_Embarque," +
                "prod.St_Conf_ as St_Conf_," +
                "prod.Tamanho_dimensao as Tamanho_dimensao" +
                $" from {Cfg.Init.db_comum}.{Cfg.Init.tb_zpp0100_embarques} as prod where ($P$) and prod.St_Conf_ = '@5Y@'".Replace("$P$", chave.Replace("pep", "Elemento_PEP"));
            var lista_fab = dbase.Consulta(consulta);
            var lista_zpp0100 = dbase.Clonar().Consulta(consulta_emb);
            ConcurrentBag <PLAN_PECA> retorno = new ConcurrentBag<PLAN_PECA>();
            /*quebra da consulta em sub_consultas*/
            var sub_max = lista_fab.Linhas.quebrar_lista(maximo);
            List<Task> Tarefas = new List<Task>();

            foreach (var ss in sub_max)
            {
                foreach (var s in ss)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new PLAN_PECA(s))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            var peps_chaves = lista_zpp0100.Linhas.GroupBy(x => Conexoes.Utilz.PEP.Get.Subetapa(x.Get("Elemento_PEP").ToString(),true));

            Tarefas = new List<Task>();
            foreach(var pep in peps_chaves)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var querys = pep.ToList();
                    var pcs = retorno.ToList().FindAll(x => Conexoes.Utilz.PEP.Get.Subetapa(x.pep, true) == pep.Key);
                    foreach (var pc in pcs)
                    {
                        pc.SetStatusByZPP0100(querys);
                    }
                }));

            }



           

            return retorno.OrderBy(x => x.pep + "-" + x.desenho).ToList();
        }
        public static SolidColorBrush getCor(double previsto,double realizado, double opacidade = 1)
        {
            if(realizado>=100)
            {
                return new SolidColorBrush(Colors.DarkGreen) { Opacity = opacidade };
            }
            else if(realizado==0 && previsto==0)
            {
                return new SolidColorBrush(Colors.DarkGray) { Opacity = opacidade };
            }
            var t = previsto - realizado;
            if (t >= 40)
            {
                return new SolidColorBrush(Colors.Red) { Opacity = opacidade };
            }
            else if (t >= 20)
            {
                return new SolidColorBrush(Colors.OrangeRed) { Opacity = opacidade };
            }
            else if (t >= 10)
            {
                return new SolidColorBrush(Colors.Yellow) { Opacity = opacidade };
            }
            else if (t >= 5)
            {
                return new SolidColorBrush(Colors.LightYellow) { Opacity = opacidade };
            }


            return new SolidColorBrush(Colors.LightBlue) { Opacity = opacidade };
        }
        public static List<PLAN_OBRA> BufferObrasPesquisa { get; set; } = new List<PLAN_OBRA>();

        private static List<string> _pedidos_clean { get; set; }
        public static List<string> GetPedidosClean(List<string> contratos, bool reset)
        {
            if (_pedidos_clean == null | reset)
            {
                var s = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_pedidos_clean);
                _pedidos_clean = s.Linhas.Select(x => x["pedido"].Valor).ToList();
            }
            List<string> retorno = new List<string>();
            foreach(var s in contratos.Distinct().ToList().FindAll(x=>x.Length>5))
            {
                retorno.AddRange(_pedidos_clean.FindAll(x => x.Contains(s)));
            }

            return retorno.Distinct().ToList();

        }

        public static List<PLAN_SUB_ETAPA> Buffer_Subetapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        public static void SetPeps(List<PLAN_SUB_ETAPA> subetapas)
        {
            var sem_peps = subetapas.FindAll(x => !x.carregou_peps);
            var peps = DLM.painel.Consultas.GetPeps(sem_peps.Select(x => x.subetapa).Distinct().ToList());
            foreach(var sub in sem_peps)
            {
                sub.Set(peps);
            }
        }
 
        public static void LimparCOOISN(string chave)
        {
            DBases.GetDB().Apagar("pep", $"%{chave}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn, true);
        }
        public static bool MatarExcel(bool confirmar = false)
        {
            var t = Process.GetProcessesByName("EXCEL").ToList();
            t.AddRange(Process.GetProcesses().ToList().FindAll(x => x.ProcessName.ToUpper().Contains("SOFFICE")));
            if (t.Count > 0)
            {
                bool matar = false;
                if (confirmar)
                {
                    matar = Conexoes.Utilz.Pergunta("Há janelas do excel abertas. Para poder continuar, é necessário fecha-las. Se clicar em sim, o sistema fechará todas as janelas. Se deseja salvar algo, faça antes de confirmar essa tela. Se clicar em não, a operação será abortada.");
                }
                else
                {
                    matar = true;
                }
                if (matar)
                {
                    foreach (var s in t)
                    {
                        try
                        {
                            s.Kill();

                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                else
                {
                    return matar;
                }
            }
            return true;
        }
        public static void setResumos(List<PLAN_PEP> peps)
        {
            peps = peps.OrderBy(x => x.pep).ToList();
            List<string> contratos = peps.Select(x => x.Contrato_Completo).Distinct().ToList();
            var resumos = Consultas.getresumo_pecas_peps(contratos);

            foreach (var ped in contratos)
            {
                var pps = peps.FindAll(x => x.Contrato_Completo == ped);
         
                foreach(var p in pps)
                {
                    var pep = resumos.Find(x => x.pep == p.pep);
                    if(pep == null)
                    {
                        var peps_sistema = resumos.FindAll(x => x.subetapa == p.subetapa);
                        var status_outros = peps_sistema.FindAll(y=> pps.Find(x=>x.pep == y.pep) ==null);
                        if(status_outros.Count>0)
                        {
                            p.resumo_pecas = status_outros[0];

                        }

                    }
                    else
                    {

                    p.resumo_pecas = pep;
                    }
                }
            }
        }

        public static List<PLAN_SUB_ETAPA> GetSubEtapasPorDataMontagem(DateTime dt_ini, DateTime dt_fim, string pedido)
        {
            var fim = $"str_to_date('{dt_fim.ToShortDateString()}', '%d/%m/%Y')";
            var ini = $"str_to_date('{dt_ini.ToShortDateString()}', '%d/%m/%Y'))";
            string comando = ($"select *  from {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento_m_copia} as pr where " +
                $"((pr.mf <= {fim} and pr.mi >= {ini}) or (pr.montagem_fim <= {fim} and pr.montagem_inicio >= {ini}))" +
                $" and " +
                $"pr.pep like '%{pedido}% '");
            var s = DBases.GetDB().Consulta(comando);


            ConcurrentBag<PLAN_SUB_ETAPA> lista = new ConcurrentBag<PLAN_SUB_ETAPA>();
            var t = new List<PLAN_SUB_ETAPA>();
            foreach (var ts in s.Linhas)
            {
                //Tarefas.Add(Task.Factory.StartNew(() => 
                lista.Add(new PLAN_SUB_ETAPA( ts, null))
                //))
                //
                ;
            }
            //Task.WaitAll(Tarefas.ToArray());

            return lista.ToList().OrderBy(x => x.pep).ToList();
        }
        public static List<List<List<T>>> quebrar_lista<T>(this List<List<T>> locations, int maximo = 30)
        {
            var list = new List<List<List<T>>>();

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
        public static List<List<T>> quebrar_lista<T>( this List<T> locations, int maximo = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
    }
}
