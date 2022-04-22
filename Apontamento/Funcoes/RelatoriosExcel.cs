using DLM.vars;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DLM.painel
{
public class Relatorios
    {
        public static bool ExportarExcel(Resultado_Economico_Header ppheader, bool abrir, string Destino = null)
        {
            if(Destino == null)
            {
            Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            }

        revalidar:
            if (ppheader.faltam_dados_preenchidos)
            {
                if (Conexoes.Utilz.Pergunta("Faltam dados da obra para poder gerar a análise. Deseja preenche-los agora?"))
                {
                    Conexoes.Utilz.Propriedades(ppheader, true, true);
                    ppheader.Salvar();
                    goto revalidar;
                }
                else
                {
                    return false;
                }
            }
        retentar:
            if (!File.Exists(Vars.template_relatorio_economico))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.template_relatorio_economico + "\n template não encontrado.");

                }
                return false;
            }

            if (Destino == "")
            {
                return false;
            }
            

            var res = ppheader.GetResultado_Economico();
            if (res == null) { return false; }
           if(ppheader.SubHeaders.Count==0)
            {

            res.SetLancamentos();
            }


           if(res.datas_com_problema)
            {
                MessageBox.Show("Obra contém lançamentos sem datas de cronograma. É necessário ajustar o cadastro.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.template_relatorio_economico, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    if (abrir)
                    {
                        if (Conexoes.Utilz.Pergunta(EX.Message + "\n\n Tentar Novamente?"))
                        {
                            goto retentar;
                        }
                    }

                }
                return false;
            }


            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    var max = res.datas.Count * res.GetGrupos().Count;
                    var w = Conexoes.Utilz.Wait(max, "Gerando Planilha...");

                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet pl = pck.Workbook.Worksheets[1];
                    OfficeOpenXml.ExcelWorksheet av = pck.Workbook.Worksheets[2];
                    string Planilha = pl.Name;
                    pl.Workbook.CalcMode = OfficeOpenXml.ExcelCalcMode.Automatic;

                    var logfile = new FileInfo(@"c:\temp\logfile.txt");
                    pl.Workbook.FormulaParserManager.AttachLogger(logfile);
                    int col_contrato = 3;
                    int col_faturamento_direto = 4;
                    int col_saving = 5;
                    int col_mes = 6;
                    int col_total = col_mes + res.datas.Count + 0;
                    int col_saldo = col_mes + res.datas.Count + 1;
                    int col_av_eco = col_mes + res.datas.Count + 2;
                    int l_min = 5;
                    int l_max = 74;
                    int l_data = 5;
                    int l_data2 = 19;
                    var templatecol = pl.Cells[l_min, col_mes, l_max, col_mes];
                    var templatecol2 = pl.Cells[l_min, col_faturamento_direto, l_max, col_faturamento_direto];

                    var templeate_emp1 = pl.Cells[04,04];
                    var templeate_emp2 = pl.Cells[18,04];

                    pl.Cells[1, 2].Value = res.descricao;
                    pl.Cells[2, 2].Value = res.Pedido;
                    pl.Cells[4, 1].Value = "Cálculo de: " +res.ultima_edicao.ToShortDateString() + " - " + ppheader.user + " - v." + System.Windows.Forms.Application.ProductVersion;

                    /*copia o estilo das colunas para todas as datas necessárias*/
                    for (int i = 0; i < res.datas.Count; i++)
                    {
                        var col_atual = pl.Cells[l_min, col_mes + i, l_max, col_mes + i];
                        if (i > 0)
                        {
                            templatecol.Copy(col_atual);
                        }
                        w.somaProgresso();
                    }
                    /*coluna total*/
                    templatecol2.Copy(pl.Cells[l_min, col_total, l_max, col_total]);
                    /*coluna saldo*/
                    templatecol2.Copy(pl.Cells[l_min, col_saldo, l_max, col_saldo]);
                    /*coluna %Av. Eco.*/
                    templatecol.Copy(pl.Cells[l_min, col_av_eco, l_max, col_av_eco]);
                    pl.Cells[l_min, col_av_eco, l_max, col_av_eco].Style.Numberformat.Format = "#0\\.00%";
                    #region preenchimento contrato
                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            pl.Cells[gr.linha_previsto, col_contrato].Value = gr.contrato.previsto;
                            pl.Cells[gr.linha_realizado, col_contrato].Value = gr.contrato.realizado;
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {

                        }
      
                    }
                    #endregion

                    var col_empenhado = 0;
                    /*preenchimento datas*/
                    for (int i = 0; i < res.datas.Count; i++)
                    {
                        /*preenche o campo data*/
                        pl.Cells[l_data, col_mes + i].Value = res.datas[i];
                        pl.Cells[l_data2, col_mes + i].Value = res.datas[i];
                        
                        for (int a = 0; a < res.GetGrupos().Count; a++)
                        {
                            try
                            {
                                var gr = res.GetGrupos()[a];
                                var dt = gr.GetLancamento(res.datas[i]);
                                pl.Cells[gr.linha_previsto, col_mes + i].Value = dt.previsto;
                                pl.Cells[gr.linha_realizado, col_mes + i].Value = dt.realizado;

                                if (gr.GetLancamento(res.datas[i]).Tipo_Valor == DLM.sapgui.Tipo_Valor.Empenhado && col_empenhado == 0 && dt.ano > 2000)
                                {
                                    col_empenhado = col_mes + i;
                                }
                            }
                            catch (Exception)
                            {

                            }
                            w.somaProgresso();
                        }

                        try
                        {
                            // D17 - SOMA(D21; D23; D33; D35; D46; D48; D50)
                            //EBITDA
                            pl.Cells[res.Custos.ebitida.linha_previsto, col_mes + i].Value =
                                  Conexoes.Utilz.Double(pl.Cells[res.Receitas.Getreceita_liquida().linha_previsto, col_mes + i].Value)
                                  -
                                (Conexoes.Utilz.Double(pl.Cells[res.Custos.projeto.linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.Getmaterial().linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.logistica.linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.Getmontagem().linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.seguros.linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.terceirizacao_de_projeto.linha_previsto, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.demais_custos.linha_previsto, col_mes + i].Value))
                                ;

                            pl.Cells[res.Custos.ebitida.linha_realizado, col_mes + i].Value =
                                  Conexoes.Utilz.Double(pl.Cells[res.Receitas.Getreceita_liquida().linha_realizado, col_mes + i].Value)
                                  -
                                (Conexoes.Utilz.Double(pl.Cells[res.Custos.projeto.linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.Getmaterial().linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.logistica.linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.Getmontagem().linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.seguros.linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.terceirizacao_de_projeto.linha_realizado, col_mes + i].Value)
                                + Conexoes.Utilz.Double(pl.Cells[res.Custos.demais_custos.linha_realizado, col_mes + i].Value)
                                )
                                ;
                            // "D16-SUM(D20;D22;D32;D34;D45;D47;D49)";
                            //pl.Cells[pp.Custos.ebitida.linha_realizado, col_mes + i].Formula = "D17-SUM(D21;D23;D33;D35;D46;D48;D50)";
                        }
                        catch (Exception)
                        {
                        }
                    }


                    #region preenchimento total
                    /*preenche o cabeçalho*/
                    try
                    {
                        pl.Cells[l_data, col_total].Value = "Total";
                        pl.Cells[l_data2, col_total].Value = "Total";
                    }
                    catch (Exception)
                    {

                    }


                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            if (gr.titulo.ToUpper().Contains("KG")) { continue; }
                            pl.Cells[gr.linha_previsto, col_total].Formula =
                                "SUM(" + pl.Cells[gr.linha_previsto, col_mes].Address + ":" + pl.Cells[gr.linha_previsto, col_mes + res.datas.Count - 1].Address + ")";
                            pl.Cells[gr.linha_realizado, col_total].Formula =
                                "SUM(" + pl.Cells[gr.linha_realizado, col_mes].Address + ":" + pl.Cells[gr.linha_realizado, col_mes + res.datas.Count - 1].Address + ")";
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    #endregion

                    #region preenchimento saldo
                    /*preenche o cabeçalho*/
                    try
                    {
                        pl.Cells[l_data, col_saldo].Value = "Saldo";
                        pl.Cells[l_data2, col_saldo].Value = "Saldo";
                    }
                    catch (Exception)
                    {
                    }

                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            if (gr.titulo.ToUpper().Contains("KG")) { continue; }
                            pl.Cells[gr.linha_previsto, col_saldo].Value = "";
                            //pl.Cells[gr.linha_previsto, col_saldo].Formula =
                            //    "(" + pl.Cells[gr.linha_previsto, col_contrato].Address + "-" + pl.Cells[gr.linha_previsto, col_saldo - 1].Address + ")";
                            pl.Cells[gr.linha_realizado, col_saldo].Formula =
                                "(" + pl.Cells[gr.linha_previsto, col_contrato].Address + "-" + pl.Cells[gr.linha_previsto, col_faturamento_direto].Address + "-" + pl.Cells[gr.linha_previsto, col_saving].Address + "-" + pl.Cells[gr.linha_realizado, col_saldo - 1].Address + ")";
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    #endregion

                    #region preenchimento faturamento direto
                  
                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            if (gr.faturamento_direto == 0) { continue; }
                            pl.Cells[gr.linha_previsto, col_faturamento_direto].Value = gr.faturamento_direto;
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    #endregion

                    #region preenchimento saving

                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            if (gr.saving == 0) { continue; }
                            pl.Cells[gr.linha_previsto, col_saving].Value = gr.saving;
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    #endregion

                    #region preenchimento %av eco com fórmulas
                    /*preenche o cabeçalho*/

                    try
                    {
                        pl.Cells[l_data, col_av_eco].Value = "%Av Eco";
                        pl.Cells[l_data2, col_av_eco].Value = "%Av Eco";
                    }
                    catch (Exception)
                    {

                    }




                    for (int i = 0; i < res.GetGrupos().Count; i++)
                    {
                        try
                        {
                            var gr = res.GetGrupos()[i];
                            if (Math.Abs(gr.contrato.previsto) > 0)
                            {
                                if (gr.titulo.ToUpper().Contains("KG")) { continue; }
                                pl.Cells[gr.linha_previsto, col_av_eco].Value = "";
                                //pl.Cells[gr.linha_previsto, col_av_eco].Formula =
                                //    "(" + pl.Cells[gr.linha_previsto, col_total].Address + "/" + pl.Cells[gr.linha_previsto, col_contrato].Address + "*100)";
                                pl.Cells[gr.linha_realizado, col_av_eco].Formula =
                                    "(" + pl.Cells[gr.linha_realizado, col_total].Address + "/" + pl.Cells[gr.linha_previsto, col_contrato].Address + "*100)";
                                w.somaProgresso();
                            }
                        }
                        catch (Exception)
                        {
                        }
                      
                    }

                    #endregion

                    #region preenchimento header empenhado

                    if (col_empenhado > 0)
                    {
                        try
                        {
                            var col_atual1 = pl.Cells[4, col_empenhado, 4, col_empenhado];
                            templeate_emp1.Copy(col_atual1);
                            var col_atual2 = pl.Cells[18, col_empenhado, 18, col_empenhado];
                            templeate_emp2.Copy(col_atual2);
                            pl.Cells[4, col_empenhado].Value = "Empenhado";
                            pl.Cells[18, col_empenhado].Value = "Empenhado";
                        }
                        catch (Exception)
                        {

                        }
                    }
                    #endregion


                    var lancs = res.LancamentosAgrupados.SelectMany(x => x.SubLancamentos).ToList();
                    w.SetProgresso(1, lancs.Count, "Gravando Dados...");
                    for (int i = 0; i < lancs.Count; i++)
                    {
                        try
                        {
                            var p = lancs[i];
                            av.Cells[i + 2, 1].Value = p.ano;
                            av.Cells[i + 2, 2].Value = p.mes;
                            av.Cells[i + 2, 3].Value = p.dia;
                            av.Cells[i + 2, 4].Value = p.descricao;
                            av.Cells[i + 2, 5].Value = p.previsto;
                            av.Cells[i + 2, 6].Value = p.realizado;
                            av.Cells[i + 2, 7].Value = p.peso;
                            av.Cells[i + 2, 8].Value = p.Tipo_Lancamento.ToString();
                            av.Cells[i + 2, 9].Value = p.Tipo_Valor.ToString();
                            w.somaProgresso();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    try
                    {
                        OfficeOpenXml.ExcelWorksheet graf = pck.Workbook.Worksheets[3];
                        int l0a = 1;
                        int c0a = 1;
                        //grava os dados
                        //receita bruta


                        var larg = 400;
                        var altura = 400;
                        var Metas = res.Receitas.Getreceita_bruta().meses;
                        c0a = InserirGrafico("grafico1","Receita Bruta", pl, graf, l0a, c0a, res.colunas + 1, 10, larg, altura, Metas);
                        InserirGrafico("grafico2", "Avanço Engenharia",pl, graf, 5, 1,res.colunas+1, 30, larg, altura, res.Engenharia.peso.meses);
                        InserirGrafico("grafico3", "Avanço Fábrica", pl, graf, 10, 1, res.colunas + 1, 50, larg, altura, res.Fabricacao.peso.meses);
                        InserirGrafico("grafico4", "Avanço Logística", pl, graf, 15, 1, res.colunas + 1, 70, larg, altura, res.Logistica.peso.meses);
                    }
                    catch (Exception)
                    {


                    }
                    w.somaProgresso();

                    w.Close();

                    pl.Workbook.FormulaParserManager.DetachLogger();

                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {
                    if (Conexoes.Utilz.Pergunta(ex.Message + "\n\n Tentar Novamente?"))
                    {
                        goto retentar;
                    }
                }
            }



            return true;
        }
        private static int InserirGrafico(string nome, string titulo,OfficeOpenXml.ExcelWorksheet planilha, OfficeOpenXml.ExcelWorksheet planilha_grafico, int l0a, int c0a, int col, int lin, int larg, int altura, List<DLM.sapgui.Lancamento> Metas)
        {

            var lmin = l0a;
            var cmin = c0a;


            var totalp = Metas.Sum(x => x.previsto);
            planilha_grafico.Cells[l0a, c0a].Value = "Previsto";
            planilha_grafico.Cells[l0a + 1, c0a].Value = "Realizado";
            foreach (var s in Metas.OrderBy(x => x.datasys))
            {
                var atualp = Math.Round(Metas.FindAll(x => x.datasys <= s.datasys).Sum(x => x.previsto) / totalp * 100, 2);
                var atualr = Math.Round(Metas.FindAll(x => x.datasys <= s.datasys).Sum(x => x.realizado) / totalp * 100, 2);
                planilha_grafico.Cells[l0a, c0a + 1].Value = s.data;
                planilha_grafico.Cells[l0a + 1, c0a + 1].Value = atualp;
                planilha_grafico.Cells[l0a + 2, c0a + 1].Value = atualr;
                c0a++;
            }
            var n1 = planilha_grafico.Cells[lmin + 1, cmin + 1, lmin + 1, c0a].FullAddress;
            var n2 = planilha_grafico.Cells[lmin + 2, cmin + 1, lmin + 2, c0a].FullAddress;

            var header = planilha_grafico.Cells[lmin, cmin + 1, lmin, c0a].FullAddress;
            var myChart = planilha.Drawings.AddChart(nome, eChartType.Line);

            // Define series for the chart
            var series = myChart.Series.Add(n1, header);
            series.Header = "Previsto";
            var series2 = myChart.Series.Add(n2, header);
            series2.Header = "Realizado";
            myChart.Border.Fill.Color = System.Drawing.Color.Green;
            myChart.Title.Text = titulo;
            myChart.Legend.Position = eLegendPosition.Left;
            myChart.SetSize(larg, altura);
            myChart.ShowDataLabelsOverMaximum = false;
            myChart.YAxis.MaxValue = 100;
            // Add to 6th row and to the 6th column
            myChart.SetPosition(lin, 0, col, 0);
            return c0a;
        }
        public static bool Exportar_Avanco_Fabrica(List<PLAN_SUB_ETAPA> subetapas, string Destino, bool abrir = true)
        {
            if (!File.Exists(Vars.TEMPLATE_AVANCO_FABRICA))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_AVANCO_FABRICA + "\n template não encontrado.");

                }
                return false;
            }

           
            if (Destino == "")
            {
                Destino = Conexoes.Utilz.SalvarArquivo("XLSX", "SELECIONE O DESTINO");
            }

            if(Destino=="" |  Destino==null)
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_AVANCO_FABRICA, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }
            if (subetapas.Count == 0)
            {
                if (abrir)
                {
                    MessageBox.Show("Nenhuma Subetapa carregada no Pedido.");

                }
                return false;
            }

            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet principal = pck.Workbook.Worksheets[1];
                    string Planilha = principal.Name;
                    OfficeOpenXml.ExcelWorksheet status = pck.Workbook.Worksheets[2];

                    gravar_avanco_fabrica(subetapas, principal, status);

                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }



            return true;
        }
        private static void gravar_avanco_fabrica(List<PLAN_SUB_ETAPA> subetapas, OfficeOpenXml.ExcelWorksheet principal, OfficeOpenXml.ExcelWorksheet status)
        {
            if (subetapas == null) { return; }
            if (subetapas.Count == 0) { return; }
            if (principal != null)
            {
                int l0 = 2;
                int c0 = 1;
                int l = 0;
                var pds = subetapas.Select(x => x.pedido).Distinct().ToList();
                var pedidos = DLM.painel.Buffer.GetTitulosPedidos().FindAll(x => pds.Find(y=>y==x.CHAVE)!=null);

                var mindate = Conexoes.Utilz.Calendario.DataDummy();
                foreach (var sub in subetapas)
                {
                    var ped = pedidos.Find(x => x.CHAVE == sub.pedido);
                    if (ped == null)
                    {
                        ped = new Titulo_Planejamento();
                    }
                    foreach (var pep in sub.peps.FindAll(x => !x.resumo_pecas.etapa_bloqueada))
                    {
                        principal.Cells[l0 + l, c0 + 0].Value = ped.DESCRICAO;
                        principal.Cells[l0 + l, c0 + 1].Value = pep.pep;
                        principal.Cells[l0 + l, c0 + 2].Value = pep.status_eng;
                        principal.Cells[l0 + l, c0 + 3].Value = pep.status;
                        principal.Cells[l0 + l, c0 + 4].Value = pep.resumo_pecas.status_usuario_pep;
                        principal.Cells[l0 + l, c0 + 5].Value = pep.resumo_pecas.centro_producao;
                        principal.Cells[l0 + l, c0 + 6].Value = pep.peso_planejado;
                        if (sub.engenharia_cronograma > mindate)
                        {
                            principal.Cells[l0 + l, c0 + 7].Value = sub.engenharia_cronograma;
                        }
                        if (sub.engenharia_liberacao > mindate)
                        {
                            principal.Cells[l0 + l, c0 + 8].Value = sub.engenharia_liberacao;
                        }
                        if (pep.fabrica_cronograma > mindate)
                        {
                            principal.Cells[l0 + l, c0 + 9].Value = pep.fabrica_cronograma;
                        }
                        if (sub.resumo_pecas.fim > mindate && sub.total_fabricado > 99)
                        {
                            principal.Cells[l0 + l, c0 + 10].Value = sub.resumo_pecas.fim;
                        }
                        if (pep.logistica_cronograma > mindate)
                        {
                            principal.Cells[l0 + l, c0 + 11].Value = pep.logistica_cronograma;
                        }
                        if (sub.montagem_cronograma_inicio > mindate)
                        {
                            principal.Cells[l0 + l, c0 + 12].Value = sub.montagem_cronograma_inicio;
                        }
                        principal.Cells[l0 + l, c0 + 13].Value = sub.Montagem_Balanco ? 1 : 0;
                        principal.Cells[l0 + l, c0 + 14].Value = pep.total_fabricado / 100;
                        principal.Cells[l0 + l, c0 + 15].Value = pep.total_embarcado / 100;
                        principal.Cells[l0 + l, c0 + 16].Value = sub.total_montado / 100;


                        l++;
                    }
                }
            }

            if (status != null)
            {
                var t1 = subetapas.SelectMany(x => x.peps.Select(y => y.status)).ToList();
                t1.AddRange(subetapas.SelectMany(x => x.peps.Select(y => y.status_eng)).ToList());
                t1.AddRange(subetapas.SelectMany(x => x.peps.Select(y => y.resumo_pecas.status_usuario_pep)).ToList());
                t1 = t1.Distinct().ToList();
                var lista = DLM.painel.Consultas.GetStatus(t1);
                int l0a = 2;
                foreach (var t in lista)
                {
                    status.Cells[l0a, 1].Value = t.status;
                    status.Cells[l0a, 2].Value = t.descricao;
                    l0a++;
                }
            }
        }
        public static bool Exportar_Datas_Fabrica(List<PEP_PLAN> peps, string descricao, string pedido = "", string local = "", bool abrir = false, bool gerar_pendentes = false, string Destino = null)
        {
  
            if (!File.Exists(Vars.TEMPLATE_DATAS_FABRICA))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_DATAS_FABRICA + "\n template não encontrado.");

                }
                return false;
            }
            if(Destino==null)
            {
            Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            }
            if (Destino == "" | Destino == null)
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_DATAS_FABRICA, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }
            if (peps.Count == 0)
            {
                if (abrir)
                {
                    MessageBox.Show("Nenhuma Subetapa carregada no Pedido.");

                }
                return false;
            }

            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet principal = pck.Workbook.Worksheets[1];
                    OfficeOpenXml.ExcelWorksheet resumo_pecas = pck.Workbook.Worksheets[2];
                    OfficeOpenXml.ExcelWorksheet avanco_unidade = pck.Workbook.Worksheets[3];
                    string Planilha = principal.Name;


                    int l0 = 10;
                    int c0 = 2;
                    int l = 0;

                    var subetapas = peps.FindAll(x => !x.resumo_pecas.etapa_bloqueada).Select(x => x.subetapa).Distinct().ToList().OrderBy(X => X);
                    DateTime mindate = Conexoes.Utilz.Calendario.DataDummy();
                    principal.Cells[1, c0].Value = descricao;
                    principal.Cells[2, c0].Value = pedido;
                    principal.Cells[3, c0].Value = local;

                    foreach (var t in subetapas)
                    {
                        try
                        {
                            var ps = peps.FindAll(x => x.subetapa == t);
                            principal.Cells[l0 + l, c0 + 0].Value = t;

                            if (t.Length > 13)
                            {
                                principal.Cells[l0 + l, 1].Value = t.Substring(0, 13);
                            }

                            //principal.Cells[l0 + l, 13].Value = peps[0].montagem_cronograma;


                            var f2 = ps.Find(x => x.pep.Contains(".F2"));
                            var f3 = ps.Find(x => x.pep.Contains(".F3"));
                            var f4 = ps.Find(x => x.pep.Contains(".F4"));
                            var fO = ps.Find(x => x.pep.Contains(".FO"));
                            if (f2 != null)
                            {
                                int CA = 0;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f2.fabrica_cronograma_inicio > mindate ? f2.fabrica_cronograma_inicio.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f2.fabrica_cronograma > mindate ? f2.fabrica_cronograma.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 3].Value = f2.peso_planejado;
                                principal.Cells[l0 + l, c0 + CA + 4].Value = f2.total_fabricado / 100;
                            }
                            if (f3 != null)
                            {
                                int CA = 4;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f3.fabrica_cronograma_inicio > mindate ? f3.fabrica_cronograma_inicio.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f3.fabrica_cronograma > mindate ? f3.fabrica_cronograma.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 3].Value = f3.peso_planejado;
                                principal.Cells[l0 + l, c0 + CA + 4].Value = f3.total_fabricado / 100;



                            }
                            if (f4 != null)
                            {
                                int CA = 8;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f4.fabrica_cronograma_inicio > mindate ? f4.fabrica_cronograma_inicio.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f4.fabrica_cronograma > mindate ? f4.fabrica_cronograma.ToShortDateString() : "";
                                principal.Cells[l0 + l, c0 + CA + 3].Value = f4.peso_planejado;
                                principal.Cells[l0 + l, c0 + CA + 4].Value = f4.total_fabricado / 100;

                            }
                            if (fO != null)
                            {
                                int CA = 12;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = fO.peso_planejado;

                            }




                        }
                        catch (Exception)
                        {

                        }




                        l++;
                    }
                    if (resumo_pecas != null && gerar_pendentes)
                    {

                        l0 = 2;
                        l = 0;
                        c0 = 1;
                        var w = Conexoes.Utilz.Wait(peps.Count, "Gravando Peças...");
                        foreach (var p in peps)
                        {
                            foreach (var pc in p.GetPecas().FindAll(x => x.embarcado_porcentagem<1))
                            {
                                resumo_pecas.Cells[l0 + l, c0].Value = pedido;
                                resumo_pecas.Cells[l0 + l, c0 + 1].Value = p.pep;
                                resumo_pecas.Cells[l0 + l, c0 + 2].Value = pc.centro;
                                resumo_pecas.Cells[l0 + l, c0 + 3].Value = p.logistica_cronograma;
                                resumo_pecas.Cells[l0 + l, c0 + 4].Value = pc.qtd_necessaria;
                                resumo_pecas.Cells[l0 + l, c0 + 5].Value = pc.qtd_embarcada>0?pc.qtd_embarcada:0;
                                resumo_pecas.Cells[l0 + l, c0 + 6].Value = pc.desenho;
                                resumo_pecas.Cells[l0 + l, c0 + 7].Value = pc.material;
                                resumo_pecas.Cells[l0 + l, c0 + 8].Value = pc.texto_breve;
                                                               
                                l++;
                            }
                            w.somaProgresso();
                        }
                        w.Close();
                    }
                    else if (resumo_pecas != null && !gerar_pendentes)
                    {
                        resumo_pecas.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                    }


                    if(avanco_unidade!=null)
                    {
                        avanco_unidade.Cells[1, c0].Value = descricao;
                        avanco_unidade.Cells[2, c0].Value = pedido;
                        avanco_unidade.Cells[3, c0].Value = local;
                        l = 8;
                        foreach (var t in subetapas)
                        {
                            try
                            {
                                
                                var ps = peps.FindAll(x => x.subetapa == t);
                                foreach(var pep in ps)
                                {
                                    avanco_unidade.Cells[l0 + l, c0 + 0].Value = pep.Pedido;
                                    avanco_unidade.Cells[l0 + l, c0 + 1].Value = pep.pep;
                                    avanco_unidade.Cells[l0 + l, c0 + 2].Value = pep.peso_planejado;
                                    var wks = pep.GetPecas().Select(x => x.centro).Distinct().ToList();
                                    foreach(var wk in wks)
                                    {
                                        var wpcs = pep.GetPecas().FindAll(x => x.centro == wk);
                                            int c = 3;
                                        if(wk == "1202")
                                        {
                                           
                                        }
                                        else if (wk == "1203")
                                        {
                                            c = 7;
                                        }
                                        else if (wk == "1204")
                                        {
                                            c = 11;
                                        }

                                        int decimais = 3;
                                        if(pep.fabrica_cronograma_inicio>mindate)
                                        {
                                        avanco_unidade.Cells[l0 + l, c0 + c + 0].Value = pep.fabrica_cronograma_inicio;

                                        }
                                        if(pep.fabrica_cronograma>mindate)
                                        {
                                        avanco_unidade.Cells[l0 + l, c0 + c + 1].Value = pep.fabrica_cronograma;

                                        }
                                        avanco_unidade.Cells[l0 + l, c0 + c + 2].Value = Math.Round(wpcs.Sum(x => x.peso_necessario)/1000, decimais);
                                        avanco_unidade.Cells[l0 + l, c0 + c + 3].Value = Math.Round(wpcs.Sum(x => x.peso_produzido)) > 0 ? (Math.Round(wpcs.Sum(x => x.peso_produzido) /wpcs.Sum(x => x.peso_necessario),3)) : 0;
                                    }

                                    l++;
                                }




                            }
                            catch (Exception)
                            {

                            }


                        }
                    }

                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }



            return true;
        }

        public static bool ExportarExcel(List<PLAN_SUB_ETAPA> peps, string descricao, bool abrir = true)
        {
            if (!File.Exists(Vars.TEMPLATE_SAIDA_PECAS))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_SAIDA_PECAS + "\n template não encontrado.");

                }
                return false;
            }

            string Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            if (Destino == "")
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_SAIDA_PECAS, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }
            if (peps.Count == 0)
            {
                if (abrir)
                {
                    MessageBox.Show("Nenhum PEP carregado no Pedido.");

                }
                return false;
            }

            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet principal = pck.Workbook.Worksheets[1];
                    string Planilha = principal.Name;


                    int l0 = 7;
                    int c0 = 1;
                    int l = 0;
                    var mindia = Conexoes.Utilz.Calendario.DataDummy();
                    DateTime min = peps.FindAll(X => X.montagem_cronograma_inicio > mindia).Min(x => x.montagem_cronograma_inicio);
                    DateTime max = peps.Max(x => x.montagem_cronograma);
                    DateTime at = new DateTime(min.Ticks);
                    int c_data0 = 21;
                    int c_data = 21;
                    while (at < max)
                    {
                        principal.Cells[l0 + l - 2, c0 + c_data].Value = at.ToShortDateString();
                        at = at.AddDays(1);
                        c_data++;
                    }
                    var w = Conexoes.Utilz.Wait(peps.Count);
                    foreach (var t in peps.OrderBy(x => x.montagem_cronograma_inicio))
                    {
                        try
                        {
                            var tipos = t.GetPecas().Select(x => x.texto_breve + ";" + x.Complexidade + ";" + x.DENOMINDSTAND + ";" + x.TIPO_DE_PINTURA + ";" + x.unidade).Distinct().ToList();

                            foreach (var tip in tipos)
                            {
                                try
                                {
                                    var pcs = t.GetPecas().FindAll(x => (x.texto_breve + ";" + x.Complexidade + ";" + x.DENOMINDSTAND + ";" + x.TIPO_DE_PINTURA + ";" + x.unidade) == tip);
                                    var qtd = pcs.Sum(x => x.qtd_necessaria);
                                    var peso = pcs.Sum(x => x.qtd_necessaria * x.peso_unitario);
                                    var pesofab = pcs.Sum(x => x.qtd_produzida * x.peso_unitario);
                                    var pesoemb = pcs.Sum(x => x.logistica.FindAll(y => y.carga_confirmada).Sum(z => z.quantidade * x.peso_unitario));
                                    var dias = (t.montagem_cronograma - t.montagem_cronograma_inicio).Days;
                                    if (dias == 0)
                                    {
                                        dias = 1;
                                    }


                                    var qtd_dias = qtd / dias;
                                    if (qtd_dias < 0)
                                    {
                                        qtd_dias = qtd;
                                    }
                                    var mercadorias = pcs.Select(y => y.grupo_mercadoria).Distinct().ToList();
                                    var complexidade = pcs.Select(y => y.Complexidade).Distinct().ToList();
                                    var DENOMINDSTAND = pcs.Select(y => y.DENOMINDSTAND).Distinct().ToList();
                                    var pintura = pcs.Select(y => y.TIPO_DE_PINTURA).Distinct().ToList();

                                    principal.Cells[l0 + l, c0 + 0].Value = descricao;
                                    principal.Cells[l0 + l, c0 + 1].Value = t.subetapa;
                                    principal.Cells[l0 + l, c0 + 2].Value = tip.Split(';')[4];
                                    principal.Cells[l0 + l, c0 + 3].Value = tip.Split(';')[0];
                                    principal.Cells[l0 + l, c0 + 4].Value = mercadorias[0];
                                    principal.Cells[l0 + l, c0 + 5].Value = complexidade[0];
                                    principal.Cells[l0 + l, c0 + 6].Value = DENOMINDSTAND[0];
                                    principal.Cells[l0 + l, c0 + 7].Value = pintura[0];
                                    principal.Cells[l0 + l, c0 + 8].Value = (t.fabrica_cronograma_inicio > mindia) ? t.fabrica_cronograma_inicio.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 9].Value = (t.fabrica_cronograma > mindia) ? t.fabrica_cronograma.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 10].Value = (t.logistica_cronograma_inicio > mindia) ? t.logistica_cronograma_inicio.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 11].Value = (t.logistica_cronograma > mindia) ? t.logistica_cronograma.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 12].Value = (t.montagem_cronograma_inicio > mindia) ? t.montagem_cronograma_inicio.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 13].Value = (t.montagem_cronograma > mindia) ? t.montagem_cronograma.ToShortDateString() : "";
                                    principal.Cells[l0 + l, c0 + 14].Value = t.Montagem_Balanco ? "X" : "";
                                    principal.Cells[l0 + l, c0 + 15].Value = t.Montagem_Balanco ? (t.total_montado / 100) : 0;
                                    principal.Cells[l0 + l, c0 + 16].Value = qtd;
                                    principal.Cells[l0 + l, c0 + 17].Value = peso;
                                    principal.Cells[l0 + l, c0 + 18].Value = pesofab;
                                    principal.Cells[l0 + l, c0 + 19].Value = pesoemb;
                                    principal.Cells[l0 + l, c0 + 20].Value = qtd_dias;

                                    if (!t.Montagem_Balanco)
                                    {
                                        principal.Cells[l0 + l, c0 + 15].Value = "";
                                    }

                                    if (t.montagem_cronograma_inicio > mindia && dias > 0)
                                    {
                                        var ca = (t.montagem_cronograma_inicio - min).Days;
                                        if (ca >= 0)
                                        {


                                            for (int i = 0; i < dias; i++)
                                            {
                                                if (qtd_dias > 0)
                                                {

                                                    principal.Cells[l0 + l, c0 + ca + i + c_data0].Value = qtd_dias;
                                                }
                                                else if (i < qtd)
                                                {
                                                    principal.Cells[l0 + l, c0 + ca + i + c_data0].Value = 1;

                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                }




                                l++;
                            }



                        }
                        catch (Exception)
                        {

                        }


                        w.somaProgresso();

                        //l++;
                    }

                    w.Close();
                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }



            return true;
        }

        public static bool RelatorioAvanco(PLAN_BASE item, bool pecas = true, bool abrir = true )
        {
           var Destino = Conexoes.Utilz.SalvarArquivo("XLSX", "SELECIONE O DESTINO");
            if (Destino == ""| Destino==null)
            {
                return false;
            }


            RelatorioAvanco(item.GetPecas(), pecas? item.GetSubEtapas():new List<PLAN_SUB_ETAPA>(), Destino, abrir);
            return File.Exists(Destino);
        }
        public static bool RelatorioAvanco(List<Peca_Planejamento> PECAS, List<PLAN_SUB_ETAPA> subetapas = null,string Destino = "", bool abrir = true)
        {
            if (PECAS.Count == 0 && subetapas == null)
            {
                return false;
            }
            else if (PECAS.Count == 0 && subetapas != null)
            {
                if (subetapas.Count == 0)
                {
                    return false;
                }
            }
            if (!File.Exists(Vars.TEMPLATE_SAIDA_PECAS_RESUMO))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_SAIDA_PECAS_RESUMO + "\n template não encontrado.");

                }
                return false;
            }

            if(Destino=="" | Destino == null)
            {
            Destino = Conexoes.Utilz.SalvarArquivo("XLSX", "SELECIONE O DESTINO");

            }
            if (Destino == "")
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_SAIDA_PECAS_RESUMO, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }

            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet pecas_aba_excel = pck.Workbook.Worksheets[1];
                    OfficeOpenXml.ExcelWorksheet subetapas_aba_excel = pck.Workbook.Worksheets[2];
                    OfficeOpenXml.ExcelWorksheet pedidos_aba_excel = pck.Workbook.Worksheets[3];
                    OfficeOpenXml.ExcelWorksheet mercadorias_aba_excel = pck.Workbook.Worksheets[4];
                    OfficeOpenXml.ExcelWorksheet avanco_fabrica = pck.Workbook.Worksheets[5];
                    OfficeOpenXml.ExcelWorksheet descricoes = pck.Workbook.Worksheets[6];

                    string Planilha = pecas_aba_excel.Name;

                    int l0 = 1;
                    int l = 1;
                    int c0 = 1;
                    var w = Conexoes.Utilz.Wait(PECAS.Count,"Gerando Planilha...");
                    var mindia = Conexoes.Utilz.Calendario.DataDummy();
                     double at = 0;
                    var pedidosstr = PECAS.Select(x => x.pedido_completo).Distinct().ToList();
                    var peds_peps = DLM.painel.Buffer.GetTitulosPedidos().FindAll(x => pedidosstr.Find(y => x.CHAVE.Contains(y)) != null);
                    var tot = PECAS.Count;

                    /*PEÇAS*/
                    foreach (var t in PECAS.OrderBy(x => x.pep))
                    {
                        double dif = (l / (double)tot) * 100;
                        try
                        {
                            var ped = peds_peps.Find(x => x.CHAVE == t.pedido_completo);
                            if (ped == null)
                            {
                                ped = new Titulo_Planejamento();
                            }
                            pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                            pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.DESCRICAO;
                            pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido_completo;
                            pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.pep;
                            pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.centro;
                            pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.qtd_necessaria;
                            pecas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.qtd_produzida;
                            pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.qtd_embarcada>0?t.qtd_embarcada:0;
                            pecas_aba_excel.Cells[l0 + l, c0 + 8].Value = t.desenho;
                            if(t.comprimento>0)
                            {
                            pecas_aba_excel.Cells[l0 + l, c0 + 9].Value = t.comprimento;
                            }
                            if(t.corte_largura>0)
                            {
                            pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = t.corte_largura;
                            }
                            if(t.espessura>0)
                            {
                            pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = t.espessura;
                            }

                            pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = t.material;
                            pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = t.texto_breve;
                            pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = t.peso_necessario;
                            pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.grupo_mercadoria;
                            pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.fabricado_porcentagem;
                            pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.embarcado_porcentagem;
                            pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.inicio > mindia ? t.inicio.ToShortDateString() : "";
                            pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.fim > mindia ? t.fim.ToShortDateString() : "";
                            pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.ultima_edicao;
                            pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.ULTIMO_STATUS;
                            pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.TIPO_DE_PINTURA;
                            pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.esq_de_pintura;
                            pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.Esquema.descricao;
                            pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.codigo_materia_prima_sap;
                            pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.bobina.cor1;
                            pecas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.bobina.cor2;
                            pecas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.tipo_aco;
                            pecas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.Complexidade;
                            pecas_aba_excel.Cells[l0 + l, c0 + 30].Value = t.DENOMINDSTAND;
                            pecas_aba_excel.Cells[l0 + l, c0 + 31].Value = t.Tipo.ToString();
                            pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = t.arquivo;
                            pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = t.Tipo_Embarque;

                        }
                        catch (Exception)
                        {


                        }
                        if (dif - at > 2)
                        {
                            w.SetProgresso(l, tot);
                            at = dif;
                        }
                        l++;




                        //l++;
                    }

                    /*SUBETAPAS*/
                    if (subetapas != null && subetapas_aba_excel != null)
                    {
                        l0 = 1;
                        l = 1;
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Carregando SubEtapas...");
                        foreach (var t in subetapas.OrderBy(x => x.etapa))
                        {
                            try
                            {
                                var ped = DLM.painel.Buffer.GetTitulosPedidos().Find(x => x.CHAVE == t.pedido);
                                if (ped == null)
                                {
                                    ped = new Titulo_Planejamento();
                                }
                                subetapas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.DESCRICAO;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.pep;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.resumo_pecas.etapa_bloqueada ? 1 : 0;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.peso_planejado;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.liberado_engenharia / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.total_fabricado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_embarcado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 9].Value = t.total_montado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 10].Value = t.engenharia_cronograma_inicio > mindia ? t.engenharia_cronograma_inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 11].Value = t.engenharia_cronograma > mindia ? t.engenharia_cronograma.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 12].Value = t.fabrica_cronograma_inicio > mindia ? t.fabrica_cronograma_inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 13].Value = t.fabrica_cronograma > mindia ? t.fabrica_cronograma.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 14].Value = t.logistica_cronograma_inicio > mindia ? t.logistica_cronograma_inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.logistica_cronograma > mindia ? t.logistica_cronograma.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.montagem_cronograma_inicio > mindia ? t.montagem_cronograma_inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.montagem_cronograma > mindia ? t.montagem_cronograma.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.ultima_consulta_sap > mindia ? t.ultima_consulta_sap.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.Montagem_Balanco ? "X" : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.data_transsap > mindia ? t.data_transsap.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.engenharia_liberacao > mindia ? t.engenharia_liberacao.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.resumo_pecas.inicio > mindia ? t.resumo_pecas.inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.resumo_pecas.fim > mindia ? t.resumo_pecas.fim.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.update_montagem.ToUpper().Replace("MONTAGEM: ","");
                                subetapas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.engenharia_projetista;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.engenharia_calculista;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.engenharia_responsavel;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.montagem_engenheiro;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.almox_comprado ? "X" : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 30].Value = t.almox_comprado_data > mindia ? t.almox_comprado_data.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 31].Value = t.almox_comprado_user;

                            }
                            catch (Exception)
                            {


                            }
                            w.somaProgresso();
                            l++;
                        }

                    }

                    /*PEDIDOS*/
                    if (pedidos_aba_excel != null && subetapas != null)
                    {
                       
                        var peds = subetapas.Select(x => x.contrato).Distinct().ToList();
                        var pedss = subetapas.Select(x => x.pedido).Distinct().ToList();
                        //var pedidos_sistema = DLM.painel.Consultas.GetPedidos(peds);
                        var pedidos_sistema = DLM.painel.Consultas.GetPedidos(peds);

                        l0 = 2;
                        c0 = 1;
                        l = 1;
                        pedidos_sistema = pedidos_sistema.FindAll(x => pedss.FindAll(y => y == x.pedido) != null);
                        if(pedidos_sistema.Count>0)
                        {
                            w = Conexoes.Utilz.Wait(pedidos_sistema.Count, "Carregando Pedidos...");
                        }
                   
                        foreach (var t in pedidos_sistema.OrderBy(x => x.pedidos))
                        {
                            double dif = (l / (double)pedidos_sistema.Count()) * 100;
                            try
                            {

                                pedidos_aba_excel.Cells[l0 + l, c0 + 0].Value = t.Titulo.DESCRICAO;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 1].Value = t.pedido;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 2].Value = t.exportacao ? 1 : 0;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 3].Value = t.peso_planejado;

                                pedidos_aba_excel.Cells[l0 + l, c0 + 4].Value = t.engenharia_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 5].Value = t.liberado_engenharia / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 6].Value = (t.liberado_engenharia / 100) - (t.engenharia_previsto / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 7].Value = t.fabrica_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_fabricado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 9].Value = (t.total_fabricado / 100) -(t.fabrica_previsto / 100);


                                pedidos_aba_excel.Cells[l0 + l, c0 + 10].Value = t.embarque_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 11].Value = t.total_embarcado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 12].Value = (t.total_embarcado / 100) - (t.embarque_previsto / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 13].Value = t.montagem_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 14].Value = t.total_montado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 15].Value = (t.total_montado / 100) - (t.montagem_previsto / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 16].Value = t.status_montagem != "SEM APONTAMENTO" ? 1 : 0;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 17].Value = t.montagem_engenheiro;





                            }
                            catch (Exception)
                            {


                            }
                            if (dif - at > 1)
                            {
                                w.SetProgresso(l, peds.Count);
                                at = dif;
                            }
                            l++;


                            w.somaProgresso();

                            //l++;
                        }
                    }

                    /*MERCADORIAS*/
                    if (subetapas != null && PECAS.Count > 0 && mercadorias_aba_excel != null)
                    {
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Criando Lista por Grupo de Mercadorias...");
                        l0 = 1;
                        l = 1;
                        foreach (var t in subetapas.OrderBy(x => x.etapa).ToList().FindAll(x => !x.resumo_pecas.etapa_bloqueada))
                        {
                            foreach (var pep in t.peps)
                            {
                                foreach (var grupo in pep.GetGrupos_Mercadoria())
                                {
                                    try
                                    {

                                        if (grupo.Peso_Total == 0)
                                        {

                                        }
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 0].Value = grupo.contrato;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 1].Value = grupo.descricao_obra;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 2].Value = grupo.pedido_completo;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 3].Value = grupo.pep;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 4].Value = grupo.centro;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 5].Value = grupo.descricao;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 6].Value = grupo.Qtd_Necessaria;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 7].Value = grupo.Qtd_Produzida;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 8].Value = grupo.Qtd_Embarcada;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 9].Value = grupo.Peso_Total;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 10].Value = t.liberado_engenharia / 100;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 11].Value = grupo.Total_Fabricado / 100;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 12].Value = grupo.Total_Embarcado / 100;
                                        if (pep.fabrica_cronograma > mindia)
                                        {
                                            mercadorias_aba_excel.Cells[l0 + l, c0 + 13].Value = pep.fabrica_cronograma;

                                        }
                                        if (pep.logistica_cronograma > mindia)
                                        {
                                            mercadorias_aba_excel.Cells[l0 + l, c0 + 14].Value = pep.logistica_cronograma;

                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    w.somaProgresso();
                                    l++;
                                }
                            }
                            w.somaProgresso();
                        }
                    }
                    else
                    {
                        List<Grupo_Mercadoria> mercadorias = Classificadores.GetGrupo_Mercadorias(PECAS);


                        /*novo mercadorias*/
                        if (mercadorias.Count > 0 && mercadorias_aba_excel != null)
                        {
                            w = Conexoes.Utilz.Wait(mercadorias.Count, "Criando Lista por Grupo de Mercadorias...");
                            l0 = 1;
                            l = 1;
                            foreach (var grupo in mercadorias)
                            {
                                try
                                {
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 0].Value = grupo.contrato;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 1].Value = grupo.descricao_obra;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 2].Value = grupo.pedido_completo;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 3].Value = grupo.pep;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 4].Value = grupo.centro;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 5].Value = grupo.descricao;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 6].Value = grupo.Qtd_Necessaria;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 7].Value = grupo.Qtd_Produzida;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 8].Value = grupo.Qtd_Embarcada;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 9].Value = grupo.Peso_Total;
                                    //mercadorias_aba_excel.Cells[l0 + l, c0 + 10].Value = t.liberado_engenharia / 100;
                                    if(grupo.Total_Fabricado>0)
                                    {
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 11].Value = grupo.Total_Fabricado / 100;
                                    }
                                    if(grupo.Total_Embarcado>0)
                                    {
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 12].Value = grupo.Total_Embarcado / 100;
                                    }
                                    //if (pep.fabrica_cronograma > mindia)
                                    //{
                                    //    mercadorias_aba_excel.Cells[l0 + l, c0 + 13].Value = pep.fabrica_cronograma;

                                    //}
                                    //if (pep.logistica_cronograma > mindia)
                                    //{
                                    //    mercadorias_aba_excel.Cells[l0 + l, c0 + 14].Value = pep.logistica_cronograma;

                                    //}
                                }
                                catch (Exception)
                                {
                                }
                                w.somaProgresso();
                                l++;
                            }
                        }
                    }

                    /*AVANÇO FÁBRICA*/
                    gravar_avanco_fabrica(subetapas, avanco_fabrica, descricoes);

                    /*ESCONDE ABAS*/
                    try
                    {
                        if (subetapas == null)
                        {
                            if (pecas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                pecas_aba_excel.Select();
                            }
                            subetapas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            pedidos_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        else if (subetapas.Count == 0)
                        {
                            if (pecas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                pecas_aba_excel.Select();
                            }
                            subetapas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            pedidos_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }

                    

                        if (PECAS.Count == 0)
                        {
                            if (subetapas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                subetapas_aba_excel.Select();
                            }
                            pecas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            mercadorias_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                    }
                    catch (Exception)
                    {


                    }

                  

                    w.Close();
                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }



            return true;
        }
        public static bool ExportarEmbarque(List<Peca_Planejamento> Pecas, bool abrir = false, List<Logistica_Planejamento> log = null, string Destino = null, bool enviar_dbase = false, bool gera_excel = true)
        {
            if (Pecas.Count == 0)
            {
                return false;
            }

           
            if (!File.Exists(Vars.TEMPLATE_EMBARQUES) && gera_excel)
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_EMBARQUES + "\n template não encontrado.");

                }
                return false;
            }

            if(Destino==null && gera_excel)
            {
            Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            }
            if(Destino == null && gera_excel)
            {
                return false;
            }
            if (Destino == "")
            {
                return false;
            }
            if(gera_excel)
            {
                try
                {

                    if (File.Exists(Destino)) { File.Delete(Destino); };
                    File.Copy(Vars.TEMPLATE_EMBARQUES, Destino);
                }
                catch (Exception ex)
                {
                    if (abrir)
                    {
                        MessageBox.Show(ex.Message);

                    }
                    return false;
                }
            }
            string pedido = string.Join(", ", Pecas.Select(x => x.contrato).Distinct().ToList());
          
            var w = Conexoes.Utilz.Wait(10,$"Logística...{Pecas.Count} Peças do(s) pedido(s) {pedido}");
            List<Peca_Planejamento> orfas = new List<Peca_Planejamento>();
            if (log==null && Pecas.Count>0)
            {
               
            log = DLM.painel.Consultas.GetLogistica(null, Pecas, out orfas);
            }

            Pecas.AddRange(orfas);
            Pecas = Pecas.OrderBy(x => x.ToString()).ToList();
            if(enviar_dbase && Pecas.Count>0)
            {
                var pc = Pecas[0];
               
                if(pc.contrato.Length>0)
                {
                    
                Conexoes.DBases.GetDB().Comando($"delete from " +
                    $"painel_de_obras2.pecas WHERE " +
                    $"painel_de_obras2.pecas.pep LIKE '%{pc.contrato}%'");
                }
            }
            try
            {
                int l0 = 1;
                int l = 1;
                int c0 = 1;
              
                w.SetProgresso(1, Pecas.Count,"Mapeando peças...");
                var mindia = Conexoes.Utilz.Calendario.DataDummy();
                double at = 0;
                var tot = Pecas.Count;

                if (gera_excel)
                {
                    using (var pck = new OfficeOpenXml.ExcelPackage())
                    {
                        using (Stream stream = new FileStream(Destino,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         FileShare.ReadWrite))
                        {
                            pck.Load(stream);
                        }


                        OfficeOpenXml.ExcelWorksheet pecas_aba_excel = pck.Workbook.Worksheets[1];

                        string Planilha = pecas_aba_excel.Name;




                        foreach (var p in Pecas.OrderBy(x => x.pep))
                        {
                            double dif = (l / (double)tot) * 100;
                            double qtd_fabr = p.qtd_produzida;

                            if (p.logistica.Count == 0)
                            {
                                try
                                {
                                    AddLinha(l0, l, c0, mindia, pecas_aba_excel, p, new Logistica_Planejamento(),p.qtd_necessaria, p.qtd_produzida, 0, p.peso_produzido, p.peso_embarcado,p.peso_necessario);

                                    l++;
                                }
                                catch (Exception)
                                {
                                }
                            }
                            else
                            {
                                foreach (var t in p.logistica.FindAll(x=>x.carga_confirmada))
                                {
                                    try
                                    {
                                        double peso_parcial_fabricado = t.quantidade > 0 ? (t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade) : 0;
                                        double peso_embarcado = t.carga_confirmada ? (t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade) : 0;
                                        double peso_parcial_necessario = t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade;
                                        AddLinha(l0, l, c0, mindia, pecas_aba_excel,p, t, t.quantidade,t.quantidade,0, peso_parcial_fabricado, peso_embarcado, peso_parcial_necessario);
                                        l++;



                                    }
                                    catch (Exception)
                                    {


                                    }
                                }
                            }

                            var qtd_a_embarcar = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                            var qtd_a_fabricar = p.qtd_necessaria - p.qtd_produzida;
                            var qtd_a_embarcar_produzida = p.qtd_produzida - (p.qtd_embarcada>0?p.qtd_embarcada:0);
                            var qtd_fab = qtd_a_fabricar <= 0 ? qtd_a_embarcar : qtd_a_embarcar_produzida;

                            if (qtd_a_embarcar>0)
                            {
                                AddLinha(l0, l, c0, mindia, pecas_aba_excel, p, new Logistica_Planejamento(), qtd_a_embarcar, qtd_fab, qtd_a_embarcar, qtd_fab * p.peso_unitario, 0, p.peso_unitario * qtd_a_embarcar);
                                l++;
                            }

                            if (dif - at > 2)
                            {
                                w.SetProgresso(l, tot);
                                at = dif;
                            }
                        }
                        pck.SaveAs(new FileInfo(Destino));
                        if (abrir)
                        {

                            Process.Start(Destino);
                        }
                    }

                }

                if(enviar_dbase)
                {
                    List<DLM.db.Linha> linhas = new List<DLM.db.Linha>();
                    foreach (var p in Pecas.OrderBy(x => x.pep))
                    {
                        double dif = (l / (double)tot) * 100;
                        double qtd_fabr = p.qtd_produzida;

                        if (p.logistica.Count == 0)
                        {
                            try
                            {
                                var lp = new Logistica_Planejamento();
                                lp.peca = p;
                                DLM.db.Linha ldb = GetLinhaDB(mindia, lp, p.qtd_necessaria, p.qtd_produzida, 0, p.qtd_necessaria);
                                linhas.Add(ldb);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            foreach (var t in p.logistica.FindAll(x => x.carga_confirmada))
                            {
                                try
                                {
                                    double peso_parcial_fabricado = t.quantidade > 0 ? (t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade) : 0;
                                    double peso_embarcado = t.carga_confirmada ? (t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade) : 0;
                                    double peso_parcial_necessario = t.peca.peso_necessario / t.peca.qtd_necessaria * t.quantidade;
                                    DLM.db.Linha ldb = GetLinhaDB(mindia, t, t.quantidade, t.quantidade, t.quantidade,0);
                                    linhas.Add(ldb);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }

                        var qtd_a_embarcar = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                        var qtd_a_fabricar = p.qtd_necessaria - p.qtd_produzida;
                        var qtd_a_embarcar_produzida = p.qtd_produzida - (p.qtd_embarcada>0?p.qtd_embarcada:0);
                        var qtd_fab = qtd_a_fabricar <= 0 ? qtd_a_embarcar : qtd_a_embarcar_produzida;

                        if (qtd_a_embarcar > 0)
                        {
                            DLM.db.Linha ldb = GetLinhaDB(mindia, new Logistica_Planejamento() { peca = p }, qtd_a_embarcar,qtd_fab, 0, qtd_a_embarcar);
                            linhas.Add(ldb);
                        }

                        if (dif - at > 2)
                        {
                            w.SetProgresso(l, tot);
                            at = dif;
                        }
                    }

                    Conexoes.DBases.GetDB().Cadastro(linhas, "painel_de_obras2", "pecas");
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }


                w.Close();


            return true;
        }
        public static bool ExportarEmbarque(string contrato, string Destino, bool abrir = true)
        {
            if(contrato.Length<5)
            {
                return false;
            }
            if (!File.Exists(Vars.TEMPLATE_EMBARQUES))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_EMBARQUES + "\n template não encontrado.");

                }
                return false;
            }

            if (Destino == null)
            {
                Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            }
            if (Destino == null)
            {
                return false;
            }
            if (Destino == "")
            {
                return false;
            }
            try
            {

                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_EMBARQUES, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }
            var w = Conexoes.Utilz.Wait(10, "Consultando logística...");


            var Pecas = Conexoes.DBases.GetDB().Consulta($"SELECT * FROM {Cfg.Init.tb_painel_de_obras2}.pecas AS pr WHERE pr.pep LIKE '%{contrato}%'").Linhas;

            try
            {
                int l0 = 1;
                int l = 1;
                int c0 = 1;

                w.SetProgresso(1, Pecas.Count, "Mapeando peças...");
                var mindia = Conexoes.Utilz.Calendario.DataDummy();
                var tot = Pecas.Count;


                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet pecas_aba_excel = pck.Workbook.Worksheets[1];

                    string Planilha = pecas_aba_excel.Name;




                    foreach (var pc in Pecas)
                    {
                        double qtd = pc.Get("qtd").Int();
                        double peso_total = pc.Get("peso_tot").Double();
                        double peso_unit = 0;
                        var inicio = pc.Get("primeiro_apontamento_fab").Data();
                        var fim = pc.Get("ultimo_apontamento_fab").Data();
                        var atualizado = pc.Get("atualizado_em").Data();
                        var qtd_carregada = pc.Get("qtd_carregada").Int();
                        var qtd_fabricada = pc.Get("produzido").Int();
                        var qtd_a_embarcar = pc.Get("qtd_a_embarcar").Int();
                        if (qtd>0 && peso_total>0)
                        {
                            peso_unit = peso_total / qtd;
                        }
                        double peso_embarcado = qtd_carregada * peso_unit;
                        pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = pc.Get("pep").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = pc.Get("centro").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = pc.Get("carreta").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = pc.Get("ordem").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = pc.Get("material").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = pc.Get("descricao").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = pc.Get("qtd_a_embarcar").Int(); //qtd a embarcar
                        pecas_aba_excel.Cells[l0 + l, c0 + 8].Value = qtd_carregada; //qtd carregada
                        pecas_aba_excel.Cells[l0 + l, c0 + 9].Value = pc.Get("placa").ToString(); //placa
                        pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = pc.Get("motorista").ToString(); //motorista
                        pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = pc.Get("marca").ToString(); //marca
                        pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = pc.Get("observacoes").ToString(); //observações
                        pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = qtd; //qtd
                        pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = qtd_fabricada; //qtd fabricada
                        //pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = pc.qtd_necessaria; //quantidade total necessária
                        pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = pc.Get("comprimento").Double(); //comprimento
                        pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = pc.Get("corte").Double(); //corte
                        pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = pc.Get("espessura").Double(); //espessura
                        pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = pc.Get("material").ToString(); //material
                        pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = peso_unit; /*peso unitario*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = qtd *peso_unit;/*peso parcial nec*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = qtd_fabricada * peso_unit; //peso parcial fabricado
                        pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = peso_embarcado;  // peso embarcado
                        pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = (qtd * peso_unit) - (qtd_fabricada * peso_unit);//* - peso a produzir;
                        pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = qtd_a_embarcar * peso_unit;// - peso a embarcar*;
                        pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = pc.Get("mercadoria").ToString(); //mercadoria
                        //pecas_aba_excel.Cells[l0 + l, c0 + 27].Value = pc.total_fabricado / 100; //porcentagem total fabricada
                        //pecas_aba_excel.Cells[l0 + l, c0 + 28].Value = pc.total_embarcado / 100; //porcentagem total embarcada
                        if (inicio > mindia)
                        {
                            pecas_aba_excel.Cells[l0 + l, c0 + 29].Value = inicio;
                        }
                        if (fim > mindia)
                        {
                            pecas_aba_excel.Cells[l0 + l, c0 + 30].Value = fim;
                        }
                        pecas_aba_excel.Cells[l0 + l, c0 + 31].Value = atualizado;
                        pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = pc.Get("status").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = pc.Get("pintura").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 34].Value = pc.Get("esquema").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = pc.Get("esquema_desc").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 36].Value = pc.Get("bobina").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 37].Value = pc.Get("face1").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 38].Value = pc.Get("face2").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 39].Value = pc.Get("complexidade").ToString(); 
                        pecas_aba_excel.Cells[l0 + l, c0 + 40].Value = pc.Get("denominacao").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 41].Value = pc.Get("tipo").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 42].Value = pc.Get("arquivo").ToString();
                        pecas_aba_excel.Cells[l0 + l, c0 + 43].Value = pc.Get("tipo_embarque").ToString();

                        l++;
                    }
                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }



            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }


            w.Close();


            return true;
        }
        private static void AddLinha(int l0, int l, int c0, DateTime mindia, OfficeOpenXml.ExcelWorksheet pecas_aba_excel, Peca_Planejamento peca, Logistica_Planejamento logistica, double qtd_necessaria, double qtd_fabricada, double qtd_a_embarcar, double peso_parcial_fabricado, double peso_embarcado, double peso_parcial_necessario)
        {
            pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = peca.pep;
            pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = peca.centro;
            pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = logistica.num_carga;
            pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = logistica.pack_list;
            pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = peca.material;
            pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = peca.texto_breve;
            /*13/07/2020*/
            if(qtd_a_embarcar>0)
            {
                pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = qtd_a_embarcar;
            }
            if(logistica.carga_confirmada)
            {
            pecas_aba_excel.Cells[l0 + l, c0 + 8].Value = logistica.quantidade;
            }
            pecas_aba_excel.Cells[l0 + l, c0 + 9].Value = logistica.placa;
            pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = logistica.motorista;
            pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = peca.desenho;
            pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = logistica.observacoes;
            pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = qtd_necessaria;
            pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = qtd_fabricada;
            pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = peca.qtd_necessaria;
            pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = peca.comprimento;
            pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = peca.corte_largura;
            pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = peca.espessura;
            pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = peca.material;
            pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = peca.peso_necessario / peca.qtd_necessaria; /*peso unitario*/
            pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = peso_parcial_necessario;/*peso parcial nec*/
            pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = peso_parcial_fabricado; //peso parcial fabricado
            pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = peso_embarcado;  // peso embarcado
            pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = peso_parcial_necessario - peso_parcial_fabricado;//*;
            pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = peso_parcial_necessario - peso_embarcado;//*;
            pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = peca.grupo_mercadoria;
            pecas_aba_excel.Cells[l0 + l, c0 + 27].Value = peca.fabricado_porcentagem;
            pecas_aba_excel.Cells[l0 + l, c0 + 28].Value = peca.embarcado_porcentagem;
            if (peca.inicio > mindia)
            {
                pecas_aba_excel.Cells[l0 + l, c0 + 29].Value = peca.inicio;
            }
            if (peca.fim > mindia)
            {
                pecas_aba_excel.Cells[l0 + l, c0 + 30].Value = peca.fim;
            }
            pecas_aba_excel.Cells[l0 + l, c0 + 31].Value = peca.ultima_edicao;
            pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = peca.ULTIMO_STATUS;
            pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = peca.TIPO_DE_PINTURA;
            pecas_aba_excel.Cells[l0 + l, c0 + 34].Value = peca.esq_de_pintura;
            pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = peca.Esquema.descricao;
            pecas_aba_excel.Cells[l0 + l, c0 + 36].Value = peca.bobina.SAP;
            pecas_aba_excel.Cells[l0 + l, c0 + 37].Value = peca.bobina.cor1;
            pecas_aba_excel.Cells[l0 + l, c0 + 38].Value = peca.bobina.cor2;
            pecas_aba_excel.Cells[l0 + l, c0 + 39].Value = peca.Complexidade;
            pecas_aba_excel.Cells[l0 + l, c0 + 40].Value = peca.DENOMINDSTAND;
            pecas_aba_excel.Cells[l0 + l, c0 + 41].Value = peca.Tipo;
            pecas_aba_excel.Cells[l0 + l, c0 + 42].Value = peca.DESENHO_1;
            pecas_aba_excel.Cells[l0 + l, c0 + 43].Value = peca.Tipo_Embarque;
        }
        private static DLM.db.Linha GetLinhaDB(DateTime mindia, Logistica_Planejamento t, double qtd, double qtd_fab, double qtd_emb, double qtd_a_embarcar)
        {
            DLM.db.Linha ldb = new DLM.db.Linha();
            ldb.Add("pep", t.peca.pep);
            ldb.Add("centro", t.peca.centro);
            ldb.Add("carreta", t.num_carga);
            ldb.Add("ordem", t.pack_list);
            ldb.Add("material", t.peca.material);
            ldb.Add("descricao", t.peca.texto_breve);
            ldb.Add("qtd_embarque", "");
            ldb.Add("qtd_carregada", qtd_emb);
            ldb.Add("placa", t.placa);
            ldb.Add("motorista", t.motorista);
            ldb.Add("marca", t.peca.desenho);
            ldb.Add("observacoes", t.observacoes);
            ldb.Add("qtd", qtd);
            ldb.Add("produzido", qtd_fab);
            ldb.Add("embarcado", qtd_emb);
            ldb.Add("qtd_a_embarcar", qtd_a_embarcar);
            ldb.Add("comprimento", t.peca.comprimento);
            ldb.Add("corte", t.peca.corte_largura);
            ldb.Add("espessura", t.peca.espessura);
            /*05/08/2020 - ajuste para pegar somente o peso total da quantidade embarcada*/
            ldb.Add("peso_tot", t.peca.peso_necessario / t.peca.qtd_necessaria * qtd_emb);
            ldb.Add("mercadoria", t.peca.grupo_mercadoria);
            if (t.peca.inicio > mindia)
            {
                ldb.Add("primeiro_apontamento_fab", t.peca.inicio);
            }
            if (t.peca.fim > mindia)
            {
                ldb.Add("ultimo_apontamento_fab", t.peca.fim);
            }

            ldb.Add("atualizado_em", t.peca.ultima_edicao);
            ldb.Add("status_sap", t.peca.ULTIMO_STATUS);
            ldb.Add("pintura", t.peca.TIPO_DE_PINTURA);
            ldb.Add("esquema", t.peca.Esquema.CODIGO_ESQUEMA);
            ldb.Add("esquema_desc", t.peca.Esquema.descricao);
            ldb.Add("bobina", t.peca.bobina.SAP);
            ldb.Add("face1", t.peca.bobina.cor1.Nome);
            ldb.Add("face2", t.peca.bobina.cor2.Nome);
            ldb.Add("complexidade", t.peca.Complexidade);
            ldb.Add("denominacao", t.peca.DENOMINDSTAND);
            ldb.Add("tipo", t.peca.Tipo.ToString());
            ldb.Add("arquivo", t.peca.DESENHO_1);
            ldb.Add("tipo_embarque", t.peca.Tipo_Embarque.ToString());
            return ldb;
        }
        public static bool ExportarListaPecasPMP(Pacote_PMP pacote, bool abrir = false, bool gerar_subetapas = false, bool gerar_grupos_mercadoria = false, bool gerar_avanco = false, bool gerar_pedidos = false, bool gerar_pecas =true)
        {

            if (!File.Exists(Vars.TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA))
            {
                if (abrir)
                {
                    MessageBox.Show(Vars.TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA + "\n template não encontrado.");

                }
                return false;
            }

            string Destino = Biblioteca_Daniel.Arquivo_Pasta.salvar("XLSX", "SELECIONE O DESTINO");
            if (Destino == "")
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(Vars.TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }

          List<Peca_Planejamento>  PECAS = new List<Peca_Planejamento>();
            List<Titulo_Planejamento> peds_peps = new List<Titulo_Planejamento>();

            if (gerar_pecas && gerar_subetapas)
            {
                PECAS.AddRange(pacote.GetPecas());
                var pedidosstr = pacote.pedidos.Select(x=>x.pep).Distinct().ToList();
                peds_peps = DLM.painel.Buffer.GetTitulosPedidos().FindAll(x => pedidosstr.Find(y => x.CHAVE.Contains(y)) != null);
            }

            try
            {
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (Stream stream = new FileStream(Destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    OfficeOpenXml.ExcelWorksheet pecas_aba_excel = pck.Workbook.Worksheets[1];
                    OfficeOpenXml.ExcelWorksheet subetapas_aba_excel = pck.Workbook.Worksheets[2];
                    OfficeOpenXml.ExcelWorksheet pedidos_aba_excel = pck.Workbook.Worksheets[3];
                    OfficeOpenXml.ExcelWorksheet mercadorias_aba_excel = pck.Workbook.Worksheets[4];
                    OfficeOpenXml.ExcelWorksheet avanco_fabrica = pck.Workbook.Worksheets[5];
                    OfficeOpenXml.ExcelWorksheet descricoes = pck.Workbook.Worksheets[6];

                    string Planilha = pecas_aba_excel.Name;

                    int l0 = 1;
                    int l = 1;
                    int c0 = 1;
                    var w = Conexoes.Utilz.Wait(PECAS.Count);
                    var TOT = PECAS.Count;
                    var mindia = Conexoes.Utilz.Calendario.DataDummy();
                    double at = 0;


                    List<SubEtapa_PMP> subetapas = new List<SubEtapa_PMP>();
                    if(gerar_subetapas)
                    {
                        subetapas = pacote.Getsubetapas();
                    }

                    /*PEÇAS*/
                    if (gerar_pecas)
                    {
                        w = Conexoes.Utilz.Wait(subetapas.Count+1, "Gravando Peças...(Isso pode demorar)");
                        int cont = 1;
                        int max = subetapas.Count;
                        foreach (var sub in subetapas.OrderBy(x=>x.pep))
                        {
                            var peps = sub.Getpeps();
                            foreach (var pep in  peps)
                            {
                                var pecas = pep.Getpecas();
                                foreach (var t in pecas)
                                {
                                    //double dif = (l / (double)TOT) * 100;
                                    try
                                    {
                                        var ped = peds_peps.Find(x => x.CHAVE == t.pedido_completo);
                                        if (ped == null)
                                        {
                                            ped = new Titulo_Planejamento();
                                        }
                                        pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.DESCRICAO;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido_completo;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.pep;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.centro;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.qtd_necessaria;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.qtd_produzida;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.qtd_embarcada>0?t.qtd_embarcada:0;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 8].Value = t.desenho;
                                        if (t.comprimento > 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 9].Value = t.comprimento;
                                        }
                                        if (t.corte_largura > 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = t.corte_largura;
                                        }
                                        if (t.espessura > 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = t.espessura;
                                        }

                                        pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = t.material;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = t.texto_breve;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = t.peso_necessario;
                                        if (t.peso_produzido > 0)
                                        {
                                        pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.peso_produzido;

                                        }
                                        pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.peso_a_produzir;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.grupo_mercadoria;
                                        if (sub.liberado_engenharia> 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = sub.liberado_engenharia / 100;
                                        }
                                        if (t.total_fabricado > 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.fabricado_porcentagem;
                                        }
                                        if (t.total_embarcado > 0)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.embarcado_porcentagem;
                                        }
                                        if (pep.ef > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = pep.ef;
                                        }
                                        if (pep.ff > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = pep.ff;
                                        }
                                        if (pep.mi > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = pep.mi;
                                        }

                                        if (t.inicio > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.inicio;
                                        }
                                        if (t.fim > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.fim;
                                        }
                                        if (t.ultima_edicao > mindia)
                                        {
                                            pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.ultima_edicao;
                                        }
                                        pecas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.ULTIMO_STATUS;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.TIPO_DE_PINTURA;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.esq_de_pintura;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 30].Value = t.Esquema.descricao;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 31].Value = t.codigo_materia_prima_sap;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = t.bobina.cor1;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = t.bobina.cor2;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 34].Value = t.tipo_aco;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = t.Complexidade;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 36].Value = t.DENOMINDSTAND;
                                        pecas_aba_excel.Cells[l0 + l, c0 + 37].Value = t.Tipo.ToString();
                                        pecas_aba_excel.Cells[l0 + l, c0 + 38].Value = t.arquivo;

                                    }
                                    catch (Exception)
                                    {


                                    }
                                   
                                    l++;




                                    //l++;
                                }
                            }
                            cont++;
                            w.somaProgresso(sub.etapa + " - Gravando Peças...");
    
                        }

                    }


                    /*SUBETAPAS*/
                    if (subetapas_aba_excel != null && gerar_subetapas)
                    {
                        l0 = 1;
                        l = 1;
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Carregando SubEtapas...");

                     

                        foreach (var t in subetapas.OrderBy(x => x.etapa))
                        {
                            try
                            {
                                var ped = Buffer.GetTitulosPedidos().Find(x => x.CHAVE == t.pedido);
                                if (ped == null)
                                {
                                    ped = new Titulo_Planejamento();
                                }

                                subetapas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.DESCRICAO;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.pep;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.tipo.ToString();
                                subetapas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.Real.resumo_pecas.etapa_bloqueada ? 1 : 0;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.peso;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.Real.liberado_engenharia / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_fabricado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 9].Value = t.total_embarcado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 10].Value = t.Real.total_montado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 11].Value = t.ei > mindia ? t.ei.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 12].Value = t.ef > mindia ? t.ef.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 13].Value = t.fi > mindia ? t.fi.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 14].Value = t.ff > mindia ? t.ff.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.li > mindia ? t.li.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.lf > mindia ? t.lf.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.mi > mindia ? t.mi.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.mf > mindia ? t.mf.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.Real.ultima_consulta_sap > mindia ? t.Real.ultima_consulta_sap.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.Real.Montagem_Balanco ? "X" : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.Real.data_transsap > mindia ? t.Real.data_transsap.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.Real.engenharia_liberacao > mindia ? t.Real.engenharia_liberacao.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.Real.resumo_pecas.inicio > mindia ? t.Real.resumo_pecas.inicio.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.Real.resumo_pecas.fim > mindia ? t.Real.resumo_pecas.fim.ToShortDateString() : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.Real.update_montagem.ToUpper().Replace(" ","").Replace("MONTAGEM:","");
                                subetapas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.Real.engenharia_projetista;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.Real.engenharia_calculista;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.Real.engenharia_responsavel;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.Real.montagem_engenheiro;


                            }
                            catch (Exception)
                            {


                            }
                            w.somaProgresso();
                            l++;
                        }

                    }

                    /*PEDIDOS*/
                    if (pedidos_aba_excel != null && gerar_pedidos)
                    {

                        var peds = subetapas.Select(x => x.contrato).Distinct().ToList();
                        var pedss = subetapas.Select(x => x.pedido).Distinct().ToList();
                        //var pedidos_sistema = DLM.painel.Consultas.GetPedidos(peds);
                        var pedidos_sistema = pacote.pedidos.FindAll(x=>x.Material_REAL).Select(x=>x.Real).ToList();

                        l0 = 2;
                        c0 = 1;
                        l = 1;
                        pedidos_sistema = pedidos_sistema.FindAll(x => pedss.FindAll(y => y == x.pedido) != null);
                        if (pedidos_sistema.Count > 0)
                        {
                            w = Conexoes.Utilz.Wait(pedidos_sistema.Count, "Carregando Pedidos...");
                        }

                        foreach (var t in pedidos_sistema.OrderBy(x => x.pedidos))
                        {
                            double dif = (l / (double)pedidos_sistema.Count()) * 100;
                            try
                            {

                                pedidos_aba_excel.Cells[l0 + l, c0 + 0].Value = t.Titulo.DESCRICAO;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 1].Value = t.pedido;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 2].Value = t.exportacao ? 1 : 0;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 3].Value = t.peso_planejado;

                                pedidos_aba_excel.Cells[l0 + l, c0 + 4].Value = t.engenharia_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 5].Value = t.liberado_engenharia / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 6].Value = (t.engenharia_previsto / 100) - (t.liberado_engenharia / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 7].Value = t.fabrica_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_fabricado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 9].Value = (t.fabrica_previsto / 100) - (t.total_fabricado / 100);


                                pedidos_aba_excel.Cells[l0 + l, c0 + 10].Value = t.embarque_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 11].Value = t.total_embarcado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 12].Value = (t.embarque_previsto / 100) - (t.total_embarcado / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 13].Value = t.montagem_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 14].Value = t.total_montado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 15].Value = (t.montagem_previsto / 100) - (t.total_montado / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 16].Value = t.status_montagem != "SEM APONTAMENTO" ? 1 : 0;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 17].Value = t.montagem_engenheiro;





                            }
                            catch (Exception)
                            {


                            }
                            if (dif - at > 1)
                            {
                                w.SetProgresso(l, peds.Count);
                                at = dif;
                            }
                            l++;


                            w.somaProgresso();

                            //l++;
                        }
                    }

                    /*MERCADORIAS*/
                    if (subetapas != null && PECAS.Count > 0 && mercadorias_aba_excel != null && gerar_grupos_mercadoria)
                    {
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Criando Lista por Grupo de Mercadorias...");
                        l0 = 1;
                        l = 1;
                        foreach (var t in subetapas.OrderBy(x => x.etapa))
                        {
                            foreach (var pep in t.Getpeps())
                            {
                                foreach (var grupo in pep.GetGrupos_Mercadoria())
                                {
                                    try
                                    {

                                        if (grupo.Peso_Total == 0)
                                        {

                                        }
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 0].Value = grupo.contrato;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 1].Value = grupo.descricao_obra;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 2].Value = grupo.pedido_completo;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 3].Value = grupo.pep;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 4].Value = grupo.centro;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 5].Value = grupo.descricao;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 6].Value = grupo.Qtd_Necessaria;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 7].Value = grupo.Qtd_Produzida;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 8].Value = grupo.Qtd_Embarcada;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 9].Value = grupo.Peso_Total;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 10].Value = t.Real.liberado_engenharia / 100;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 11].Value = grupo.Total_Fabricado / 100;
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 12].Value = grupo.Total_Embarcado / 100;
                                        if (pep.Real.fabrica_cronograma > mindia)
                                        {
                                            mercadorias_aba_excel.Cells[l0 + l, c0 + 13].Value = pep.Real.fabrica_cronograma;

                                        }
                                        if (pep.Real.logistica_cronograma > mindia)
                                        {
                                            mercadorias_aba_excel.Cells[l0 + l, c0 + 14].Value = pep.Real.logistica_cronograma;

                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    w.somaProgresso();
                                    l++;
                                }
                            }
                            w.somaProgresso();
                        }
                    }
                    else if(gerar_grupos_mercadoria)
                    {
                        List<Grupo_Mercadoria> mercadorias = Classificadores.GetGrupo_Mercadorias(PECAS);


                        /*novo mercadorias*/
                        if (mercadorias.Count > 0 && mercadorias_aba_excel != null)
                        {
                            w = Conexoes.Utilz.Wait(mercadorias.Count, "Criando Lista por Grupo de Mercadorias...");
                            l0 = 1;
                            l = 1;
                            foreach (var grupo in mercadorias)
                            {
                                try
                                {
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 0].Value = grupo.contrato;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 1].Value = grupo.descricao_obra;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 2].Value = grupo.pedido_completo;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 3].Value = grupo.pep;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 4].Value = grupo.centro;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 5].Value = grupo.descricao;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 6].Value = grupo.Qtd_Necessaria;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 7].Value = grupo.Qtd_Produzida;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 8].Value = grupo.Qtd_Embarcada;
                                    mercadorias_aba_excel.Cells[l0 + l, c0 + 9].Value = grupo.Peso_Total;
                                    //mercadorias_aba_excel.Cells[l0 + l, c0 + 10].Value = t.liberado_engenharia / 100;
                                    if (grupo.Total_Fabricado > 0)
                                    {
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 11].Value = grupo.Total_Fabricado / 100;
                                    }
                                    if (grupo.Total_Embarcado > 0)
                                    {
                                        mercadorias_aba_excel.Cells[l0 + l, c0 + 12].Value = grupo.Total_Embarcado / 100;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                w.somaProgresso();
                                l++;
                            }
                        }
                    }


                    /*AVANÇO FÁBRICA*/
                    if(gerar_avanco)
                    {
                    gravar_avanco_fabrica(subetapas.FindAll(x => x.Material_REAL).Select(x => x.Real).ToList(), avanco_fabrica, descricoes);

                    }

                    /*ESCONDE ABAS*/
                    try
                    {
                        if (subetapas == null )
                        {
                            if (pecas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                pecas_aba_excel.Select();
                            }
                            subetapas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            pedidos_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        else if (subetapas.Count == 0)
                        {
                            if (pecas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                pecas_aba_excel.Select();
                            }
                            subetapas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            pedidos_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }



                        if (PECAS.Count == 0)
                        {
                            if (subetapas_aba_excel.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                subetapas_aba_excel.Select();
                            }
                            pecas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            mercadorias_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }

                        if(!gerar_avanco)
                        {
                            avanco_fabrica.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if(!gerar_grupos_mercadoria)
                        {
                            mercadorias_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if(!gerar_pedidos)
                        {
                            pedidos_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if(!gerar_subetapas)
                        {
                            subetapas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if (!gerar_pecas)
                        {
                            pecas_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                    }
                    catch (Exception)
                    {


                    }



                    w.Close();




                    pck.SaveAs(new FileInfo(Destino));
                    if (abrir)
                    {

                        Process.Start(Destino);
                    }
                }

            }
            catch (Exception ex)
            {
                if (abrir)
                {

                    MessageBox.Show(ex.Message);
                }
            }



            return true;
        }
    }
}
