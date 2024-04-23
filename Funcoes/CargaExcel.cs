using Conexoes;
using DLM.painel;
using DLM.vars;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    public class CargaExcel
    {
        public static int max_tasks { get; set; } = 700;
        public static List<ZCONTRATOS> ZCONTRATO(string arquivo)
        {
            var retorno = new ConcurrentBag<ZCONTRATOS>();
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZCONTRATOS(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.Elemento_PEP.Replace(" ", "") != "");
        }
        public static List<ZPP0100> ZPP0100(string arquivo, out DLM.db.Tabela tabela)
        {
            var retorno = new ConcurrentBag<ZPP0100>();
            tabela = Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPP0100(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }
            return retorno.ToList().FindAll(x => x.Elemento_PEP.Replace(" ", "") != "");
        }
        public static List<ZPP0112> ZPP0112(string arquivo)
        {
            var retorno = new ConcurrentBag<ZPP0112>();
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPP0112(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.Elemento_PEP.Replace(" ", "") != "");
        }
        public static List<SAPFAGLB03> FAGLB03(string arquivo, string empresa_de = "1100", string empresa_ate = "1200", int ano = 2022, string conta = "3111003011", bool cadastrar = true)
        {
            var linhas = Conexoes.Utilz.Arquivo.Ler(arquivo);
            List<SAPFAGLB03> retorno = new List<SAPFAGLB03>();
            foreach (var linha in linhas)
            {
                var valores = linha.Split('|').ToList().Select(x => x.TrimStart().TrimEnd()).ToList();

                if (valores.Count >= (int)TAB_FAGLB03.Dt_lcto)
                {
                    SAPFAGLB03 pp = new SAPFAGLB03(valores, empresa_de, empresa_ate, ano, conta);
                    retorno.Add(pp);
                }
            }
            db.Linha chave = new db.Linha();
            chave.Add("K_ano", ano);
            chave.Add("K_Empresa_De", empresa_de);
            chave.Add("K_Empresa_Ate", empresa_ate);
            chave.Add("K_Conta", conta);
            Conexoes.DBases.GetDB().Apagar(chave, Cfg.Init.db_comum, Cfg.Init.tb_faglb03);
            var tabela = retorno.GetTabela(true);
            Conexoes.DBases.GetDB().Cadastro(tabela.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_faglb03);
            return retorno;
        }
        public static List<AVANCO_FATURAMENTO> Cadastro_AVANCO_FATURAMENTO()
        {
            var retorno = new List<AVANCO_FATURAMENTO>();
            string arquivo = Conexoes.Utilz.Abrir_String("xlsx");
            if (arquivo == null) { return new List<AVANCO_FATURAMENTO>(); }
            var selecao = Conexoes.Utilz.Excel.GetTabelaPrompt(arquivo);

            if (selecao.Count > 0)
            {
                foreach (var linha in selecao.Linhas)
                {
                    var pedido =         linha["C1"].Valor;
                    var valor_contrato = linha["C6"].Valor;
                    var valor_f_direto = linha["C7"].Valor;

                    var novo = new AVANCO_FATURAMENTO(pedido, valor_contrato, valor_f_direto);
                    if (novo.Pedido.Length == 13)
                    {
                        retorno.Add(novo);
                    }
                }

                if (retorno.Count > 0)
                {
                    var cadastro = retorno.GetTabela(true);
                    DBases.GetDB().Apagar("id", "%%", Cfg.Init.db_comum, Cfg.Init.tb_avanco_faturamento);
                    DBases.GetDB().Cadastro(cadastro.Linhas, Cfg.Init.db_comum, Cfg.Init.tb_avanco_faturamento);
                    Conexoes.Utilz.Alerta($"Avanço sincronizado! {retorno.Count} itens cadastrados.");
                }
                else
                {
                    Conexoes.Utilz.Alerta("Nenhum item de avanço encontrado na aba selecionada.");
                }
            }
            return retorno;
        }
        public static List<FAGLL03> FAGLL03(string arquivo, string pedido)
        {
            var retorno = new ConcurrentBag<FAGLL03>();
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new FAGLL03(pedido, l))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.N_documento.Replace(" ", "") != "");
        }
        public static List<CJI3> CJI3(string arquivo)
        {
            var retorno = new ConcurrentBag<CJI3>();
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new CJI3(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.Elemento_PEP.Replace(" ", "") != "");
        }

        public static List<ZPMP> ZPMP(string arquivo, out DLM.db.Tabela tabela)
        {
            var retorno = new ConcurrentBag<ZPMP>();
            tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPMP(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.PEP.Codigo.Length > 0).ToList();
        }
        public static List<ZPP0066N> ZPP0066N(string arquivo, bool semperfil)
        {
            var retorno = new ConcurrentBag<ZPP0066N>();
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var sub in tabela.Linhas.Quebrar(max_tasks))
            {
                var Tarefas = new List<Task>();
                foreach (var linha in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPP0066N(linha, semperfil))));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return retorno.ToList().FindAll(x => x.Material != "").ToList();
        }
        public static List<PLAN_PECA_ZPMP> ZPPCOOISN(string destino, string Pedido, bool buffer = false)
        {
            var retorno = new List<PLAN_PECA_ZPMP>();

            //19/06/2020 - aumentei o filtro para pegar a subetapa. serão mais consultas, no entanto evita os erros.
            /*12/05/2022 - mudei a chamada para um procedure.*/
            var ped = Pedido.Replace("*", "").Replace(" ", "");
            var chamada = $"call comum.getzppcoisn_qtd_pcs('{ped}')";
            var consulta = DBases.GetDB().Consulta(chamada);
            var peps = consulta.Linhas.Select(x => x["pep"].Valor).ToList();

            var total = consulta.Linhas.Select(x => x["pcs"].Int()).Sum();
            if(total>1500)
            {
                foreach (var pep in peps)
                {
                    var pecas = DLM.painel.Consultas.GetPecasZPMP(pep).ToList();
                    if (pecas.Count > 0)
                    {
                        DLM.painel.Consultas.MatarExcel(false);
                        ZPPCOOISN(destino, pep, pecas, buffer);
                        retorno.AddRange(pecas);
                    }
                }
            }
            else
            {
                /*11.08.2023 - simplificar chamadas -> pedidos com menos de 6 mil peças faz a coois completa*/
                var pecas = DLM.painel.Consultas.GetPecasZPMP(ped).ToList();
                if (pecas.Count > 0)
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    ZPPCOOISN(destino, ped, pecas, buffer);
                    retorno.AddRange(pecas);
                }
            }


           
            return retorno;
        }
        private static void ZPPCOOISN(string dest, string Pedido, List<DLM.painel.PLAN_PECA_ZPMP> retorno, bool buffer = false)
        {
            if (dest != "")
            {
                string arq = Pedido.Replace("*", "").Replace("%", "") + Cfg.Init.SAP_ZPPCOOISNARQ;
                SAP_Consulta_Macro p = new SAP_Consulta_Macro();
                if (!buffer)
                {
                    if (p.ZPPCOOISN(Pedido, dest, arq, false, "/SISTEMA"))
                    {
                        Carregar_ZPPCOOISN_Layout(Pedido, retorno, dest, arq);
                    }
                }
                else
                {
                    Carregar_ZPPCOOISN_Layout(Pedido, retorno, dest, arq);
                }


            }
        }
        public static string TratarPEP(string PEP)
        {
            // 20/10/2020
            //TRATAMENTO QUE ALTERA QUALQUER LETRA DA UNIDADE FABRIL PRA 'F' 
            // PRA RESOLVER O PROBLEMA DE MERDA OS CADASTRO DE PEP NA 
            //LOGISTICA QUE FICAM FAZENDO TIGRADA DE BOTAR NOMES NÃO PADRÃO
            string ret = "";
            if (PEP.Length > 22 &&
                (
                PEP.EndsWith(".Z2") |
                PEP.EndsWith(".Z3") |
                PEP.EndsWith(".Z4") |
                PEP.EndsWith(".ZO") |

                PEP.EndsWith(".G2") |
                PEP.EndsWith(".G3") |
                PEP.EndsWith(".G4") |
                PEP.EndsWith(".GO") |

                PEP.EndsWith(".A2") |
                PEP.EndsWith(".A3") |
                PEP.EndsWith(".A4") |
                PEP.EndsWith(".AO") |

                PEP.EndsWith(".B2") |
                PEP.EndsWith(".B3") |
                PEP.EndsWith(".B4") |
                PEP.EndsWith(".BO") |

                PEP.EndsWith(".C2") |
                PEP.EndsWith(".C3") |
                PEP.EndsWith(".C4") |
                PEP.EndsWith(".CO") |

                PEP.EndsWith(".D2") |
                PEP.EndsWith(".D3") |
                PEP.EndsWith(".D4") |
                PEP.EndsWith(".DO") |

                PEP.EndsWith(".E2") |
                PEP.EndsWith(".E3") |
                PEP.EndsWith(".E4") |
                PEP.EndsWith(".EO") |

                PEP.EndsWith(".H2") |
                PEP.EndsWith(".H3") |
                PEP.EndsWith(".H4") |
                PEP.EndsWith(".HO") |

                PEP.EndsWith(".I2") |
                PEP.EndsWith(".I3") |
                PEP.EndsWith(".I4") |
                PEP.EndsWith(".IO") |

                PEP.EndsWith(".J2") |
                PEP.EndsWith(".J3") |
                PEP.EndsWith(".J4") |
                PEP.EndsWith(".JO") |

                PEP.EndsWith(".K2") |
                PEP.EndsWith(".K3") |
                PEP.EndsWith(".K4") |
                PEP.EndsWith(".KO") |

                PEP.EndsWith(".R2") |
                PEP.EndsWith(".R3") |
                PEP.EndsWith(".R4") |
                PEP.EndsWith(".RO") |




                PEP.EndsWith(".A3")
                )
                )
            {
                ret = PEP.Substring(0, PEP.Length - 2) + "F" + PEP[PEP.Length - 1];
            }
            else
            {
                ret = PEP;
            }
            return ret;
        }
        private static void Carregar_ZPPCOOISN_Layout(string Pedido, List<PLAN_PECA_ZPMP> pecas, string dest = null, string arq = null)
        {
            if (Pedido.Length < 6) { return; }
            if (dest == null) { dest = Cfg.Init.DIR_APP.GetSubPasta("SAP"); }
            if (arq == null) { arq = Pedido.Replace("*", "").Replace("%", "") + Cfg.Init.SAP_ZPPCOOISNARQ; }
            if (pecas == null) { return; }

            if (pecas.Count == 0) { return; }

            if (File.Exists(dest + arq))
            {
                DBases.GetDB().Apagar("pep", $"%{Pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn);

                var arquivo = dest + arq;
                var t = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
                var listacooisn = t.Linhas.Select(x => new List<object> {
                            /*0*/x[(int)TAB_ZPPCOOISN.MATERIAL].ToString(),
                            /*1*/x[(int)TAB_ZPPCOOISN.DENOMINDSTAND].ToString(),
                            /*2*/x[(int)TAB_ZPPCOOISN.DESENHO_1].ToString(),
                            /*3*/x[(int)TAB_ZPPCOOISN.PINTURA_TIPO].ToString(),
                            /*4*/x[(int)TAB_ZPPCOOISN.ELEMENTO_PEP].ToString(),
                            /*5*/x[(int)TAB_ZPPCOOISN.DATA_APONTAMENTO].Data(),
                            /*6*/x[(int)TAB_ZPPCOOISN.TXTBREVE_OPERACAO].ToString(),
                            /*7*/x[(int)TAB_ZPPCOOISN.ESPESSURA].ToString(),
                            /*8*/x[(int)TAB_ZPPCOOISN.CORTE].ToString(),
                            /*9*/x[(int)TAB_ZPPCOOISN.COMPRIMENTO].ToString(),
                            /*10*/x[(int)TAB_ZPPCOOISN.PINTURA_ESQUEMA].ToString(),
                            /*11*/x[(int)TAB_ZPPCOOISN.PINTURA_SUPERFICIE].ToString(),
                            /*12*/x[(int)TAB_ZPPCOOISN.FUROS].ToString(),
                            /*13*/x[(int)TAB_ZPPCOOISN.MARCA].ToString(),
                            /*14*/x[(int)TAB_ZPPCOOISN.OPERACAO].ToString(),
                            /*15*/x[(int)TAB_ZPPCOOISN.ESPESSURA].ToString(),
                            /*16*/x[(int)TAB_ZPPCOOISN.TIPO_DE_ACO].ToString(),
                            /*17*/x[(int)TAB_ZPPCOOISN.MATERIA_PRIMA].ToString(),
                            /*18*/x[(int)TAB_ZPPCOOISN.MARCA].ToString(),
                            /*19*/TratarPEP(x[(int)TAB_ZPPCOOISN.ELEMENTO_PEP].ToString()),
                            /*20*/"",
                            /*21*/x[(int)TAB_ZPPCOOISN.DESENHO_2].ToString(),
                        }).Distinct().ToList();

                var min = Cfg.Init.DataDummy();
                var pcs = pecas.FindAll(x => x.Material.ToString().StartsW("31"));
                var linhas = new List<DLM.db.Linha>();
                foreach (var peca in pcs)
                {


                    var curr = listacooisn.FindAll(x => peca.Material.ToString() == x[0].ToString()
                    |
                    (peca.Marca == x[18].ToString() && peca.PEP == x[18].ToString())
                    );
                    //ordena pela operação
                    curr = curr.OrderBy(x => x[18].ToString()).ToList();

                    var curr_marca = curr.Find(x => x[18].Int() == 10);
                    var curr_submateriais = new List<List<object>>();
                    if (curr_marca != null)
                    {
                        curr_submateriais = listacooisn.FindAll(x => x[13].ToString() == curr_marca[13].ToString() && x[4].ToString() == curr_marca[4].ToString() && x[0].ToString() != curr_marca[0].ToString());

                    }
                    curr.AddRange(curr_submateriais);
                    curr = curr.OrderBy(x => x[5].ToString().Data()).ToList();
                    //08/04/2020
                    curr = curr.OrderBy(x => x[18].ToString()).ToList();
                    if (curr.Count > 0)
                    {
                        var Dts = curr.Select(x => x[5].ToString().Data()).OrderBy(x => x).ToList().FindAll(x => x > Cfg.Init.DataDummy());
                        DateTime? inicio = Cfg.Init.DataDummy();
                        DateTime? fim = Cfg.Init.DataDummy();
                        string ultimo_status = "";
                        if (Dts.Count > 0)
                        {
                            inicio = Dts.Min();
                            fim = Dts.Max();
                        }
                        for (int i = 0; i < curr.Count - 1; i++)
                        {
                            var data = curr[i][5].ToString().Data();
                            if (data > Cfg.Init.DataDummy())
                            {
                                if (curr.Count > i + 1)
                                {
                                    ultimo_status = curr[i + 1][6].ToString();
                                }
                                else
                                {
                                    ultimo_status = curr[i][6].ToString();
                                }
                            }
                        }
                        if (curr.Last()[5].ToString().Data() > Cfg.Init.DataDummy())
                        {
                            ultimo_status = curr.Last()[6].ToString();
                        }

                        if (ultimo_status == "")
                        {
                            ultimo_status = "ORDEM CRIADA";
                        }
                        var acos = curr.Select(x => x[16].ToString()).Distinct().ToList();
                        var materias_primas = curr.Select(x => x[17].ToString()).Distinct().ToList().FindAll(x => x.Replace(" ", "").Replace("0", "") != "");
                        string aco = "";
                        string codigo_materia_prima = "";
                        if (materias_primas.Count == 1)
                        {
                            codigo_materia_prima = materias_primas[0];
                        }
                        if (acos.Count > 0)
                        {
                            aco = acos[0];
                        }

                        var pdfs = curr.Select(x => x[2].ToString()).ToList();

                        pdfs.AddRange(curr.Select(x => x[21].ToString()).ToList());
                        pdfs = pdfs.Distinct().ToList().FindAll(x => x.Length > 0);

                        string desenho = pdfs.Find(x => x.ToUpper().Contains(Cfg.Init.DWG_FAB_FILTRO));
                        if (desenho == null)
                        {
                            if (pdfs.Count > 0)
                            {
                                desenho = pdfs[0];
                            }
                            else
                            {
                                desenho = "";
                            }
                        }


                        var tipo_pintura = curr[0][3].ToString();
                        var denominstand = curr[0][1].ToString();

                        var valores = new db.Linha();
                        valores.Add("DENOMINDSTAND", denominstand);
                        valores.Add("DESENHO_1", desenho);
                        valores.Add("TIPO_DE_PINTURA", tipo_pintura);
                        valores.Add("DATA_INICIO", inicio > min ? inicio : null);
                        valores.Add("ULTIMO_STATUS", ultimo_status);
                        valores.Add("DATA_FIM", fim > min ? fim : null);
                        valores.Add("TIPO_ACO", aco);
                        valores.Add("CODIGO_MATERIA_PRIMA_SAP", codigo_materia_prima);

                        if (curr_marca != null)
                        {
                            double esp =    curr_marca[15].Double();
                            int furos =     curr_marca[12].Int();
                            string marca =  curr_marca[13].ToString();
                            double corte =  curr_marca[08].Double();

                            if (curr_submateriais.Count > 0)
                            {
                                if (furos == 0)
                                {
                                    furos = curr_submateriais.Select(x => x[12].Int()).Max();
                                }

                                esp = curr_submateriais[0][15].Double();


                                if (peca.Grupo_Mercadoria.Contains("PURLIN"))
                                {
                                    corte = curr_submateriais.Select(x => x[8].Double()).Max();
                                }

                            }

                            var superficie = curr_marca[11].Double();
                            var comprimento = curr_marca[9].Double();

                            /*04/04/2019 - novas caracteristicas adicionadas*/
                            valores.Add("MARCA", marca);
                            valores.Add("CORTE_LARGURA", corte);
                            valores.Add("COMPRIMENTO", comprimento);
                            valores.Add("ESQ_DE_PINTURA", curr_marca[10].ToString());
                            valores.Add("SUPERFICIE", superficie);
                            valores.Add("FURACOES", furos);
                            valores.Add("ESPESSURA", esp);
                        }

                        valores.Add("material", peca.Material);
                        valores.Add("pep", peca.PEP);

                        linhas.Add(valores);
                    }
                }
                DBases.GetDB().Cadastro(linhas, Cfg.Init.db_comum, "zppcooisn");
            }
        }

        public static List<CN47N> CN47N(string arquivo, out DLM.db.Tabela tabela)
        {
            var retorno = new ConcurrentBag<CN47N>();
            var Tarefas = new List<Task>();

            tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            foreach (var linha in tabela.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new CN47N(linha))));
            }
            Task.WaitAll(Tarefas.ToArray());

            return retorno.ToList();
        }
        public static DLM.sapgui.FolhaMargem ZSD0031N(string arquivo)
        {
            var tabela = Conexoes.Utilz.Excel.GetPrimeiraAba(arquivo, true);
            var plan = tabela.Linhas;
            var ret = new DLM.sapgui.FolhaMargem();
            try
            {
                int max_col = 12;
                int m = 3;
                int c12 = 11;
                int c4 = 3;
                if (plan.Count < 63) { return new DLM.sapgui.FolhaMargem(); }
                if (plan.Max(x => x.Count) < max_col) { return new DLM.sapgui.FolhaMargem(); }

                ret.receitabruta.material.valor = plan[19 - m][c12].Double();
                ret.receitabruta.montagem.valor = plan[20 - m][c12].Double();
                ret.receitabruta.projeto.valor = plan[21 - m][c12].Double();

                ret.impostos.IPI_Material.valor = plan[25 - m][c12].Double();
                ret.impostos.ICMS_Material.valor = plan[26 - m][c12].Double();
                ret.impostos.PIS_COFINS_Material.valor = plan[27 - m][c12].Double();
                ret.impostos.PIS_COFINS_Montagem.valor = plan[28 - m][c12].Double();
                ret.impostos.PIS_COFINS_Projeto.valor = plan[29 - m][c12].Double();
                ret.impostos.ISS_Locacao_Equipamentos.valor = plan[30 - m][c12].Double();
                ret.impostos.ISS_Supervisao.valor = plan[31 - m][c12].Double();
                ret.impostos.ISS_Montagem.valor = plan[32 - m][c12].Double();
                ret.impostos.ISS_Projeto.valor = plan[33 - m][c12].Double();
                ret.impostos.CPRB_Servico.valor = plan[34 - m][c12].Double();
                ret.impostos.CPRB_Material.valor = plan[35 - m][c12].Double();

                ret.impostos.IPI_Material.porcentagem = plan[25 - m][c4].Double();
                ret.impostos.ICMS_Material.porcentagem = plan[26 - m][c4].Double();
                ret.impostos.PIS_COFINS_Material.porcentagem = plan[27 - m][c4].Double();
                ret.impostos.PIS_COFINS_Montagem.porcentagem = plan[28 - m][c4].Double();
                ret.impostos.PIS_COFINS_Projeto.porcentagem = plan[29 - m][c4].Double();
                ret.impostos.ISS_Locacao_Equipamentos.porcentagem = plan[30 - m][c4].Double();
                ret.impostos.ISS_Supervisao.porcentagem = plan[31 - m][c4].Double();
                ret.impostos.ISS_Montagem.porcentagem = plan[32 - m][c4].Double();
                ret.impostos.ISS_Projeto.porcentagem = plan[33 - m][c4].Double();
                ret.impostos.CPRB_Servico.porcentagem = plan[34 - m][c4].Double();
                ret.impostos.CPRB_Material.porcentagem = plan[35 - m][c4].Double();

                ret.receitaliquida.material.valor = plan[39 - m][c12].Double();
                ret.receitaliquida.montagem.valor = plan[40 - m][c12].Double();
                ret.receitaliquida.projeto.valor = plan[41 - m][c12].Double();

                ret.custosmateriais.materiais.valor = plan[45 - m][c12].Double();
                ret.custosmateriais.contingencia.valor = plan[46 - m][c12].Double();

                ret.total_custo_projeto.valor = plan[48 - m][c12].Double();

                ret.custosmontagem.equipamentos.valor = plan[52 - m][c12].Double();
                ret.custosmontagem.empreiteiros_despesas.valor = plan[53 - m][c12].Double();
                ret.custosmontagem.supervisao.valor = plan[54 - m][c12].Double();

                ret.gastoslogisticos.frete_rodoviario.valor = plan[61 - m][c12].Double();
                ret.gastoslogisticos.verba_adicional.valor = plan[62 - m][c12].Double();
                ret.gastoslogisticos.gastos_logisticos.valor = plan[63 - m][c12].Double();
                ret.gastoslogisticos.outras_despesas.valor = plan[64 - m][c12].Double();
                ret.gastoslogisticos.seguro_internacional.valor = plan[65 - m][c12].Double();
                ret.gastoslogisticos.frete_aereo.valor = plan[66 - m][c12].Double();
                ret.gastoslogisticos.frete_maritimo.valor = plan[67 - m][c12].Double();
                ret.gastoslogisticos.frete_rodoviario_internacional.valor = plan[68 - m][c12].Double();
                ret.gastoslogisticos.frete_rodoviario_nacional_exportacao.valor = plan[69 - m][c12].Double();
                ret.gastoslogisticos.frete_maritimo_cabotagem.valor = plan[70 - m][c12].Double();
                ret.gastoslogisticos.frete_rodoviario_nacional_cabotagem.valor = plan[71 - m][c12].Double();

                ret.despesasgerais.seguro.valor = plan[75 - m][c12].Double();
                ret.despesasgerais.comissao.valor = plan[76 - m][c12].Double();
                ret.despesasgerais.assessoria.valor = plan[77 - m][c12].Double();
                ret.despesasgerais.custo_financeiro.valor = plan[78 - m][c12].Double();
                ret.despesasgerais.supervisao_exportacao.valor = plan[79 - m][c12].Double();
                ret.despesasgerais.creditos_debitos_material.valor = plan[80 - m][c12].Double();
                ret.despesasgerais.creditos_debitos_projeto.valor = plan[81 - m][c12].Double();
                ret.despesasgerais.creditos_debitos_montagem.valor = plan[82 - m][c12].Double();
                ret.despesasgerais.projeto_exportacao.valor = plan[83 - m][c12].Double();
                ret.despesasgerais.outros.valor = plan[84 - m][c12].Double();

                ret.despesasgerais.seguro.porcentagem =                     plan[75 - m][c4].Double();
                ret.despesasgerais.comissao.porcentagem =                   plan[76 - m][c4].Double();
                ret.despesasgerais.assessoria.porcentagem =                 plan[77 - m][c4].Double();
                ret.despesasgerais.custo_financeiro.porcentagem =           plan[78 - m][c4].Double();
                ret.despesasgerais.supervisao_exportacao.porcentagem =      plan[79 - m][c4].Double();
                ret.despesasgerais.creditos_debitos_material.porcentagem =  plan[80 - m][c4].Double();
                ret.despesasgerais.creditos_debitos_projeto.porcentagem =   plan[81 - m][c4].Double();
                ret.despesasgerais.creditos_debitos_montagem.porcentagem =  plan[82 - m][c4].Double();
                ret.despesasgerais.projeto_exportacao.porcentagem =         plan[83 - m][c4].Double();
                ret.despesasgerais.outros.porcentagem =                     plan[84 - m][c4].Double();

                ret.margens.valor = plan[63][c12].Double();
                ret.margens.porcentagem = plan[63][c4].Double();
                ret.Carregado = true;
            }
            catch (Exception)
            {

            }


            return ret;
        }
    }
}
