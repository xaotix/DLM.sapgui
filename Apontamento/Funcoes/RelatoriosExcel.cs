﻿using Conexoes;
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

  
        public static bool Exportar_Datas_Fabrica(List<PLAN_PEP> peps, string descricao, string pedido = "", string local = "", bool abrir = false, bool gerar_pendentes = false, string Destino = null)
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

                    var subetapas = peps.Select(x => x.subetapa).Distinct().ToList().OrderBy(X => X);
                    DateTime mindate = Cfg.Init.DataDummy();
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


                            var f2 = ps.Find(x => x.PEP.Contains(".F2"));
                            var f3 = ps.Find(x => x.PEP.Contains(".F3"));
                            var f4 = ps.Find(x => x.PEP.Contains(".F4"));
                            var fO = ps.Find(x => x.PEP.Contains(".FO"));
                            if (f2 != null)
                            {
                                int CA = 0;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f2.fabrica_cronograma_inicio > mindate ? f2.fabrica_cronograma_inicio: null;
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f2.fabrica_cronograma > mindate ? f2.fabrica_cronograma: null;
                                principal.Cells[l0 + l, c0 + CA + 3].Value = f2.peso_planejado;
                                principal.Cells[l0 + l, c0 + CA + 4].Value = f2.total_fabricado / 100;
                            }
                            if (f3 != null)
                            {
                                int CA = 4;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f3.fabrica_cronograma_inicio > mindate ? f3.fabrica_cronograma_inicio: null;
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f3.fabrica_cronograma > mindate ? f3.fabrica_cronograma: null;
                                principal.Cells[l0 + l, c0 + CA + 3].Value = f3.peso_planejado;
                                principal.Cells[l0 + l, c0 + CA + 4].Value = f3.total_fabricado / 100;



                            }
                            if (f4 != null)
                            {
                                int CA = 8;
                                principal.Cells[l0 + l, c0 + CA + 1].Value = f4.fabrica_cronograma_inicio > mindate ? f4.fabrica_cronograma_inicio: null;
                                principal.Cells[l0 + l, c0 + CA + 2].Value = f4.fabrica_cronograma > mindate ? f4.fabrica_cronograma: null;
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
                                resumo_pecas.Cells[l0 + l, c0 + 1].Value = p.PEP;
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
                                    avanco_unidade.Cells[l0 + l, c0 + 1].Value = pep.PEP;
                                    avanco_unidade.Cells[l0 + l, c0 + 2].Value = pep.peso_planejado;
                                    var wks = pep.GetPecas().Select(x => x.centro).Distinct().ToList();
                                    foreach(var wk in wks)
                                    {
                                        var wpcs = pep.GetPecas().FindAll(x => x.centro == wk);
                                            int c = 3;
                                        if(wk == Cfg.Init.CENTRO_NOB)
                                        {
                                           
                                        }
                                        else if (wk == Cfg.Init.CENTRO_SER)
                                        {
                                            c = 7;
                                        }
                                        else if (wk == Cfg.Init.CENTRO_CHA)
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
                    var mindia = Cfg.Init.DataDummy();
                    DateTime min = peps.FindAll(x => (DateTime)x.montagem_cronograma_inicio > mindia).Min(x => (DateTime)x.montagem_cronograma_inicio);
                    DateTime max = peps.Max(x => (DateTime)x.montagem_cronograma);
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
                    foreach (var subetapa in peps.OrderBy(x => x.montagem_cronograma_inicio))
                    {
                        try
                        {
                            var tipos = subetapa.GetPecas().Select(x => x.texto_breve + ";" + x.Complexidade + ";" + x.DENOMINDSTAND + ";" + x.TIPO_DE_PINTURA + ";" + x.unidade).Distinct().ToList();

                            foreach (var tip in tipos)
                            {
                                try
                                {
                                    var pcs = subetapa.GetPecas().FindAll(x => (x.texto_breve + ";" + x.Complexidade + ";" + x.DENOMINDSTAND + ";" + x.TIPO_DE_PINTURA + ";" + x.unidade) == tip);
                                    var qtd = pcs.Sum(x => x.qtd_necessaria);
                                    var peso = pcs.Sum(x => x.qtd_necessaria * x.peso_unitario);
                                    var pesofab = pcs.Sum(x => x.qtd_produzida * x.peso_unitario);
                                    var pesoemb = pcs.Sum(x => x.logistica.FindAll(y => y.carga_confirmada).Sum(z => z.quantidade * x.peso_unitario));
                                    var dias = ((DateTime)subetapa.montagem_cronograma - (DateTime)subetapa.montagem_cronograma_inicio).Days;
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
                                    principal.Cells[l0 + l, c0 + 1].Value = subetapa.subetapa;
                                    principal.Cells[l0 + l, c0 + 2].Value = tip.Split(';')[4];
                                    principal.Cells[l0 + l, c0 + 3].Value = tip.Split(';')[0];
                                    principal.Cells[l0 + l, c0 + 4].Value = mercadorias[0];
                                    principal.Cells[l0 + l, c0 + 5].Value = complexidade[0];
                                    principal.Cells[l0 + l, c0 + 6].Value = DENOMINDSTAND[0];
                                    principal.Cells[l0 + l, c0 + 7].Value = pintura[0];
                                    principal.Cells[l0 + l, c0 + 8].Value = (subetapa.fabrica_cronograma_inicio > mindia) ? subetapa.fabrica_cronograma_inicio: null;
                                    principal.Cells[l0 + l, c0 + 9].Value = (subetapa.fabrica_cronograma > mindia) ? subetapa.fabrica_cronograma: null;
                                    principal.Cells[l0 + l, c0 + 10].Value = (subetapa.logistica_cronograma_inicio > mindia) ? subetapa.logistica_cronograma_inicio: null;
                                    principal.Cells[l0 + l, c0 + 11].Value = (subetapa.logistica_cronograma > mindia) ? subetapa.logistica_cronograma : null;
                                    principal.Cells[l0 + l, c0 + 12].Value = (subetapa.montagem_cronograma_inicio > mindia) ? subetapa.montagem_cronograma_inicio: null;
                                    principal.Cells[l0 + l, c0 + 13].Value = (subetapa.montagem_cronograma > mindia) ? subetapa.montagem_cronograma: null;
                                    principal.Cells[l0 + l, c0 + 14].Value = subetapa.Montagem_Balanco ? "X" : "";
                                    principal.Cells[l0 + l, c0 + 15].Value = subetapa.Montagem_Balanco ? (subetapa.total_montado / 100) : 0;
                                    principal.Cells[l0 + l, c0 + 16].Value = qtd;
                                    principal.Cells[l0 + l, c0 + 17].Value = peso;
                                    principal.Cells[l0 + l, c0 + 18].Value = pesofab;
                                    principal.Cells[l0 + l, c0 + 19].Value = pesoemb;
                                    principal.Cells[l0 + l, c0 + 20].Value = qtd_dias;

                                    if (!subetapa.Montagem_Balanco)
                                    {
                                        principal.Cells[l0 + l, c0 + 15].Value = "";
                                    }

                                    if (subetapa.montagem_cronograma_inicio > mindia && dias > 0)
                                    {
                                        var ca = ((DateTime)subetapa.montagem_cronograma_inicio - (DateTime)min).Days;
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
           var destino = Conexoes.Utilz.SalvarArquivo("xlsx");
            if (destino == null) { return false; }


            RelatorioAvanco(item.GetPecas(), pecas? item.GetSubEtapas():new List<PLAN_SUB_ETAPA>(), destino, abrir);
            return File.Exists(destino);
        }
        public static bool RelatorioAvanco(List<PLAN_PECA> PECAS, List<PLAN_SUB_ETAPA> subetapas = null,string Destino = "", bool abrir = true)
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
            Destino = Conexoes.Utilz.SalvarArquivo("xlsx");

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
                    var mindia = Cfg.Init.DataDummy();
                     double at = 0;
                    var pedidosstr = PECAS.Select(x => x.pedido_completo).Distinct().ToList();
                    var tot = PECAS.Count;

                    /*PEÇAS*/
                    foreach (var t in PECAS.OrderBy(x => x.PEP))
                    {
                        double dif = (l / (double)tot) * 100;
                        try
                        {
                            pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                            pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = "";
                            pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido_completo;
                            pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.PEP;
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
                            pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.inicio > mindia ? t.inicio: null;
                            pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.fim > mindia ? t.fim: null;
                            pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.ultima_edicao;
                            pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.ULTIMO_STATUS;
                            pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.TIPO_DE_PINTURA;
                            pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.esq_de_pintura;
                            pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.Esquema.Getdescricao();
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
                        foreach (var sub in subetapas.OrderBy(x => x.etapa))
                        {
                            try
                            {
                                subetapas_aba_excel.Cells[l0 + l, c0 + 0].Value = sub.contrato;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 1].Value = "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 2].Value = sub.pedido;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 3].Value = sub.PEP;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 5].Value = sub.peso_planejado;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 6].Value = sub.liberado_engenharia / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 7].Value = sub.total_fabricado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 8].Value = sub.total_embarcado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 9].Value = sub.total_montado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 10].Value = sub.engenharia_cronograma_inicio > mindia ? sub.engenharia_cronograma_inicio: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 11].Value = sub.engenharia_cronograma > mindia ? sub.engenharia_cronograma: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 12].Value = sub.fabrica_cronograma_inicio > mindia ? sub.fabrica_cronograma_inicio: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 13].Value = sub.fabrica_cronograma > mindia ? sub.fabrica_cronograma: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 14].Value = sub.logistica_cronograma_inicio > mindia ? sub.logistica_cronograma_inicio: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 15].Value = sub.logistica_cronograma > mindia ? sub.logistica_cronograma: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 16].Value = sub.montagem_cronograma_inicio > mindia ? sub.montagem_cronograma_inicio: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 17].Value = sub.montagem_cronograma > mindia ? sub.montagem_cronograma: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 18].Value = sub.ultima_consulta_sap > mindia ? sub.ultima_consulta_sap: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 19].Value = sub.Montagem_Balanco ? "X" : "";
                                subetapas_aba_excel.Cells[l0 + l, c0 + 21].Value = sub.engenharia_liberacao > mindia ? sub.engenharia_liberacao: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 24].Value = sub.update_montagem.ToUpper().Replace("MONTAGEM: ","");
                                subetapas_aba_excel.Cells[l0 + l, c0 + 28].Value = sub.montagem_engenheiro;

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

                                pedidos_aba_excel.Cells[l0 + l, c0 + 0].Value = t.nome;
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
                        foreach (var t in subetapas.OrderBy(x => x.etapa).ToList())
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
                    //gravar_avanco_fabrica(subetapas, avanco_fabrica, descricoes);

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
        public static bool ExportarEmbarque(List<PLAN_PECA> Pecas, bool abrir = false, List<Logistica_Planejamento> log = null, string Destino = null, bool enviar_dbase = false, bool gera_excel = true)
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
            List<PLAN_PECA> orfas = new List<PLAN_PECA>();
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
                    DBases.GetDB().Apagar("pep", $"%{pc.contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas, false);     
                }
            }
            try
            {
                int l0 = 1;
                int l = 1;
                int c0 = 1;
              
                w.SetProgresso(1, Pecas.Count,"Mapeando peças...");
                var mindia = Cfg.Init.DataDummy();
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




                        foreach (var p in Pecas.OrderBy(x => x.PEP))
                        {
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
                                var qtd_a_embarcar = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                                var qtd_a_fabricar = p.qtd_necessaria - p.qtd_produzida;
                                var qtd_a_embarcar_produzida = p.qtd_produzida - (p.qtd_embarcada > 0 ? p.qtd_embarcada : 0);
                                var qtd_fab = qtd_a_fabricar <= 0 ? qtd_a_embarcar : qtd_a_embarcar_produzida;

                                var resto = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                                if (resto < 0)
                                {
                                    resto = 0;
                                }

                                if (qtd_a_embarcar > 0)
                                {
                                    AddLinha(l0, l, c0, mindia, pecas_aba_excel, p, new Logistica_Planejamento(), resto, qtd_fab, qtd_a_embarcar, qtd_fab * p.peso_unitario, 0, p.peso_unitario * qtd_a_embarcar);
                                    l++;
                                }

                               
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
                            w.somaProgresso();
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
                    foreach (var p in Pecas.OrderBy(x => x.PEP))
                    {
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
                            var qtd_a_embarcar = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                            var qtd_a_fabricar = p.qtd_necessaria - p.qtd_produzida;
                            var qtd_a_embarcar_produzida = p.qtd_produzida - (p.qtd_embarcada > 0 ? p.qtd_embarcada : 0);
                            var qtd_fab = qtd_a_fabricar <= 0 ? qtd_a_embarcar : qtd_a_embarcar_produzida;

                            var resto = p.qtd_necessaria - p.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                            if(resto<0)
                            {
                                resto = 0;
                            }

                            if (qtd_a_embarcar > 0)
                            {
                                DLM.db.Linha ldb = GetLinhaDB(mindia, new Logistica_Planejamento() { peca = p }, resto, qtd_fab, 0, qtd_a_embarcar);
                                linhas.Add(ldb);
                            }
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

                        w.somaProgresso();
                    }

                    DBases.GetDB().Cadastro(linhas, Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas);
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


            var Pecas = DBases.GetDB().Consulta($"SELECT * FROM {Cfg.Init.db_painel_de_obras2}.{Cfg.Init.tb_pecas} AS pr WHERE pr.pep LIKE '%{contrato}%'").Linhas;

            try
            {
                int l0 = 1;
                int l = 1;
                int c0 = 1;

                w.SetProgresso(1, Pecas.Count, "Mapeando peças...");
                var mindia = Cfg.Init.DataDummy();
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
                        pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = pc.Get("pep").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = pc.Get("centro").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = pc.Get("carreta").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = pc.Get("ordem").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = pc.Get("material").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = pc.Get("descricao").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = pc.Get("qtd_a_embarcar").Int(); //qtd a embarcar
                        pecas_aba_excel.Cells[l0 + l, c0 + 8].Value = qtd_carregada; //qtd carregada
                        pecas_aba_excel.Cells[l0 + l, c0 + 9].Value = pc.Get("placa").Valor; //placa
                        pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = pc.Get("motorista").Valor; //motorista
                        pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = pc.Get("marca").Valor; //marca
                        pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = pc["observacoes"].Valor; //observações
                        pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = qtd; //qtd
                        pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = qtd_fabricada; //qtd fabricada
                        //pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = pc.qtd_necessaria; //quantidade total necessária
                        pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = pc.Get("comprimento").Double(); //comprimento
                        pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = pc.Get("corte").Double(); //corte
                        pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = pc.Get("espessura").Double(); //espessura
                        pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = pc.Get("material").Valor; //material
                        pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = peso_unit; /*peso unitario*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = qtd *peso_unit;/*peso parcial nec*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = qtd_fabricada * peso_unit; //peso parcial fabricado
                        pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = peso_embarcado;  // peso embarcado
                        pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = (qtd * peso_unit) - (qtd_fabricada * peso_unit);//* - peso a produzir;
                        pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = qtd_a_embarcar * peso_unit;// - peso a embarcar*;
                        pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = pc.Get("mercadoria").Valor; //mercadoria
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
                        pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = pc.Get("status").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = pc.Get("pintura").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 34].Value = pc.Get("esquema").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = pc.Get("esquema_desc").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 36].Value = pc.Get("bobina").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 37].Value = pc.Get("face1").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 38].Value = pc.Get("face2").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 39].Value = pc.Get("complexidade").Valor; 
                        pecas_aba_excel.Cells[l0 + l, c0 + 40].Value = pc.Get("denominacao").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 41].Value = pc.Get("tipo").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 42].Value = pc.Get("arquivo").Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 43].Value = pc.Get("tipo_embarque").Valor;

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
        private static void AddLinha(int l0, int l, int c0, DateTime mindia, OfficeOpenXml.ExcelWorksheet pecas_aba_excel, PLAN_PECA peca, Logistica_Planejamento logistica, double qtd_necessaria, double qtd_fabricada, double qtd_a_embarcar, double peso_parcial_fabricado, double peso_embarcado, double peso_parcial_necessario)
        {
            pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = peca.PEP;
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
            pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = peca.Esquema.Getdescricao();
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
            ldb.Add("pep", t.peca.PEP);
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
            ldb.Add("esquema", t.peca.Esquema.ESQUEMA_COD);
            ldb.Add("esquema_desc", t.peca.Esquema.Getdescricao());
            ldb.Add("bobina", t.peca.bobina.SAP);
            ldb.Add("face1", t.peca.bobina.cor1.Nome);
            ldb.Add("face2", t.peca.bobina.cor2.Nome);
            ldb.Add("complexidade", t.peca.Complexidade);
            ldb.Add("denominacao", t.peca.DENOMINDSTAND);
            ldb.Add("tipo", t.peca.Tipo);
            ldb.Add("arquivo", t.peca.DESENHO_1);
            ldb.Add("tipo_embarque", t.peca.Tipo_Embarque);
            return ldb;
        }
        public static bool ExportarListaPecasPMP(Pacote_PMP pacote, bool abrir = false, bool gerar_subetapas = false, bool gerar_grupos_mercadoria = false, bool gerar_avanco = false, bool gerar_pedidos = false, bool gerar_pecas =true)
        {

            var template = Vars.TEMPLATE_SAIDA_PECAS_RESUMO_CONSOLIDADA;
            if (!File.Exists(template))
            {
                if (abrir)
                {
                    MessageBox.Show(template + "\n template não encontrado.");

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
                File.Copy(template, Destino);
            }
            catch (Exception EX)
            {
                if (abrir)
                {
                    MessageBox.Show(EX.Message);

                }
                return false;
            }

         var PECAS = new List<PLAN_PECA>();


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
                    var mindia = Cfg.Init.DataDummy();
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
                                foreach (var peca in pecas)
                                {
                                    var L1 = l0 + l;
                                    try
                                    {
                                        pecas_aba_excel.Cells[$"A{L1}"].Value = peca.contrato;
                                        pecas_aba_excel.Cells[$"B{L1}"].Value = "";
                                        pecas_aba_excel.Cells[$"C{L1}"].Value = peca.pedido_completo;
                                        pecas_aba_excel.Cells[$"D{L1}"].Value = peca.PEP;
                                        pecas_aba_excel.Cells[$"E{L1}"].Value = peca.desenho;
                                        pecas_aba_excel.Cells[$"F{L1}"].Value = peca.centro;
                                        pecas_aba_excel.Cells[$"G{L1}"].Value = peca.qtd_necessaria;
                                        pecas_aba_excel.Cells[$"H{L1}"].Value = peca.qtd_produzida;
                                        pecas_aba_excel.Cells[$"I{L1}"].Value = peca.qtd_embarcada>0?peca.qtd_embarcada:0;
                                        pecas_aba_excel.Cells[$"J{L1}"].Value = sub.Embarque.Necessario;
                                        pecas_aba_excel.Cells[$"K{L1}"].Value = sub.Embarque.Embarcado;
                                        pecas_aba_excel.Cells[$"L{L1}"].Value = peca.comprimento;
                                        pecas_aba_excel.Cells[$"M{L1}"].Value = peca.corte_largura;
                                        pecas_aba_excel.Cells[$"N{L1}"].Value = peca.espessura;
                                        pecas_aba_excel.Cells[$"O{L1}"].Value = peca.material;
                                        pecas_aba_excel.Cells[$"P{L1}"].Value = peca.texto_breve;
                                        pecas_aba_excel.Cells[$"Q{L1}"].Value = peca.peso_necessario;
                                        pecas_aba_excel.Cells[$"R{L1}"].Value = peca.peso_produzido;
                                        pecas_aba_excel.Cells[$"S{L1}"].Value = peca.peso_a_produzir;
                                        pecas_aba_excel.Cells[$"T{L1}"].Value = peca.grupo_mercadoria;
                                        pecas_aba_excel.Cells[$"U{L1}"].Value = sub.liberado_engenharia / 100;
                                        pecas_aba_excel.Cells[$"V{L1}"].Value = peca.fabricado_porcentagem;
                                        pecas_aba_excel.Cells[$"W{L1}"].Value = peca.embarcado_porcentagem;
                                        pecas_aba_excel.Cells[$"X{L1}"].Value = pep.ef>mindia?pep.ef:null;
                                        pecas_aba_excel.Cells[$"Y{L1}"].Value = pep.ff;
                                        pecas_aba_excel.Cells[$"Z{L1}"].Value = pep.li;
                                        pecas_aba_excel.Cells[$"AA{L1}"].Value = pep.mi;
                                        pecas_aba_excel.Cells[$"AB{L1}"].Value = sub.mi_s;
                                        pecas_aba_excel.Cells[$"AC{L1}"].Value = peca.inicio;
                                        pecas_aba_excel.Cells[$"AD{L1}"].Value = peca.fim;
                                        pecas_aba_excel.Cells[$"AE{L1}"].Value = peca.ultima_edicao;
                                        pecas_aba_excel.Cells[$"AF{L1}"].Value = peca.ULTIMO_STATUS;
                                        pecas_aba_excel.Cells[$"AG{L1}"].Value = peca.TIPO_DE_PINTURA;
                                        pecas_aba_excel.Cells[$"AH{L1}"].Value = peca.esq_de_pintura;
                                        pecas_aba_excel.Cells[$"AI{L1}"].Value = peca.Esquema.Getdescricao();
                                        pecas_aba_excel.Cells[$"AJ{L1}"].Value = peca.codigo_materia_prima_sap;
                                        pecas_aba_excel.Cells[$"AK{L1}"].Value = peca.bobina.cor1;
                                        pecas_aba_excel.Cells[$"AL{L1}"].Value = peca.bobina.cor2;
                                        pecas_aba_excel.Cells[$"AM{L1}"].Value = peca.tipo_aco;
                                        pecas_aba_excel.Cells[$"AN{L1}"].Value = peca.Complexidade;
                                        pecas_aba_excel.Cells[$"AO{L1}"].Value = peca.DENOMINDSTAND;
                                        pecas_aba_excel.Cells[$"AP{L1}"].Value = peca.Tipo.ToString();
                                        pecas_aba_excel.Cells[$"AQ{L1}"].Value = peca.arquivo;

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

                     

                        foreach (var subetapa in subetapas.OrderBy(x => x.etapa))
                        {
                            try
                            {
                                var L1 = l0 + l;


                                subetapas_aba_excel.Cells[$"A{L1}"].Value = subetapa.contrato;
                                subetapas_aba_excel.Cells[$"B{L1}"].Value = "";
                                subetapas_aba_excel.Cells[$"C{L1}"].Value = subetapa.pedido;
                                subetapas_aba_excel.Cells[$"D{L1}"].Value = subetapa.pep;
                                subetapas_aba_excel.Cells[$"E{L1}"].Value = subetapa.tipo.ToString();
                                //subetapas_aba_excel.Cells[$"F{L1}"].Value = subetapa.Real.resumo_pecas.etapa_bloqueada ? 1 : 0;
                                subetapas_aba_excel.Cells[$"G{L1}"].Value = subetapa.peso;
                                subetapas_aba_excel.Cells[$"H{L1}"].Value = subetapa.Real.liberado_engenharia / 100;
                                subetapas_aba_excel.Cells[$"I{L1}"].Value = subetapa.total_fabricado / 100;
                                subetapas_aba_excel.Cells[$"J{L1}"].Value = subetapa.total_embarcado / 100;
                                subetapas_aba_excel.Cells[$"K{L1}"].Value = subetapa.Embarque.Peso_Necessario;
                                subetapas_aba_excel.Cells[$"L{L1}"].Value = subetapa.Embarque.Peso_Embarcado;
                                subetapas_aba_excel.Cells[$"M{L1}"].Value = subetapa.Embarque.Porcentagem_Embarcado;
                                subetapas_aba_excel.Cells[$"N{L1}"].Value = subetapa.Real.total_montado / 100;
                                subetapas_aba_excel.Cells[$"O{L1}"].Value = subetapa.ei > mindia ? subetapa.ei : null;
                                subetapas_aba_excel.Cells[$"P{L1}"].Value = subetapa.ef > mindia ? subetapa.ef : null;
                                subetapas_aba_excel.Cells[$"Q{L1}"].Value = subetapa.fi > mindia ? subetapa.fi : null;
                                subetapas_aba_excel.Cells[$"R{L1}"].Value = subetapa.ff > mindia ? subetapa.ff : null;
                                subetapas_aba_excel.Cells[$"S{L1}"].Value = subetapa.li > mindia ? subetapa.li : null;
                                subetapas_aba_excel.Cells[$"T{L1}"].Value = subetapa.lf > mindia ? subetapa.lf : null;
                                subetapas_aba_excel.Cells[$"U{L1}"].Value = subetapa.mi_s > mindia ? subetapa.mi_s :null;
                                subetapas_aba_excel.Cells[$"V{L1}"].Value = subetapa.mf_s > mindia ? subetapa.mf_s :null;
                                subetapas_aba_excel.Cells[$"W{L1}"].Value = subetapa.mi > mindia ? subetapa.mf : null;
                                subetapas_aba_excel.Cells[$"X{L1}"].Value = subetapa.mf > mindia ? subetapa.mf : null;
                                subetapas_aba_excel.Cells[$"Y{L1}"].Value = subetapa.Real.ultima_consulta_sap > mindia ? subetapa.Real.ultima_consulta_sap : null;
                                //subetapas_aba_excel.Cells[$"Z{L1}"].Value = subetapa.Real.resumo_pecas.Inicio > mindia ? subetapa.Real.resumo_pecas.Inicio : null;
                                //subetapas_aba_excel.Cells[$"AA{L1}"].Value = subetapa.Real.resumo_pecas.Fim > mindia ? subetapa.Real.resumo_pecas.Fim : null;
                                subetapas_aba_excel.Cells[$"AB{L1}"].Value = subetapa.Real.update_montagem.ToUpper().Replace(" ","").Replace("MONTAGEM:","");
                                subetapas_aba_excel.Cells[$"AC{L1}"].Value = subetapa.Real.montagem_engenheiro;


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
                        var pedidos_sistema = pacote.Pedidos.FindAll(x=>x.Material_REAL).Select(x=>x.Real).ToList();

                        l0 = 2;
                        c0 = 1;
                        l = 1;
                        pedidos_sistema = pedidos_sistema.FindAll(x => pedss.FindAll(y => y == x.pedido) != null);
                        if (pedidos_sistema.Count > 0)
                        {
                            w = Conexoes.Utilz.Wait(pedidos_sistema.Count, "Carregando Pedidos...");
                        }

                        foreach (var pedido in pedidos_sistema.OrderBy(x => x.pedidos))
                        {
                            var L1 = l0 + l;
                            double dif = (l / (double)pedidos_sistema.Count()) * 100;
                            try
                            {

                                pedidos_aba_excel.Cells[$"A{L1}"].Value = pedido.nome;
                                pedidos_aba_excel.Cells[$"B{L1}"].Value = pedido.pedido;
                                pedidos_aba_excel.Cells[$"C{L1}"].Value = pedido.exportacao ? 1 : 0;
                                pedidos_aba_excel.Cells[$"D{L1}"].Value = pedido.peso_planejado;

                                pedidos_aba_excel.Cells[$"E{L1}"].Value = pedido.engenharia_previsto / 100;
                                pedidos_aba_excel.Cells[$"F{L1}"].Value = pedido.liberado_engenharia / 100;
                                pedidos_aba_excel.Cells[$"G{L1}"].Value = (pedido.engenharia_previsto / 100) - (pedido.liberado_engenharia / 100);

                                pedidos_aba_excel.Cells[$"H{L1}"].Value = pedido.fabrica_previsto / 100;
                                pedidos_aba_excel.Cells[$"I{L1}"].Value = pedido.total_fabricado / 100;
                                pedidos_aba_excel.Cells[$"J{L1}"].Value = (pedido.fabrica_previsto / 100) - (pedido.total_fabricado / 100);


                                pedidos_aba_excel.Cells[$"K{L1}"].Value = pedido.embarque_previsto / 100;
                                pedidos_aba_excel.Cells[$"L{L1}"].Value = pedido.total_embarcado / 100;
                                pedidos_aba_excel.Cells[$"M{L1}"].Value = (pedido.embarque_previsto / 100) - (pedido.total_embarcado / 100);

                                pedidos_aba_excel.Cells[$"N{L1}"].Value = pedido.montagem_previsto / 100;
                                pedidos_aba_excel.Cells[$"O{L1}"].Value = pedido.total_montado / 100;
                                pedidos_aba_excel.Cells[$"P{L1}"].Value = (pedido.montagem_previsto / 100) - (pedido.total_montado / 100);

                                pedidos_aba_excel.Cells[$"Q{L1}"].Value = pedido.status_montagem != "SEM APONTAMENTO" ? 1 : 0;
                                pedidos_aba_excel.Cells[$"R{L1}"].Value = pedido.montagem_engenheiro;





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



                    ///*MERCADORIAS*/
                    //if (subetapas != null && PECAS.Count > 0 && mercadorias_aba_excel != null && gerar_grupos_mercadoria)
                    //{
                    //    w = Conexoes.Utilz.Wait(subetapas.Count, "Criando Lista por Grupo de Mercadorias...");
                    //    l0 = 1;
                    //    l = 1;
                    //    foreach (var t in subetapas.OrderBy(x => x.etapa))
                    //    {
                    //        foreach (var pep in t.Getpeps())
                    //        {
                    //            foreach (var grupo in pep.GetGrupos_Mercadoria())
                    //            {
                    //                try
                    //                {

                    //                    if (grupo.Peso_Total == 0)
                    //                    {

                    //                    }
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 0].Value = grupo.contrato;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 1].Value = grupo.descricao_obra;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 2].Value = grupo.pedido_completo;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 3].Value = grupo.pep;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 4].Value = grupo.centro;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 5].Value = grupo.descricao;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 6].Value = grupo.Qtd_Necessaria;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 7].Value = grupo.Qtd_Produzida;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 8].Value = grupo.Qtd_Embarcada;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 9].Value = grupo.Peso_Total;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 10].Value = t.Real.liberado_engenharia / 100;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 11].Value = grupo.Total_Fabricado / 100;
                    //                    mercadorias_aba_excel.Cells[l0 + l, c0 + 12].Value = grupo.Total_Embarcado / 100;
                    //                    if (pep.Real.fabrica_cronograma > mindia)
                    //                    {
                    //                        mercadorias_aba_excel.Cells[l0 + l, c0 + 13].Value = pep.Real.fabrica_cronograma;

                    //                    }
                    //                    if (pep.Real.logistica_cronograma > mindia)
                    //                    {
                    //                        mercadorias_aba_excel.Cells[l0 + l, c0 + 14].Value = pep.Real.logistica_cronograma;

                    //                    }
                    //                }
                    //                catch (Exception)
                    //                {
                    //                }
                    //                w.somaProgresso();
                    //                l++;
                    //            }
                    //        }
                    //        w.somaProgresso();
                    //    }
                    //}
                    //else if(gerar_grupos_mercadoria)
                    //{
                    //    List<Grupo_Mercadoria> mercadorias = Classificadores.GetGrupo_Mercadorias(PECAS);


                    //    /*novo mercadorias*/
                    //    if (mercadorias.Count > 0 && mercadorias_aba_excel != null)
                    //    {
                    //        w = Conexoes.Utilz.Wait(mercadorias.Count, "Criando Lista por Grupo de Mercadorias...");
                    //        l0 = 1;
                    //        l = 1;
                    //        var L1 = l0 + l;
                    //        foreach (var grupo in mercadorias)
                    //        {
                    //            try
                    //            {
                    //                mercadorias_aba_excel.Cells[$"A{L1}"].Value = grupo.contrato;
                    //                mercadorias_aba_excel.Cells[$"B{L1}"].Value = grupo.descricao_obra;
                    //                mercadorias_aba_excel.Cells[$"C{L1}"].Value = grupo.pedido_completo;
                    //                mercadorias_aba_excel.Cells[$"D{L1}"].Value = grupo.pep;
                    //                mercadorias_aba_excel.Cells[$"E{L1}"].Value = grupo.centro;
                    //                mercadorias_aba_excel.Cells[$"F{L1}"].Value = grupo.descricao;
                    //                mercadorias_aba_excel.Cells[$"G{L1}"].Value = grupo.Qtd_Necessaria;
                    //                mercadorias_aba_excel.Cells[$"H{L1}"].Value = grupo.Qtd_Produzida;
                    //                mercadorias_aba_excel.Cells[$"I{L1}"].Value = grupo.Qtd_Embarcada;
                    //                mercadorias_aba_excel.Cells[$"J{L1}"].Value = grupo.Peso_Total;
                    //                if (grupo.Total_Fabricado > 0)
                    //                {
                    //                    mercadorias_aba_excel.Cells[$"L{L1}"].Value = 100;
                    //                }
                    //                if (grupo.Total_Fabricado > 0)
                    //                {
                    //                    mercadorias_aba_excel.Cells[$"L{L1}"].Value = grupo.Total_Fabricado / 100;
                    //                }
                    //                if (grupo.Total_Embarcado > 0)
                    //                {
                    //                    mercadorias_aba_excel.Cells[$"M{L1}"].Value = grupo.Total_Embarcado / 100;
                    //                }
                    //            }
                    //            catch (Exception)
                    //            {
                    //            }
                    //            w.somaProgresso();
                    //            l++;
                    //        }
                    //    }
                    //}


                    ///*AVANÇO FÁBRICA*/
                    //if (gerar_avanco)
                    //{
                    //    gravar_avanco_fabrica(subetapas.FindAll(x => x.Material_REAL).Select(x => x.Real).ToList(), avanco_fabrica, descricoes);
                    //}

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

                        //if(!gerar_avanco)
                        //{
                        //    avanco_fabrica.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        //}
                        //if(!gerar_grupos_mercadoria)
                        //{
                        //    mercadorias_aba_excel.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        //}
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
