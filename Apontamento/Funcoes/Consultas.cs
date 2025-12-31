using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Conexoes;
using DLM.sapgui;
using DLM.vars;

namespace DLM.painel
{
    public static class Consultas
    {
        private static List<PLAN_OBRA> _obras { get; set; }
        private static List<PLAN_PEDIDO> _pedidos { get; set; }
        private static List<string> _pedidos_clean { get; set; }
        private static List<Plan_Ped_Contrato> _titulos_obras { get; set; }


        public static List<ORC_PED> GetObrasPGO()
        {
            var retorno = new List<ORC_PED>();
            var consulta = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_resumo_consolidada);
            foreach (var p in consulta)
            {
                var ped = new ORC_PED(p, Tipo_Material.Consolidado);

                if (ped.PEP.LenghtStr() > 0)
                {
                    retorno.Add(ped);
                }
            }

            var pacotes = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada_arquivos_peps);
            foreach (var pedido in retorno)
            {
                var pcks = pacotes.ToList().FindAll(x => x["pedido"].Valor == pedido.PEP);
                var arqs = pcks.Select(x => x["arquivo"].Valor).Distinct().ToList();
                foreach (var arq in arqs)
                {
                    var peps = pcks.FindAll(x => x["arquivo"].Valor == arq).Select(x => x["pep"].Valor).Distinct().ToList();
                    pedido.pacotes.Add(new ORC_PCK(arq, peps, pedido));
                }

            }
            return retorno;
        }

        public static List<PLAN_PECA> GetPecasPMP(List<Pedido_PMP> pedidos)
        {
            var retorno = new List<PLAN_PECA>();
            var pedidos_real = pedidos.FindAll(x => x.Material_REAL).Select(x => x.PEP).Distinct().ToList();
            var pedidos_consolidados = pedidos.FindAll(x => x.Material_CONS).Select(x => x.PEP).Distinct().ToList();

            var w = Conexoes.Utilz.Wait(pedidos_real.Count * 4 + 3, $"Mapeando peças...{pedidos.Count} pedidos...");

            int tam_pacote = 20;

            var reais_pecas = GetPecasReal(pedidos_real, tam_pacote);
            w.somaProgresso();

            var cons_pecas = GetPecasPGO(pedidos_consolidados, tam_pacote, true);
            w.somaProgresso();

            foreach (var real in pedidos_real)
            {
                var ped = pedidos.Find(x => x.PEP == real);
                if (ped != null)
                {

                    ped.Real.Set(reais_pecas);
                }
                w.somaProgresso();
            }
            foreach (var orc in pedidos_consolidados)
            {
                var ped = pedidos.Find(x => x.PEP == orc);
                if (ped != null)
                {
                    ped.Consolidada.Set(cons_pecas);
                }
                w.somaProgresso();

            }
            retorno.AddRange(reais_pecas);
            retorno.AddRange(cons_pecas);
            retorno = retorno.OrderBy(x => x.PEP).ToList();
            foreach (var pedido in pedidos)
            {
                pedido.Set(retorno);
                w.somaProgresso();
            }
            w.Close();
            return retorno;
        }
        public static List<PLAN_PECA> GetPecasPGO(List<string> pedidos, int max_pacote, bool consolidada = false)
        {
            if (pedidos.Count == 0)
            {
                return new List<PLAN_PECA>();
            }
            var retorno = new List<PLAN_PECA>();
            var sub_lista = pedidos.Quebrar(max_pacote);
            var w = Conexoes.Utilz.Wait(sub_lista.Count, "Procurando Peças ..." + (consolidada ? "(2/3 - Consolidadas)" : "(3/3 - Orcamentos)"));
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

            var tabela = DBases.GetDBPGO().Consulta($"SELECT * from {Cfg.Init.db_orcamento}.{tab} as prod where {chave}");
            foreach (var linha in tabela)
            {
                var pc = new PLAN_PECA(linha, true);

                if (consolidada)
                {
                    pc.Tipo = Tipo_Material.Consolidado;
                }
                retorno.Add(pc);
            }
            return retorno;
        }

        public static List<ORC_ETP> GetEtapasPGO(List<string> pedidos, bool consolidada = false)
        {

            string chave_pedidos = "";
            var retorno = new List<ORC_ETP>();
            string tab = "pmp_orc_etapas";
            if (consolidada)
            {
                tab = "pmp_orc_etapas_consolidada";
            }


            foreach (var pedido in pedidos)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_orcamento}.{tab} as pr where ") + $" pr.pep like '%{pedido}%'";
            }



            var consulta = DBases.GetDBPGO().Consulta(chave_pedidos);
            foreach (var linha in consulta)
            {
                var pc = new ORC_ETP(linha);
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


        public static List<ZPP0100_Resumo> GetResumoEmbarquesPEP(List<string> peps, int coluna = 24)
        {
            var retorno = new List<ZPP0100_Resumo>();

            var subs = peps.GroupBy(x => Conexoes.Utilz.PEP.Get.Pedido(x, true)).ToList();

            foreach (var sub in subs)
            {
                var peps_grp = sub.ToList();
                var st_sub = DBases.GetDB().Consulta($"call {Cfg.Init.db_comum}.getzpp0100_resumo('{sub.Key}',{coluna})");
                if (st_sub.Count > 0)
                {
                    foreach (var linha in st_sub)
                    {
                        retorno.Add(new ZPP0100_Resumo(linha));
                    }
                    //foreach (var pedido in sub.ToList())
                    //{
                    //    //var consulta = DBases.GetDB().Consulta($"call {Cfg.Init.db_comum}.getzpp0100_resumo('{pedido}',{coluna})");
                    //    var consulta = st_sub.Filtrar(true, "pep", pedido);
                    //    foreach (var linha in consulta)
                    //    {
                    //        retorno.Add(new ZPP0100_Resumo(linha));
                    //    }
                    //}

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


            var consulta = DBases.GetDBPGO().Consulta(chave_pedidos);
            foreach (var linha in consulta)
            {
                var nSub = new ORC_SUB(linha);
                nSub.GetDatas();
                retorno.Add(nSub);
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

            List<ORC_PEP> peps = new List<ORC_PEP>();
            string tab = "pmp_orc_peps";
            if (consolidada)
            {
                tab = "pmp_orc_peps_consolidada";
            }


            string chave_pedidos = "";
            foreach (var pedido in pedidos)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_orcamento}.{tab} as pr where ") + $" pr.pep like '%{pedido}%'";
            }


            var tabela = DBases.GetDBPGO().Consulta(chave_pedidos);



            foreach (var linha in tabela)
            {
                var nPEP = new ORC_PEP(linha);
                peps.Add(nPEP);
            }

            if (consolidada)
            {
                foreach (var nPEP in peps)
                {
                    nPEP.tipo = Tipo_Material.Consolidado;
                }
            }

            return peps;
        }


        public static void ApagarObra(string pedido)
        {
            pedido = pedido.Replace("*", "").Replace(" ", "").Replace("%", "");

            if (pedido.LenghtStr() < 6)
            {
                return;
            }

            DBases.LimparPMP_Orcamento(pedido);
            DBases.LimparPMP_Consolidada(pedido);


            DBases.GetDB().Apagar("pedido_principal", $"%{pedido}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_obras_copia);
            DBases.GetDB().Apagar("pedido", $"%{pedido}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pedidos_copia);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pep_copia);

            DBases.GetDB().Apagar("contrato", $"%{pedido}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);

            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n);

            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada_ranges);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc);
            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_datas);

            DBases.GetDB().Apagar("pep", $"%{pedido}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas);

            DBases.GetDB().Apagar("pspid", $"%{pedido.Replace("-", "").Replace(".", "")}%", Cfg.Init.db_sap, Cfg.Init.tb_sap_pedidos);

            DLM.SAP.ZGWBS_UPDATE(pedido, DLM.sap.SAP_Acao.Nada, DLM.sap.SAP_Acao.Sim);
        }

        public static void SincronizarTitulosContratos(List<string> contratos)
        {
            var contratos_sap = DLM.SAP.GetContratos();
            foreach (var contrato in contratos)
            {
                if (contrato.LenghtStr() == 6)
                {
                    if (contrato.Int() > 100000)
                    {
                        var contrato_sap = contratos_sap.Find(x => x.Contrato == contrato);

                        if (contratos_sap != null)
                        {
                            Conexoes.DBases.GetDB().Apagar("contrato", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);

                            var linhas = new List<DLM.db.Linha>();
                            foreach (var pedido in contrato_sap.GetPedidos())
                            {
                                if (pedido.PEP.Contem(".P"))
                                {
                                    linhas.Add(new db.Linha("contrato", pedido.PEP, "descricao", pedido.Descricao));
                                }
                            }
                            if (linhas.Count > 0)
                            {
                                Conexoes.DBases.GetDB().Cadastro(linhas, Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
                            }
                        }
                    }

                }
            }
        }

        public static List<Plan_Ped_Contrato> GetPedidosContratos()
        {
            if (_titulos_obras == null)
            {
                _titulos_obras = new List<Plan_Ped_Contrato>();
                var lista_fab = DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
                var retorno = new ConcurrentBag<Plan_Ped_Contrato>();
                var Tarefas = new List<Task>();
                foreach (var linha in lista_fab)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new Plan_Ped_Contrato(linha))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());
                _titulos_obras = retorno.OrderBy(x => x.Contrato).ToList();
            }
            return _titulos_obras;
        }




        private static List<PLAN_OBRA> GetObras(bool reset = false)
        {

            if (_obras == null | reset)
            {
                _obras = new List<PLAN_OBRA>();
                var consulta = DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_obras_copia);

                foreach (var linha in consulta)
                {
                    _obras.Add(new PLAN_OBRA(linha));
                }

                var Tarefas = new List<Task>();
                var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_04_obra);

                foreach (var obra in _obras)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        var igual = st_base["pep", obra.PEP];
                        if (igual.Count > 0)
                        {
                            obra.SetBase(igual[0]);
                        }
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
            }
            return _obras;
        }
        public static List<PLAN_OBRA> GetObras(List<string> obras = null, bool reset = false)
        {
            if (obras == null)
            {
                obras = new List<string>();
            }
            obras = obras.Distinct().ToList().FindAll(x => x.LenghtStr() > 0);
            var obras_lista = GetObras(reset);
            if (obras.Count == 0)
            {
                return obras_lista;
            }

            var retorno = new List<PLAN_OBRA>();
            foreach (var obra in obras)
            {
                retorno.AddRange(obras_lista.FindAll(x => x.PEP.Contem(obra)));
            }
            retorno = retorno.GroupBy(x => x.PEP).Select(x => x.First()).ToList();

            return retorno;
        }
        public static void SincronizarPedidos(List<string> pedidos)
        {
            var w = Conexoes.Utilz.Wait(pedidos.Count, "Rodando Pedidos...");
            var contratos = pedidos.Select(x => Conexoes.Utilz.PEP.Get.Contrato(x)).Distinct().ToList();
            Consultas.SincronizarTitulosContratos(contratos);

            var conexaoSAP2 = new ConexaoSAP("");
            var pack_pedidos = pedidos.Quebrar(3);

            var Tarefas = new List<Task>();
            int m = pedidos.Count;
            int c = 1;
            foreach (var pacote in pack_pedidos)
            {
                var obrasSAP = new List<ConexaoSAP>();
                foreach (var pedido in pacote)
                {
                    w.somaProgresso($"{c}/{m} - {pedido}");

                    var consultaContrato = new ConexaoSAP(pedido);
                    obrasSAP.Add(consultaContrato);
                    /*como o consultasap carrega todas as datas, eu salvo as datas de cronograma no sistema.*/
                    if (consultaContrato.ConsultaSAP())
                    {
                        Consultas.CriarCache(pedido.Replace("*", "").Replace("%", ""));
                    }
                    c++;
                }
            }
            w.Close();
            Conexoes.Email.Enviar(new List<string> { "daniel.maciel@medabil.com.br" }, $"Sincronização ZPAINEL {pedidos.Count} pedidos - {DateTime.Now.ToString()}",
                $"<html>{string.Join("\n<br>", pedidos)}</html>");
        }
        public static void CriarCache(string contrato)
        {
            if (contrato.LenghtStr() < 4) { return; }
            DBases.Painel_Criar_Cache(contrato);


            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas);
            var Pecas = Consultas.GetPecasReal(new List<string> { contrato });


            if (Pecas.Count == 0)
            {
                return;
            }


            var w = Conexoes.Utilz.Wait(10, $"Logística...{Pecas.Count} [{contrato}]");

            var orfas = new List<PLAN_PECA>();
            var logistica = DLM.painel.Consultas.GetLogistica(Pecas, out orfas);

            Pecas.AddRange(orfas);
            Pecas = Pecas.OrderBy(x => x.ToString()).ToList();


            w.SetProgresso(1, Pecas.Count, "Mapeando peças...");
            var mindia = Cfg.Init.DataDummy;


            var linhas = new List<DLM.db.Linha>();
            foreach (var peca in Pecas.OrderBy(x => x.PEP))
            {
                var lp = new PLAN_PECA_LOG(peca);
                if (peca.logistica.Count == 0)
                {
                    var ldb = GetLinhaDB(mindia, lp, peca.qtd_necessaria, peca.qtd_produzida, 0, peca.qtd_necessaria);
                    linhas.Add(ldb);
                }
                else
                {
                    var qtd_a_embarcar = peca.qtd_necessaria - peca.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);
                    var qtd_a_fabricar = peca.qtd_necessaria - peca.qtd_produzida;
                    var qtd_a_embarcar_produzida = peca.qtd_produzida - (peca.qtd_embarcada > 0 ? peca.qtd_embarcada : 0);
                    var qtd_fab = qtd_a_fabricar <= 0 ? qtd_a_embarcar : qtd_a_embarcar_produzida;
                    var resto = peca.qtd_necessaria - peca.logistica.FindAll(x => x.carga_confirmada).Sum(x => x.quantidade);

                    if (resto < 0)
                    {
                        resto = 0;
                    }

                    if (qtd_a_embarcar > 0)
                    {
                        linhas.Add(GetLinhaDB(mindia, lp, resto, qtd_fab, 0, qtd_a_embarcar));
                    }
                    foreach (var plog in peca.logistica.FindAll(x => x.carga_confirmada))
                    {
                        linhas.Add(GetLinhaDB(mindia, plog, plog.quantidade, plog.quantidade, plog.quantidade, 0));
                    }
                }
                w.somaProgresso();
            }
            DBases.GetDB().Cadastro(linhas, Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas);


            w.Close();

        }


        private static DLM.db.Linha GetLinhaDB(DateTime mindia, PLAN_PECA_LOG peca_log, double qtd, double qtd_fab, double qtd_emb, double qtd_a_embarcar)
        {
            var ldb = new DLM.db.Linha();
            ldb.Add("pep", peca_log.peca.PEP);
            ldb.Add("centro", peca_log.peca.centro);
            ldb.Add("carreta", peca_log.num_carga);
            ldb.Add("ordem", peca_log.pack_list);
            ldb.Add("material", peca_log.peca.material);
            ldb.Add("descricao", peca_log.peca.texto_breve);
            ldb.Add("qtd_embarque", "");
            ldb.Add("qtd_carregada", qtd_emb);
            ldb.Add("placa", peca_log.placa);
            ldb.Add("motorista", peca_log.motorista);
            ldb.Add("marca", peca_log.peca.desenho);
            ldb.Add("observacoes", peca_log.observacoes);
            ldb.Add("qtd", qtd);
            ldb.Add("produzido", qtd_fab);
            ldb.Add("embarcado", qtd_emb);
            ldb.Add("qtd_a_embarcar", qtd_a_embarcar);
            ldb.Add("comprimento", peca_log.peca.comprimento);
            ldb.Add("corte", peca_log.peca.corte_largura);
            ldb.Add("espessura", peca_log.peca.espessura);
            /*05/08/2020 - ajuste para pegar somente o peso total da quantidade embarcada*/
            ldb.Add("peso_tot", peca_log.peca.peso_necessario / peca_log.peca.qtd_necessaria * qtd_emb);
            ldb.Add("mercadoria", peca_log.peca.grupo_mercadoria);
            ldb.Add("primeiro_apontamento_fab", peca_log.peca.inicio > mindia ? peca_log.peca.inicio : null);
            ldb.Add("ultimo_apontamento_fab", peca_log.peca.fim > mindia ? peca_log.peca.fim : null);
            ldb.Add("atualizado_em", peca_log.peca.ultima_edicao);
            ldb.Add("status_sap", peca_log.peca.ULTIMO_STATUS);
            ldb.Add("pintura", peca_log.peca.TIPO_DE_PINTURA);
            ldb.Add("esquema", peca_log.peca.Esquema.ESQUEMA_COD);
            ldb.Add("esquema_desc", peca_log.peca.Esquema.Getdescricao());
            if (peca_log.peca.bobina != null)
            {
                ldb.Add("bobina", peca_log.peca.bobina.SAP);
                ldb.Add("face1", peca_log.peca.bobina.Cor1.Nome);
                ldb.Add("face2", peca_log.peca.bobina.Cor2.Nome);
            }
            ldb.Add("complexidade", peca_log.peca.Complexidade);
            ldb.Add("denominacao", peca_log.peca.DENOMINDSTAND);
            ldb.Add("tipo", peca_log.peca.Tipo);
            ldb.Add("arquivo", peca_log.peca.DESENHO_1);
            ldb.Add("tipo_embarque", peca_log.peca.Tipo_Embarque);
            return ldb;
        }

        public static List<PLAN_PEDIDO> GetPedidos(bool reset = false)
        {
            if (_pedidos == null | reset)
            {
                _pedidos = new List<PLAN_PEDIDO>();
                var consulta = new DLM.db.Tabela();
                var lista = new List<PLAN_PEDIDO>();

                consulta = DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pedidos_copia);

                var Tarefas = new List<Task>();
                foreach (var linha in consulta)
                {
                    lista.Add(new PLAN_PEDIDO(linha, new PLAN_OBRA()));
                }
                Task.WaitAll(Tarefas.ToArray());
                _pedidos.AddRange(lista);
                _pedidos = _pedidos.OrderBy(x => x.pedido).ToList();

                foreach (var o in GetObras(false))
                {
                    o.Set(_pedidos);
                }

                var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_03_pedido);


                foreach (var t in _pedidos)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        var igual = st_base["pep", t.PEP];
                        if (igual.Count > 0)
                        {
                            t.SetBase(igual[0]);
                        }
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
            }




            return _pedidos;
        }


        public static List<PLAN_PEDIDO> GetPedidos(List<string> contrato)
        {

            contrato = contrato.Distinct().ToList().FindAll(x => x.LenghtStr() > 0);
            if (contrato.Count == 0)
            {
                return GetPedidos();
            }

            List<PLAN_PEDIDO> pedidos = new List<PLAN_PEDIDO>();
            foreach (var s in contrato)
            {
                pedidos.AddRange(GetPedidos().FindAll(x => x.PEP.Contem(s.Replace("%", "").Replace("*", ""))));
            }
            pedidos = pedidos.GroupBy(x => x.PEP).Select(x => x.First()).ToList();

            return pedidos;
        }
        public static List<PLAN_ETAPA> GetEtapas(List<string> pedidos)
        {
            var _etapas = new ConcurrentBag<PLAN_ETAPA>();
            var subetapas = GetSubEtapas(pedidos);
            List<Task> Tarefas = new List<Task>();

            foreach (var et in subetapas.Select(x => x.etapa).OrderBy(x => x).Distinct().ToList())
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    _etapas.Add(new PLAN_ETAPA(subetapas.FindAll(x => x.etapa == et)));
                }));
            }
            Task.WaitAll(Tarefas.ToArray());

            var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_02_etapa);

            var etapa_list = _etapas.ToList();

            foreach (var etapa in etapa_list)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var tabela = st_base["pep", etapa.PEP.ToUpper().Replace(".P00", "")];
                    if (tabela.Count > 0)
                    {
                        etapa.SetBase(tabela[0]);
                    }
                }));
            }
            Task.WaitAll(Tarefas.ToArray());


            if (_pedidos != null)
            {
                foreach (var p in _pedidos)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        p.Set(etapa_list);
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
            }

            return etapa_list;
        }
        public static List<PLAN_SUB_ETAPA> GetSubEtapas(List<string> pedidos)
        {
            var _subetapas = new List<PLAN_SUB_ETAPA>();
            var peps = GetPepsReal(pedidos);

            string chave_pedidos = "";

            foreach (var pedido in pedidos)
            {
                chave_pedidos += (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_painel_de_obras2}.{Cfg.Init.tb_pep_copia} as pr where ") + $" pr.pep like '%{pedido}%'";
            }




            var consulta = DBases.GetDB().Consulta(chave_pedidos.Replace("%%", "%"));
            var lista = new ConcurrentBag<PLAN_SUB_ETAPA>();

            var Tarefas = new List<Task>();

            foreach (var pack in consulta.Quebrar(500))
            {
                foreach (var linha in pack)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                                  lista.Add(new PLAN_SUB_ETAPA(linha, peps))
                    ));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();
            }

            _subetapas.AddRange(lista);
            _subetapas = _subetapas.OrderBy(x => x.subetapa).ToList();
            var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_01_subetapa);


            foreach (var subetapa in _subetapas)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var igual = st_base["pep", subetapa.PEP.ToUpper().Replace(".P00", "")];
                    if (igual.Count > 0)
                    {
                        subetapa.SetBase(igual[0]);
                    }
                }));
            }
            Task.WaitAll(Tarefas.ToArray());


            return _subetapas;
        }
        public static List<PLAN_PEP> GetPepsReal(List<string> peps)
        {
            var _peps = new ConcurrentBag<PLAN_PEP>();
            var retorno = new List<PLAN_PEP>();

            string tabela = $"{Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento}";
            var sub = peps.Quebrar(300);
            var Tarefas = new List<Task>();

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
                        chave = $"{tabela}.pep like '%{chaves_consulta[i]}%'";
                    }
                    else
                    {
                        chave = chave + $" or {tabela}.pep like '%{chaves_consulta[i]}%'";
                    }
                }

                var consulta1 = DBases.GetDB().Consulta($"select * from {tabela} where {chave}");

                foreach (var pep in consulta1)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        _peps.Add(new PLAN_PEP(pep));
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
            }

            retorno.AddRange(_peps);


            var consulta2 = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_00_pep);
            foreach (var ret in retorno)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var igual = consulta2["pep", ret.PEP];
                    if (igual.Count > 0)
                    {
                        ret.SetBase(igual[0]);
                    }
                }));
            }
            Task.WaitAll(Tarefas.ToArray());


            return retorno;
        }

        public static List<PLAN_PECA_LOG> GetLogistica(List<PLAN_PECA> pecas, out List<PLAN_PECA> orfas)
        {
            orfas = new List<PLAN_PECA>();
            var pedidos = pecas.Select(x => x.pedido_completo).Distinct().ToList();
            pedidos = pedidos.FindAll(x => x.LenghtStr() > 5).Distinct().ToList();

            if (pedidos.Count == 0) { return new List<PLAN_PECA_LOG>(); }
            var retorno = new List<PLAN_PECA_LOG>();

            foreach (var pedido in pedidos)
            {
                var lista_fab_0100 = DBases.GetDB().Consulta($"call {Cfg.Init.db_comum}.getzpp0100_cargas('{pedido}')");
                foreach (var linha in lista_fab_0100)
                {
                    retorno.Add(new PLAN_PECA_LOG(pecas, linha, Tipo_Embarque.ZPP0100));
                }
            }
            for (int i = 0; i < pecas.Count; i++)
            {
                if (pecas[i].PEP.LenghtStr() > 0 && pecas[i].material != "")
                {
                    var logs = retorno.ToList().FindAll(x => x.pep == pecas[i].PEP && x.material == pecas[i].material);
                    pecas[i].SetLogistica(logs);
                }
            }

            var sem_peca = retorno.FindAll(x => x.peca.material == "").ToList();
            var grp_sem_peca = sem_peca.GroupBy(x => x.pep + "/" + x.material + "/" + x.desenho).ToList().Select(x => x.ToList()).ToList();
            foreach (var s in grp_sem_peca)
            {
                var fils = s.ToList();
                var pc = new PLAN_PECA(fils[0]);
                pc.SetLogistica(fils);
                orfas.Add(pc);
            }

            return retorno.FindAll(x => x.peca != null);
        }

        public static List<PLAN_PECA_ZPMP> GetPecasZPMP(string pep)
        {
            var consulta = Conexoes.DBases.GetDB().Consulta("pep", pep, Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao, false);
            var retorno = new List<PLAN_PECA_ZPMP>();
            foreach (var linha in consulta)
            {
                retorno.Add(new PLAN_PECA_ZPMP(linha));
            }
            return retorno;
        }

        public static List<PLAN_PECA> GetPecasReal(List<string> lista_pedidos, int max_pacote = 10)
        {
            lista_pedidos = lista_pedidos.FindAll(x => x.LenghtStr() > 5).Distinct().ToList();
            lista_pedidos = lista_pedidos.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "").OrderBy(x => x).ToList();

            if (lista_pedidos.Count == 0) { return new List<PLAN_PECA>(); }
            var retorno = new List<PLAN_PECA>();

            var tabelas_pecas = new List<DLM.db.Tabela>();
            var tabelas_embarques = new List<DLM.db.Tabela>();
            foreach (var pedido in lista_pedidos)
            {
                var lista_pecas = DBases.GetDB().Consulta($"call {Cfg.Init.db_comum}.getpecas('{pedido}')");
                var lista_embarques = DBases.GetDB().Consulta($"call {Cfg.Init.db_comum}.getPecasEmbarques('{pedido}')");
                tabelas_pecas.Add(lista_pecas);
                tabelas_embarques.Add(lista_embarques);
            }

            for (int i = 0; i < tabelas_pecas.Count; i++)
            {
                var lista_pecas = tabelas_pecas[i];
                var lista_embarques = tabelas_embarques[i];
                var _pecas = new ConcurrentBag<PLAN_PECA>();
                var Tarefas = new List<Task>();
                foreach (var linha in lista_pecas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        _pecas.Add(new PLAN_PECA(linha));
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());

                var plan_pecas = new List<PLAN_PECA>();
                plan_pecas.AddRange(_pecas);

                var peps_chaves = lista_embarques.GroupBy(x => Conexoes.Utilz.PEP.Get.Subetapa(x["Elemento_PEP"].Valor, true)).ToList();
                foreach (var pep in peps_chaves)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        var pecas_pep = plan_pecas.FindAll(x => Conexoes.Utilz.PEP.Get.Subetapa(x.PEP, true) == pep.Key);
                        foreach (var peca in pecas_pep)
                        {
                            peca.SetStatusByZPP0100(pep.ToList());
                        }
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());

                retorno.AddRange(plan_pecas);
            }
            return retorno;
        }

        public static SolidColorBrush getCor(double previsto, double realizado, double opacidade = 1)
        {
            if (realizado >= 100)
            {
                return new SolidColorBrush(Colors.DarkGreen) { Opacity = opacidade };
            }
            else if (realizado == 0 && previsto == 0)
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

        public static List<string> GetPedidosClean(List<string> contratos, bool update)
        {

            if (_pedidos_clean == null | update)
            {
                _pedidos_clean = new List<string>();

                _pedidos_clean.AddRange(DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pedidos_copia).Select(x => x["pedido"].Valor).Distinct().ToList());
                _pedidos_clean = _pedidos_clean.Distinct().ToList().FindAll(x => x.LenghtStr() > 3);
            }
            if (contratos != null)
            {
                var _retorno = new List<string>();
                foreach (var contrato in contratos.Distinct().ToList().FindAll(x => x.LenghtStr() > 5))
                {
                    _retorno.AddRange(_pedidos_clean.FindAll(x => x.Contem(contrato)));
                }
                _retorno = _retorno.Distinct().ToList();
                return _retorno;
            }

            return _pedidos_clean;

        }


        public static bool MatarExcel(bool confirmar = false)
        {
            var t = Process.GetProcessesByName("EXCEL").ToList();
            t.AddRange(Process.GetProcesses().ToList().FindAll(x => x.ProcessName.ToUpper().Contem("SOFFICE")));
            if (t.Count > 0)
            {
                bool matar = false;
                if (confirmar)
                {
                    matar = "Há janelas do excel abertas. Para poder continuar, é necessário fecha-las. Se clicar em sim, o sistema fechará todas as janelas. Se deseja salvar algo, faça antes de confirmar essa tela. Se clicar em não, a operação será abortada.".Pergunta();
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
    }
}
