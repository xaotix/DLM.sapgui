using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace DLM.painel
{
    public class Relatorios
    {

        public static bool RelatorioAvanco(PLAN_BASE item, bool pecas = true, bool abrir = true)
        {
            var destino = "xlsx".SalvarArquivo();
            if (destino == null) { return false; }


            RelatorioAvanco(item.GetPecas(), pecas ? item.GetSubEtapas() : new List<PLAN_SUB_ETAPA>(), destino, abrir);
            return File.Exists(destino);
        }
        public static bool RelatorioAvanco(List<PLAN_PECA> Pecas, List<PLAN_SUB_ETAPA> subetapas = null, string destino = "", bool abrir = true)
        {
            if (Pecas.Count == 0 && subetapas == null)
            {
                return false;
            }
            else if (Pecas.Count == 0 && subetapas != null)
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

            if (destino == "" | destino == null)
            {
                destino = "xlsx".SalvarArquivo();

            }
            if (destino == "")
            {
                return false;
            }
            try
            {
                if (File.Exists(destino)) { File.Delete(destino); };
                File.Copy(Vars.TEMPLATE_SAIDA_PECAS_RESUMO, destino);
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
                    using (Stream stream = new FileStream(destino,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite))
                    {
                        pck.Load(stream);
                    }


                    var pecas_aba_excel = pck.Workbook.Worksheets[1];
                    var subetapas_aba_excel = pck.Workbook.Worksheets[2];
                    var pedidos_aba_excel = pck.Workbook.Worksheets[3];
                    var mercadorias_aba_excel = pck.Workbook.Worksheets[4];
                    var avanco_fabrica = pck.Workbook.Worksheets[5];
                    var descricoes = pck.Workbook.Worksheets[6];

                    string Planilha = pecas_aba_excel.Name;

                    int l0 = 1;
                    int l = 1;
                    int c0 = 1;
                    var w = Conexoes.Utilz.Wait(Pecas.Count, "Gerando Planilha...");
                    var mindia = Cfg.Init.DataDummy;
                    double at = 0;
                    var pedidosstr = Pecas.Select(x => x.pedido_completo).Distinct().ToList();
                    var peds_peps = Consultas.GetPedidosContratos().FindAll(x => pedidosstr.Find(y => x.Contrato.Contains(y)) != null);
                    var tot = Pecas.Count;

                    /*PEÇAS*/
                    foreach (var t in Pecas.OrderBy(x => x.PEP))
                    {
                        double dif = (l / (double)tot) * 100;
                        try
                        {
                            var ped = peds_peps.Find(x => x.Contrato == t.contrato);
                            if (ped == null)
                            {
                                ped = new Plan_Ped_Contrato();
                            }
                            pecas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                            pecas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.Descricao;
                            pecas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido_completo;
                            pecas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.PEP;
                            pecas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.centro;
                            pecas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.qtd_necessaria;
                            pecas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.qtd_produzida;
                            pecas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.qtd_embarcada > 0 ? t.qtd_embarcada : 0;
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
                            pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.grupo_mercadoria;
                            pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.fabricado_porcentagem;
                            pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.embarcado_porcentagem;
                            pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.inicio > mindia ? t.inicio : null;
                            pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.fim > mindia ? t.fim : null;
                            pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.ultima_edicao;
                            pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.ULTIMO_STATUS;
                            pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.TIPO_DE_PINTURA;
                            pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.esq_de_pintura;
                            pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.Esquema.Getdescricao();
                            pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.codigo_materia_prima_sap;
                            pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.bobina.Cor1;
                            pecas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.bobina.Cor2;
                            pecas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.tipo_aco;
                            pecas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.Complexidade;
                            pecas_aba_excel.Cells[l0 + l, c0 + 30].Value = t.DENOMINDSTAND;
                            pecas_aba_excel.Cells[l0 + l, c0 + 31].Value = t.Tipo.ToString();
                            pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = ""; //arquivo
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
                                var ped = Consultas.GetPedidosContratos().Find(x => x.Contrato == t.contrato);
                                if (ped == null)
                                {
                                    ped = new Plan_Ped_Contrato();
                                }
                                subetapas_aba_excel.Cells[l0 + l, c0 + 0].Value = t.contrato;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 1].Value = ped.Descricao;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 2].Value = t.pedido;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 3].Value = t.PEP;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 4].Value = t.resumo_pecas.etapa_bloqueada ? 1 : 0;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 5].Value = t.peso_planejado;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 6].Value = t.liberado_engenharia / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 7].Value = t.total_fabricado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_embarcado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 9].Value = t.total_montado / 100;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 10].Value = t.engenharia_cronograma_inicio > mindia ? t.engenharia_cronograma_inicio : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 11].Value = t.engenharia_cronograma > mindia ? t.engenharia_cronograma : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 12].Value = t.fabrica_cronograma_inicio > mindia ? t.fabrica_cronograma_inicio : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 13].Value = t.fabrica_cronograma > mindia ? t.fabrica_cronograma : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 14].Value = t.logistica_cronograma_inicio > mindia ? t.logistica_cronograma_inicio : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 15].Value = t.logistica_cronograma > mindia ? t.logistica_cronograma : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 16].Value = t.montagem_cronograma_inicio > mindia ? t.montagem_cronograma_inicio : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 17].Value = t.montagem_cronograma > mindia ? t.montagem_cronograma : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 18].Value = t.ultima_consulta_sap > mindia ? t.ultima_consulta_sap : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 19].Value = t.Montagem_Balanco ? "X" : "";
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 20].Value = t.data_transsap > mindia ? t.data_transsap: null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 21].Value = t.engenharia_liberacao > mindia ? t.engenharia_liberacao : null;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 22].Value = t.resumo_pecas.Inicio > mindia ? t.resumo_pecas.Inicio : null;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 23].Value = t.resumo_pecas.Fim > mindia ? t.resumo_pecas.Fim : null;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 24].Value = t.update_montagem.ToUpper().Replace("MONTAGEM: ", "");
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 25].Value = t.engenharia_projetista;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 26].Value = t.engenharia_calculista;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 27].Value = t.engenharia_responsavel;
                                subetapas_aba_excel.Cells[l0 + l, c0 + 28].Value = t.montagem_engenheiro;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 29].Value = t.almox_comprado ? "X" : "";
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 30].Value = t.almox_comprado_data > mindia ? t.almox_comprado_data: null;
                                //subetapas_aba_excel.Cells[l0 + l, c0 + 31].Value = t.almox_comprado_user;

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
                        if (pedidos_sistema.Count > 0)
                        {
                            w = Conexoes.Utilz.Wait(pedidos_sistema.Count, "Carregando Pedidos...");
                        }

                        foreach (var t in pedidos_sistema.OrderBy(x => x.pedidos))
                        {
                            double dif = (l / (double)pedidos_sistema.Count()) * 100;
                            try
                            {

                                pedidos_aba_excel.Cells[l0 + l, c0 + 0].Value = t.descricao;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 1].Value = t.pedido;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 2].Value = t.exportacao ? 1 : 0;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 3].Value = t.peso_planejado;

                                pedidos_aba_excel.Cells[l0 + l, c0 + 4].Value = t.engenharia_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 5].Value = t.liberado_engenharia / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 6].Value = (t.liberado_engenharia / 100) - (t.engenharia_previsto / 100);

                                pedidos_aba_excel.Cells[l0 + l, c0 + 7].Value = t.fabrica_previsto / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 8].Value = t.total_fabricado / 100;
                                pedidos_aba_excel.Cells[l0 + l, c0 + 9].Value = (t.total_fabricado / 100) - (t.fabrica_previsto / 100);


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
                    if (subetapas != null && Pecas.Count > 0 && mercadorias_aba_excel != null)
                    {
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Criando Lista por Grupo de Mercadorias...");
                        l0 = 1;
                        l = 1;
                        foreach (var t in subetapas)
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
                       var mercadorias = Classificadores.GetGrupo_Mercadorias(Pecas);


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

                        if (Pecas.Count == 0)
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
                    pck.SaveAs(new FileInfo(destino));
                    if (abrir)
                    {
                        Process.Start(destino);
                    }
                }
            }
            catch (Exception ex)
            {
                if (abrir)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
            }



            return true;
        }

        public static bool ExportarEmbarque(string contrato, string Destino, bool abrir = true)
        {
            if (contrato.Length < 5)
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
                Destino = "xlsx".SalvarArquivo();
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


            var Pecas = DBases.GetDB().Consulta("pep",contrato,Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas,false);

            try
            {
                int l0 = 1;
                int l = 1;
                int c0 = 1;

                w.SetProgresso(1, Pecas.Count, "Mapeando peças...");
                var mindia = Cfg.Init.DataDummy;
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
                    var pecas_aba_excel = pck.Workbook.Worksheets[1];
                    var Planilha = pecas_aba_excel.Name;

                    foreach (var linha in Pecas)
                    {
                        double qtd = linha["qtd"].Int();
                        double peso_total = linha["peso_tot"].Double();
                        double peso_unit = 0;
                        var inicio = linha["primeiro_apontamento_fab"].DataNull();
                        var fim = linha["ultimo_apontamento_fab"].DataNull();
                        var atualizado = linha["atualizado_em"].DataNull();
                        var qtd_carregada = linha["qtd_carregada"].Int();
                        var qtd_fabricada = linha["produzido"].Int();
                        var qtd_a_embarcar = linha["qtd_a_embarcar"].Int();
                        if (qtd > 0 && peso_total > 0)
                        {
                            peso_unit = peso_total / qtd;
                        }
                        double peso_embarcado = qtd_carregada * peso_unit;
                        pecas_aba_excel.Cells[l0 + l, c0 + 0].Value =  linha["pep"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 1].Value =  linha["centro"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 2].Value =  linha["carreta"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 3].Value =  linha["ordem"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 4].Value =  linha["material"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 5].Value =  linha["descricao"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 7].Value =  linha["qtd_a_embarcar"].Int(); //qtd a embarcar
                        pecas_aba_excel.Cells[l0 + l, c0 + 8].Value =  qtd_carregada; //qtd carregada
                        pecas_aba_excel.Cells[l0 + l, c0 + 9].Value =  linha["placa"].Valor; //placa
                        pecas_aba_excel.Cells[l0 + l, c0 + 10].Value = linha["motorista"].Valor; //motorista
                        pecas_aba_excel.Cells[l0 + l, c0 + 11].Value = linha["marca"].Valor; //marca
                        pecas_aba_excel.Cells[l0 + l, c0 + 12].Value = linha["observacoes"].Valor; //observações
                        pecas_aba_excel.Cells[l0 + l, c0 + 13].Value = qtd; //qtd
                        pecas_aba_excel.Cells[l0 + l, c0 + 14].Value = qtd_fabricada; //qtd fabricada
                        //pecas_aba_excel.Cells[l0 + l, c0 + 15].Value = pc.qtd_necessaria; //quantidade total necessária
                        pecas_aba_excel.Cells[l0 + l, c0 + 16].Value = linha["comprimento"].Double(); //comprimento
                        pecas_aba_excel.Cells[l0 + l, c0 + 17].Value = linha["corte"].Double(); //corte
                        pecas_aba_excel.Cells[l0 + l, c0 + 18].Value = linha["espessura"].Double(); //espessura
                        pecas_aba_excel.Cells[l0 + l, c0 + 19].Value = linha["material"].Valor; //material
                        pecas_aba_excel.Cells[l0 + l, c0 + 20].Value = peso_unit; /*peso unitario*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 21].Value = qtd * peso_unit;/*peso parcial nec*/
                        pecas_aba_excel.Cells[l0 + l, c0 + 22].Value = qtd_fabricada * peso_unit; //peso parcial fabricado
                        pecas_aba_excel.Cells[l0 + l, c0 + 23].Value = peso_embarcado;  // peso embarcado
                        pecas_aba_excel.Cells[l0 + l, c0 + 24].Value = (qtd * peso_unit) - (qtd_fabricada * peso_unit);//* - peso a produzir;
                        pecas_aba_excel.Cells[l0 + l, c0 + 25].Value = qtd_a_embarcar * peso_unit;// - peso a embarcar*;
                        pecas_aba_excel.Cells[l0 + l, c0 + 26].Value = linha["mercadoria"].Valor; //mercadoria
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
                        pecas_aba_excel.Cells[l0 + l, c0 + 32].Value = linha["status_sap"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 33].Value = linha["pintura"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 34].Value = linha["esquema"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 35].Value = linha["esquema_desc"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 36].Value = linha["bobina"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 37].Value = linha["face1"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 38].Value = linha["face2"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 39].Value = linha["complexidade"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 40].Value = linha["denominacao"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 41].Value = linha["tipo"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 42].Value = linha["arquivo"].Valor;
                        pecas_aba_excel.Cells[l0 + l, c0 + 43].Value = linha["tipo_embarque"].Valor;

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
        public static void ExportarListaPecasPMP()
        {
            var pedidos = DLM.painel.Buffer.Pedidos_PMP(true);
            pedidos = pedidos.ListaSelecionarVarios();
            if(pedidos.Count>0)
            {
                var pacote = new Pacote_PMP(pedidos);
                ExportarListaPecasPMP(pacote,true,true,true,true,true,true);
            }
        }
        public static bool ExportarListaPecasPMP(Pacote_PMP pacote, bool abrir = false, bool gerar_subetapas = false, bool gerar_grupos_mercadoria = false, bool gerar_avanco = false, bool gerar_pedidos = false, bool gerar_pecas = true)
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

            var Destino = "xlsx".SalvarArquivo();
            if (Destino == "")
            {
                return false;
            }
            try
            {
                if (File.Exists(Destino)) { File.Delete(Destino); };
                File.Copy(template, Destino);
            }
            catch (Exception ex)
            {
                if (abrir)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                return false;
            }

            var pecas = new List<PLAN_PECA>();

            if (gerar_pecas && gerar_subetapas)
            {
                pecas.AddRange(pacote.GetPecas());
                pacote.Getsubetapas();
                pacote.Getpeps();
                var pedidosstr = pacote.Pedidos.Select(x => x.pep).Distinct().ToList();
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


                    var excel_peca = pck.Workbook.Worksheets[1];
                    var excel_sub = pck.Workbook.Worksheets[2];
                    var excel_pedidos = pck.Workbook.Worksheets[3];
                    var mercadorias_aba_excel = pck.Workbook.Worksheets[4];
                    var avanco_fabrica = pck.Workbook.Worksheets[5];
                    var descricoes = pck.Workbook.Worksheets[6];

                    string Planilha = excel_peca.Name;

                    int l0 = 1;
                    int l = 1;
                    var w = Conexoes.Utilz.Wait(pecas.Count);
                    var TOT = pecas.Count;
                    var mindia = Cfg.Init.DataDummy;
                    double at = 0;


                    var subetapas = new List<SubEtapa_PMP>();
                    if (gerar_subetapas)
                    {
                        subetapas = pacote.Getsubetapas();
                    }

                    /*PEÇAS*/
                    if (gerar_pecas)
                    {
                        w = Conexoes.Utilz.Wait(subetapas.Count + 1, "Gravando Peças...(Isso pode demorar)");
                        int cont = 1;
                        int max = subetapas.Count;
                        foreach(var ped in pacote.Pedidos)
                        {
                            foreach (var sub in ped.SupEtapas)
                            {
                                foreach (var pep in sub.Getpeps())
                                {
                                    foreach (var peca in pep.Getpecas())
                                    {
                                        var L1 = l0 + l;
                                        try
                                        {
                                            excel_peca.Cells[$"A{L1}"].Value = peca.contrato;
                                            excel_peca.Cells[$"B{L1}"].Value = ped.descricao;
                                            excel_peca.Cells[$"C{L1}"].Value = peca.pedido_completo;
                                            excel_peca.Cells[$"D{L1}"].Value = peca.PEP;
                                            excel_peca.Cells[$"E{L1}"].Value = peca.desenho;
                                            excel_peca.Cells[$"F{L1}"].Value = peca.centro;
                                            excel_peca.Cells[$"G{L1}"].Value = peca.qtd_necessaria;
                                            excel_peca.Cells[$"H{L1}"].Value = peca.qtd_produzida;
                                            excel_peca.Cells[$"I{L1}"].Value = peca.qtd_embarcada > 0 ? peca.qtd_embarcada : 0;
                                            excel_peca.Cells[$"J{L1}"].Value = sub.Embarque.Peso_Necessario;
                                            excel_peca.Cells[$"K{L1}"].Value = sub.Embarque.Peso_Embarcado;
                                            excel_peca.Cells[$"L{L1}"].Value = peca.comprimento;
                                            excel_peca.Cells[$"M{L1}"].Value = peca.corte_largura;
                                            excel_peca.Cells[$"N{L1}"].Value = peca.espessura;
                                            excel_peca.Cells[$"O{L1}"].Value = peca.material;
                                            excel_peca.Cells[$"P{L1}"].Value = peca.texto_breve;
                                            excel_peca.Cells[$"Q{L1}"].Value = peca.peso_necessario;
                                            excel_peca.Cells[$"R{L1}"].Value = peca.peso_produzido;
                                            excel_peca.Cells[$"S{L1}"].Value = peca.peso_a_produzir;
                                            excel_peca.Cells[$"T{L1}"].Value = peca.grupo_mercadoria;
                                            excel_peca.Cells[$"U{L1}"].Value = sub.liberado_engenharia / 100;
                                            excel_peca.Cells[$"V{L1}"].Value = peca.fabricado_porcentagem;
                                            excel_peca.Cells[$"W{L1}"].Value = peca.embarcado_porcentagem;
                                            excel_peca.Cells[$"X{L1}"].Value = sub.ef;
                                            excel_peca.Cells[$"Y{L1}"].Value = sub.ff;
                                            excel_peca.Cells[$"Z{L1}"].Value = sub.li;
                                            excel_peca.Cells[$"AA{L1}"].Value =  sub.mi;
                                            excel_peca.Cells[$"AB{L1}"].Value = sub.mi_s> mindia ? sub.mi_s : null;
                                            excel_peca.Cells[$"AC{L1}"].Value = peca.inicio > mindia ? peca.inicio : null;
                                            excel_peca.Cells[$"AD{L1}"].Value = peca.fim > mindia ? peca.fim : null;
                                            excel_peca.Cells[$"AE{L1}"].Value = peca.ultima_edicao;
                                            excel_peca.Cells[$"AF{L1}"].Value = peca.ULTIMO_STATUS;
                                            excel_peca.Cells[$"AG{L1}"].Value = peca.TIPO_DE_PINTURA;
                                            excel_peca.Cells[$"AH{L1}"].Value = peca.esq_de_pintura;
                                            excel_peca.Cells[$"AI{L1}"].Value = peca.Esquema.Getdescricao();
                                            excel_peca.Cells[$"AJ{L1}"].Value = peca.codigo_materia_prima_sap;
                                            excel_peca.Cells[$"AK{L1}"].Value = peca.bobina.Cor1;
                                            excel_peca.Cells[$"AL{L1}"].Value = peca.bobina.Cor2;
                                            excel_peca.Cells[$"AM{L1}"].Value = peca.tipo_aco;
                                            excel_peca.Cells[$"AN{L1}"].Value = peca.Complexidade;
                                            excel_peca.Cells[$"AO{L1}"].Value = peca.DENOMINDSTAND;
                                            excel_peca.Cells[$"AP{L1}"].Value = peca.Tipo.ToString();
                                            excel_peca.Cells[$"AQ{L1}"].Value = ""; //arquivo
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        l++;
                                    }
                                }
                                cont++;
                                w.somaProgresso(sub.etapa + " - Gravando Peças...");
                            }
                        }
                    }


                    /*SUBETAPAS*/
                    if (excel_sub != null && gerar_subetapas)
                    {
                        l0 = 1;
                        l = 1;
                        w = Conexoes.Utilz.Wait(subetapas.Count, "Carregando SubEtapas...");
                        foreach(var ped in pacote.Pedidos)
                        {
                            foreach (var sub in ped.SupEtapas)
                            {
                                try
                                {
                                    var L1 = l0 + l;
                                    excel_sub.Cells[$"A{L1}"].Value = sub.contrato;
                                    excel_sub.Cells[$"B{L1}"].Value = ped.descricao;
                                    excel_sub.Cells[$"C{L1}"].Value = sub.pedido;
                                    excel_sub.Cells[$"D{L1}"].Value = sub.pep;
                                    excel_sub.Cells[$"E{L1}"].Value = sub.tipo.ToString();
                                    //excel_sub.Cells[$"F{L1}"].Value = sub.Real.resumo_pecas.etapa_bloqueada ? 1 : 0;
                                    excel_sub.Cells[$"G{L1}"].Value = sub.peso;
                                    excel_sub.Cells[$"H{L1}"].Value = sub.Real.liberado_engenharia / 100;
                                    excel_sub.Cells[$"I{L1}"].Value = sub.total_fabricado / 100;
                                    excel_sub.Cells[$"J{L1}"].Value = sub.total_embarcado / 100;
                                    excel_sub.Cells[$"K{L1}"].Value = sub.Embarque.Peso_Necessario;
                                    excel_sub.Cells[$"L{L1}"].Value = sub.Embarque.Peso_Embarcado;
                                    excel_sub.Cells[$"M{L1}"].Value = sub.Embarque.Porcentagem_Embarcado;
                                    excel_sub.Cells[$"N{L1}"].Value = sub.Real.total_montado / 100;
                                    excel_sub.Cells[$"O{L1}"].Value = sub.ei > mindia ? sub.ei : null;
                                    excel_sub.Cells[$"P{L1}"].Value = sub.ef > mindia ? sub.ef : null;
                                    excel_sub.Cells[$"Q{L1}"].Value = sub.fi > mindia ? sub.fi : null;
                                    excel_sub.Cells[$"R{L1}"].Value = sub.ff > mindia ? sub.ff : null;
                                    excel_sub.Cells[$"S{L1}"].Value = sub.li > mindia ? sub.li : null;
                                    excel_sub.Cells[$"T{L1}"].Value = sub.lf > mindia ? sub.lf : null;
                                    excel_sub.Cells[$"U{L1}"].Value = sub.mi_s > mindia ? sub.mi_s : null;
                                    excel_sub.Cells[$"V{L1}"].Value = sub.mf_s > mindia ? sub.mf_s : null;
                                    excel_sub.Cells[$"W{L1}"].Value = sub.mi > mindia ? sub.mf : null;
                                    excel_sub.Cells[$"X{L1}"].Value = sub.mf > mindia ? sub.mf : null;
                                    excel_sub.Cells[$"Y{L1}"].Value = sub.Real.ultima_consulta_sap > mindia ? sub.Real.ultima_consulta_sap : null;
                                    //excel_sub.Cells[$"Z{L1}"].Value = sub.Real.resumo_pecas.Inicio > mindia ? sub.Real.resumo_pecas.Inicio : null;
                                    //excel_sub.Cells[$"AA{L1}"].Value = sub.Real.resumo_pecas.Fim > mindia ? sub.Real.resumo_pecas.Fim : null;
                                    excel_sub.Cells[$"AB{L1}"].Value = sub.Real.update_montagem.ToUpper().Replace(" ", "").Replace("MONTAGEM:", "");
                                    excel_sub.Cells[$"AC{L1}"].Value = sub.Real.montagem_engenheiro;
                                }
                                catch (Exception)
                                {
                                }
                                w.somaProgresso();
                                l++;
                            }
                        }
                    }

                    /*PEDIDOS*/
                    if (excel_pedidos != null && gerar_pedidos)
                    {

                        var peds = subetapas.Select(x => x.contrato).Distinct().ToList();
                        var pedss = subetapas.Select(x => x.pedido).Distinct().ToList();
                        //var pedidos_sistema = DLM.painel.Consultas.GetPedidos(peds);
                        var pedidos_sistema = pacote.Pedidos.FindAll(x => x.Material_REAL).Select(x => x.Real).ToList();

                        l0 = 2;
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

                                excel_pedidos.Cells[$"A{L1}"].Value = pedido.descricao;
                                excel_pedidos.Cells[$"B{L1}"].Value = pedido.pedido;
                                excel_pedidos.Cells[$"C{L1}"].Value = pedido.exportacao ? 1 : 0;
                                excel_pedidos.Cells[$"D{L1}"].Value = pedido.peso_planejado;

                                excel_pedidos.Cells[$"E{L1}"].Value = pedido.engenharia_previsto / 100;
                                excel_pedidos.Cells[$"F{L1}"].Value = pedido.liberado_engenharia / 100;
                                excel_pedidos.Cells[$"G{L1}"].Value = (pedido.engenharia_previsto / 100) - (pedido.liberado_engenharia / 100);

                                excel_pedidos.Cells[$"H{L1}"].Value = pedido.fabrica_previsto / 100;
                                excel_pedidos.Cells[$"I{L1}"].Value = pedido.total_fabricado / 100;
                                excel_pedidos.Cells[$"J{L1}"].Value = (pedido.fabrica_previsto / 100) - (pedido.total_fabricado / 100);


                                excel_pedidos.Cells[$"K{L1}"].Value = pedido.embarque_previsto / 100;
                                excel_pedidos.Cells[$"L{L1}"].Value = pedido.total_embarcado / 100;
                                excel_pedidos.Cells[$"M{L1}"].Value = (pedido.embarque_previsto / 100) - (pedido.total_embarcado / 100);

                                excel_pedidos.Cells[$"N{L1}"].Value = pedido.montagem_previsto / 100;
                                excel_pedidos.Cells[$"O{L1}"].Value = pedido.total_montado / 100;
                                excel_pedidos.Cells[$"P{L1}"].Value = (pedido.montagem_previsto / 100) - (pedido.total_montado / 100);

                                excel_pedidos.Cells[$"Q{L1}"].Value = pedido.status_montagem != "SEM APONTAMENTO" ? 1 : 0;
                                excel_pedidos.Cells[$"R{L1}"].Value = pedido.montagem_engenheiro;
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
                        }
                    }



                    /*ESCONDE ABAS*/
                    try
                    {
                        if (subetapas == null)
                        {
                            if (excel_peca.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                excel_peca.Select();
                            }
                            excel_sub.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            excel_pedidos.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        else if (subetapas.Count == 0)
                        {
                            if (excel_peca.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                excel_peca.Select();
                            }
                            excel_sub.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                            excel_pedidos.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }



                        if (pecas.Count == 0)
                        {
                            if (excel_sub.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible)
                            {
                                excel_sub.Select();
                            }
                            excel_peca.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
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
                        if (!gerar_pedidos)
                        {
                            excel_pedidos.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if (!gerar_subetapas)
                        {
                            excel_sub.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
                        }
                        if (!gerar_pecas)
                        {
                            excel_peca.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
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
                    Conexoes.Utilz.Alerta(ex);
                }
            }
            return true;
        }


   
    }
}
