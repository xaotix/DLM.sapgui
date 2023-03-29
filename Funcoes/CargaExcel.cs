using Conexoes;
using DLM.painel;
using DLM.vars;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DLM.sapgui
{
    public class CargaExcel
    {
        public static int max_tasks { get; set; } = 700;
        public static List<ZCONTRATOS> ZCONTRATO(string arquivo)
        {
            ConcurrentBag<ZCONTRATOS> retorno = new ConcurrentBag<ZCONTRATOS>();
            var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
            foreach (var sub in DLM.painel.Consultas.quebrar_lista(t.Linhas, max_tasks))
            {
                List<Task> Tarefas = new List<Task>();

                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZCONTRATOS(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            return retorno.ToList().FindAll(x=>x.Elemento_PEP.Replace(" ","")!="");
        }
        public static List<ZPP0100> ZPP0100(string arquivo)
        {
            ConcurrentBag<ZPP0100> retorno = new ConcurrentBag<ZPP0100>();
            var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
            foreach (var sub in DLM.painel.Consultas.quebrar_lista(t.Linhas, max_tasks))
            {
                List<Task> Tarefas = new List<Task>();

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
            ConcurrentBag<ZPP0112> retorno = new ConcurrentBag<ZPP0112>();
            var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
            foreach (var sub in DLM.painel.Consultas.quebrar_lista(t.Linhas, max_tasks))
            {
                List<Task> Tarefas = new List<Task>();

                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPP0112(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            return retorno.ToList().FindAll(x => x.Elemento_PEP.Replace(" ", "") != "");
        }


        public static List<ZPMP> ZPMP(string arquivo)
        {
            ConcurrentBag<ZPMP> retorno = new ConcurrentBag<ZPMP>();
            var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
            foreach (var sub in DLM.painel.Consultas.quebrar_lista(t.Linhas, max_tasks))
            {
                List<Task> Tarefas = new List<Task>();

                foreach (var l in sub)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new ZPMP(l))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            return retorno.ToList();
        }

        public static List<DLM.painel.PLAN_PECA> ZPPCOOISN(string destino,string Pedido, bool buffer = false)
        {
            List<DLM.painel.PLAN_PECA> Retorno = new List<DLM.painel.PLAN_PECA>();

            //19/06/2020 - aumentei o filtro para pegar a subetapa. serão mais consultas, no entanto evita os erros.
            /*12/05/2022 - mudei a chamada para um procedure.*/
            var chamada = $"call {Cfg.Init.db_painel_de_obras2}.zpp_cooisn_get_qtd_pc('{Pedido.Replace("*","").Replace(" ","")}')";
            var consulta = DBases.GetDB().Consulta(chamada);
            var peps = consulta.Linhas.Select(x => x.Get("pep").Valor).ToList(); 
            var w = Conexoes.Utilz.Wait(peps.Count, Pedido + " ZPPCOOISN"); 
            w.somaProgresso();
            foreach (var s in peps)
            {
            var pecas = DLM.painel.Consultas.GetPecasReal(new List<string> { s }).ToList();
                if(pecas.Count>0)
                {
                    DLM.painel.Consultas.MatarExcel(false);
                    ZPPCOOISN(destino,s, pecas,buffer);
                    Retorno.AddRange(pecas);
                }
                w.somaProgresso(Pedido + " ZPPCOOISN " + s);
            }
            w.Close();
            return Retorno;
        }
        private static void ZPPCOOISN(string dest, string Pedido, List<DLM.painel.PLAN_PECA> retorno, bool buffer = false)
        {
            if (dest != "")
            {
                string arq = Pedido.Replace("*", "").Replace("%", "") + Cfg.Init.SAP_ZPPCOOISNARQ;
                Consulta p = new Consulta();
                if (!buffer)
                {
                    if (p.ZPPCOOISN(Pedido, dest, arq, false, "/SISTEMA"))
                    {
                        //w.SetProgresso(3, 5, "COOISN - Gravando Status...");
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
            string ret   = "";
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
        private static void Carregar_ZPPCOOISN_Layout(string Pedido, List<PLAN_PECA> pecas = null, string dest = null, string arq = null)
        {
            if (Pedido.Length < 6) { return; }
            if (dest == null) { dest = Conexoes.Utilz.CriarPasta(Cfg.Init.DIR_APP, "SAP"); }
            if (arq == null) { arq = Pedido.Replace("*", "").Replace("%", "") + Cfg.Init.SAP_ZPPCOOISNARQ; }
            if (pecas == null) { pecas = DLM.painel.Consultas.GetPecasReal(new List<string> { Pedido }).FindAll(x => x.material.ToString().StartsWith("31")).ToList(); }

            if (pecas.Count == 0) { return; }

            if (File.Exists(dest + arq))
            {
                DLM.painel.Consultas.LimparCOOISN(Pedido);
                var arquivo = dest + arq;
                var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
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
                            /*18*/x[(int)TAB_ZPPCOOISN.OPERACAO].ToString(),
                            /*19*/x[(int)TAB_ZPPCOOISN.MARCA].ToString(),
                            /*20*/TratarPEP(x[(int)TAB_ZPPCOOISN.ELEMENTO_PEP].ToString()),
                            /*21*/"",
                            /*22*/x[(int)TAB_ZPPCOOISN.DESENHO_2].ToString(),
                        }).Distinct().ToList();
                //w.SetProgresso(1, retorno.Count());
                var min = Cfg.Init.DataDummy();
                var pcs = pecas.FindAll(x => x.material.ToString().StartsWith("31"));
                List<DLM.db.Linha> linhas = new List<DLM.db.Linha>();
                foreach (var tl in pcs)
                {


                    var curr = listacooisn.FindAll(x => tl.material.ToString() == x[0].ToString()
                    |
                    (tl.marca == x[18].ToString() && tl.PEP == x[18].ToString())
                    );
                    //ordena pela operação
                    curr = curr.OrderBy(x => x[18].ToString()).ToList();

                    var curr_marca = curr.Find(x => Conexoes.Utilz.Int(x[18]) == 10);
                    var curr_submateriais = new List<List<object>>();
                    if (curr_marca != null)
                    {
                        curr_submateriais = listacooisn.FindAll(x => x[13].ToString() == curr_marca[13].ToString() && x[4].ToString() == curr_marca[4].ToString() && x[0].ToString() != curr_marca[0].ToString());

                    }
                    curr.AddRange(curr_submateriais);
                    curr = curr.OrderBy(x => Conexoes.Extensoes.Data(x[5].ToString())).ToList();
                    //08/04/2020
                    curr = curr.OrderBy(x => x[18].ToString()).ToList();
                    //var  curr2 = curr.FindAll(x => Conexoes.Conexoes.Extensoes.Data(x[5].ToString()) > Cfg.Init.DataDummy());
                    if (curr.Count > 0)
                    {
                        var Dts = curr.Select(x => Conexoes.Extensoes.Data(x[5].ToString())).OrderBy(x => x).ToList().FindAll(x => x > Cfg.Init.DataDummy());
                        //Dts = Dts.OrderBy(x => x).ToList();

                        var inicio = Cfg.Init.DataDummy();
                        var fim = Cfg.Init.DataDummy();
                        string ultimo_status = "";
                        if (Dts.Count > 0)
                        {
                            inicio = Dts.Min();
                            fim = Dts.Max();
                        }
                        for (int i = 0; i < curr.Count - 1; i++)
                        {
                            var data = Conexoes.Extensoes.Data(curr[i][5].ToString());
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
                        if (Conexoes.Extensoes.Data(curr.Last()[5].ToString()) > Cfg.Init.DataDummy())
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

                        pdfs.AddRange(curr.Select(x => x[22].ToString()).ToList());
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

                        var valores = new List<DLM.db.Celula>();
                        if(denominstand!="")
                        {
                        valores.Add(new DLM.db.Celula("DENOMINDSTAND", denominstand));
                        }
                        if(desenho!="")
                        {
                        valores.Add(new DLM.db.Celula("DESENHO_1", desenho));
                        }
                        if (tipo_pintura != "")
                        {
                        valores.Add(new DLM.db.Celula("TIPO_DE_PINTURA", tipo_pintura));
                        }
                        if(inicio>min)
                        {
                        valores.Add(new DLM.db.Celula("DATA_INICIO", inicio > min ? inicio.ToShortDateString() : ""));
                        }
                        if(fim>min)
                        {
                        valores.Add(new DLM.db.Celula("DATA_FIM", fim > min ? fim.ToShortDateString() : ""));
                        }

                        valores.Add(new DLM.db.Celula("ULTIMO_STATUS", ultimo_status));
                        if(aco!="")
                        {
                        valores.Add(new DLM.db.Celula("TIPO_ACO", aco));
                        }
                        if (codigo_materia_prima != "")
                        {
                        valores.Add(new DLM.db.Celula("CODIGO_MATERIA_PRIMA_SAP", codigo_materia_prima));
                        }
                        if (curr_marca != null)
                        {
                            double esp = Conexoes.Utilz.Double(curr_marca[15]);
                            int furos = Conexoes.Utilz.Int(curr_marca[12]);
                            string marca = curr_marca[13].ToString();
                            double corte = Conexoes.Utilz.Double(curr_marca[8]);

                            if (curr_submateriais.Count > 0)
                            {
                                if (furos == 0)
                                {
                                    furos = curr_submateriais.Select(x => Conexoes.Utilz.Int(x[12])).Max();
                                }

                                esp = Conexoes.Utilz.Double(curr_submateriais[0][15]);


                                if (tl.grupo_mercadoria.Contains("PURLIN"))
                                {
                                    corte = curr_submateriais.Select(x => Conexoes.Utilz.Double(x[8])).Max();
                                }

                            }

                            var superficie = Conexoes.Utilz.Double(curr_marca[11]);
                            var comprimento = Conexoes.Utilz.Double(curr_marca[9]);
                            /*04/04/2019 - novas caracteristicas adicionadas*/
                            if(corte>0)
                            {
                            valores.Add(new DLM.db.Celula("CORTE_LARGURA", corte));
                            }

                            if(comprimento>0)
                            {
                            valores.Add(new DLM.db.Celula("COMPRIMENTO", comprimento));
                            }
                            valores.Add(new DLM.db.Celula("ESQ_DE_PINTURA", curr_marca[10]));
                            if(superficie>0)
                            {
                            valores.Add(new DLM.db.Celula("SUPERFICIE", superficie));
                            }
                            if(furos>0)
                            {
                            valores.Add(new DLM.db.Celula("FURACOES", furos));
                            }
                            if(marca!="")
                            {
                            valores.Add(new DLM.db.Celula("MARCA", marca));
                            }

                            if(esp>0)
                            {
                            valores.Add(new DLM.db.Celula("ESPESSURA", esp));
                            }


                        }

                        valores.Add(new DLM.db.Celula("material", tl.material));
                        valores.Add(new DLM.db.Celula("pep", tl.PEP));

                        linhas.Add(new DLM.db.Linha(valores));
                    }
                }
                DBases.GetDB().Cadastro(linhas, Cfg.Init.db_comum, "zppcooisn");
            }
        }

         public static List<CN47N_Datas> CN47N(string arquivo)
        {
            ConcurrentBag<CN47N_Datas> retorno = new ConcurrentBag<CN47N_Datas>();
            List<Task> Tarefas = new List<Task>();

            var t = Conexoes.Utilz.Excel.GetTabela(arquivo, true);
            foreach (var l in t.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new CN47N_Datas(l))));
            }
            Task.WaitAll(Tarefas.ToArray());

            return retorno.ToList();
        }

    }
}
