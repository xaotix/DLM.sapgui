﻿using OfficeOpenXml;
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
using DLM.db;

namespace DLM.painel
{
    public static class Consultas
    {
        private static List<PLAN_OBRA> _obras { get; set; }
        private static List<PLAN_PEDIDO> _pedidos { get; set; }
        private static List<string> _pedidos_clean { get; set; }
        private static List<PLAN_CONTRATO> _titulos_obras { get; set; }

        public static List<Carga_Planejamento> getCargas(List<PLAN_PECA_LOG> logs)
        {
            List<Carga_Planejamento> retorno = new List<Carga_Planejamento>();

            foreach (var s in logs.Select(x => x.num_carga).Distinct().ToList())
            {
                retorno.Add(new Carga_Planejamento(s, logs));
            }

            return retorno;
        }
        public static List<SubEtapa_Logistica_Planejamento> getSubEtapasLogistica(List<PLAN_PECA_LOG> logs)
        {
            List<SubEtapa_Logistica_Planejamento> retorno = new List<SubEtapa_Logistica_Planejamento>();

            foreach (var s in logs.Select(x => x.subetapa).Distinct().ToList())
            {
                retorno.Add(new SubEtapa_Logistica_Planejamento(s, logs));
            }

            return retorno;
        }
        public static List<PackList_Planejamento> getPackLists(List<PLAN_PECA_LOG> logs)
        {
            List<PackList_Planejamento> retorno = new List<PackList_Planejamento>();

            foreach (var s in logs.Select(x => x.pack_list).Distinct().ToList())
            {
                retorno.Add(new PackList_Planejamento(s, logs));
            }

            return retorno;
        }
        public static List<ORC_PED> GetObrasPGO(bool consolidadas = false)
        {
            List<ORC_PED> retorno = new List<ORC_PED>();
            string tab = $"pmp_orc_resumo";
            if (consolidadas)
            {
                tab = $"pmp_orc_resumo_consolidada";
            }
            var s = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, tab);
            foreach (var p in s.Linhas)
            {
                var ped = new ORC_PED(p, consolidadas ? Tipo_Material.Consolidado : Tipo_Material.Orçamento);
                if (!consolidadas)
                {
                    ped.GetDatas();
                }
                if (ped.PEP.Length > 0)
                {
                    retorno.Add(ped);
                }
            }

            if (consolidadas)
            {
                var pacotes = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada_arquivos_peps).Linhas;
                foreach (var pedido in retorno)
                {
                    var pcks = pacotes.FindAll(x => x.Get("pedido").Valor == pedido.PEP);
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
        public static List<ORC_PED> GetObrasPGO(string chave, bool consolidadas = false, bool consolidadas_x_real = false)
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
                    var pcks = pacotes.FindAll(x => x.Get("pedido").Valor == pedido.PEP);
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
            List<string> pedidos_real = pedidos.FindAll(x => x.Material_REAL).Select(x => x.pep).Distinct().ToList();
            List<string> pedidos_orcamento = pedidos.FindAll(x => x.Material_ORC).Select(x => x.pep).Distinct().ToList();
            List<string> pedidos_consolidados = pedidos.FindAll(x => x.Material_CONS).Select(x => x.pep).Distinct().ToList();

            var w = Conexoes.Utilz.Wait(pedidos_real.Count * 4 + 3, $"Mapeando peças...{pedidos.Count} pedidos...");

            int tam_pacote = 20;

            var reais_pecas = GetPecasReal(pedidos_real, tam_pacote);
            w.somaProgresso();
            var orc_pecas = GetPecasPGO(pedidos_orcamento, tam_pacote);
            w.somaProgresso();
            var cons_pecas = GetPecasPGO(pedidos_consolidados, tam_pacote, true);
            w.somaProgresso();

            foreach (var real in pedidos_real)
            {
                var ped = pedidos.Find(x => x.pep == real);
                if (ped != null)
                {
                    ped.Real.Set(reais_pecas);
                }
                w.somaProgresso();
            }
            foreach (var orc in pedidos_orcamento)
            {
                var ped = pedidos.Find(x => x.pep == orc);
                if (ped != null)
                {
                    ped.Orcamento.Set(orc_pecas);
                }
                w.somaProgresso();
            }
            foreach (var orc in pedidos_consolidados)
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



            var consulta = DBases.GetDBPGO().Consulta(chave_pedidos);
            foreach (var ss in consulta.Linhas)
            {
                ORC_ETP pc = new ORC_ETP(ss);
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
        public enum Tipo_ZPP0100_Resumo
        {
            Subetapa,
            PEP,
        }

        public static List<ZPP0100_Resumo> GetResumoEmbarquesPEP(List<string> pedidos, int coluna = 24)
        {
            var retorno = new List<ZPP0100_Resumo>();

            foreach (var pedido in pedidos)
            {
                var comando = DBases.GetDB().Consulta($"call comum.getzpp0100_resumo('{pedido}',{coluna})");

                foreach (var p in comando.Linhas)
                {
                    retorno.Add(new ZPP0100_Resumo(p));
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
            foreach (var linha in consulta.Linhas)
            {
                var peca = new ORC_SUB(linha);
                if (!consolidada)
                {
                    peca.GetDatas();
                }
                retorno.Add(peca);
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
            if (consolidada)
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


        public static void ApagarObra(string contrato)
        {
            contrato = contrato.Replace("*", "").Replace(" ", "").Replace("%", "");

            if (contrato.Length < 5)
            {
                return;
            }

            DBases.GetDB().Apagar("pedido_principal", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_obras_copia);
            DBases.GetDB().Apagar("pedido", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pedidos_copia);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pep_copia);

            DBases.GetDB().Apagar("contrato", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao);
            //DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0066n_logistica);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_datas);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas);
        }




        //public static List<StatusSAP_Planejamento> GetStatus(List<string> descricoes)
        //{
        //    List<string> fim = new List<string>();
        //    foreach (var desc in descricoes)
        //    {
        //        fim.AddRange(desc.ToUpper().Split(' ').ToList().FindAll(x => x != "").Distinct().ToList());
        //    }
        //    fim = fim.Distinct().ToList().OrderBy(x => x).ToList();
        //    List<StatusSAP_Planejamento> retorno = new List<StatusSAP_Planejamento>();
        //    foreach (var ss in fim)
        //    {
        //        var tt = Buffer.GetStatus().Find(x => x.status == ss);
        //        if (tt != null)
        //        {
        //            retorno.Add(tt);
        //        }

        //    }
        //    return retorno;
        //}

        //public static List<Meta> GetMeta(List<PLAN_SUB_ETAPA> lista, Range_Meta range = Range_Meta.Mes, Tipo_Meta Tipo = Tipo_Meta.Tudo, Tipo_Filtro_Meta Filtro = Tipo_Filtro_Meta.Etapa)
        //{
        //    List<Meta> retorno = new List<Meta>();
        //    DateTime min_sistema = Cfg.Init.DataDummy();


        //    if (Tipo == Tipo_Meta.Engenharia)
        //    {
        //        var datas = lista.Select(x => x.engenharia_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList();

        //        datas.AddRange(lista.Select(x => x.engenharia_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
        //        if (datas.Count > 0)
        //        {
        //            DateTime inicio = (DateTime)datas.Min();
        //            DateTime fim = (DateTime)datas.Max();
        //            fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
        //            DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
        //            f0 = GetRanges(lista, range, Tipo, Filtro, retorno, fim, f0);

        //        }


        //    }
        //    else if (Tipo == Tipo_Meta.Fabrica)
        //    {
        //        var datas = lista.Select(x => x.resumo_pecas.Fim).ToList().FindAll(x => x > min_sistema).ToList();
        //        datas.AddRange(lista.Select(x => x.resumo_pecas.Inicio).ToList().FindAll(x => x > min_sistema).ToList());
        //        datas.AddRange(lista.Select(x => x.fabrica_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList());
        //        datas.AddRange(lista.Select(x => x.fabrica_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
        //        if (datas.Count > 0)
        //        {
        //            DateTime inicio = (DateTime)datas.Min();
        //            DateTime fim = (DateTime)datas.Max();
        //            fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
        //            DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
        //            f0 = GetRanges(lista, range, Tipo, Filtro, retorno, fim, f0);

        //        }
        //    }


        //    return retorno.FindAll(x => x.SubEtapas.Count > 0).ToList();
        //}
        //private static DateTime GetRanges(List<PLAN_SUB_ETAPA> lista, Range_Meta range, Tipo_Meta Tipo, Tipo_Filtro_Meta Filtro, List<Meta> retorno, DateTime fim, DateTime f0)
        //{
        //    if (range == Range_Meta.Mes)
        //    {
        //        while (f0 < fim)
        //        {

        //            DateTime f0_fim = new DateTime(f0.Year, f0.Month, DateTime.DaysInMonth(f0.Day, f0.Month));
        //            Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
        //            if (mm.Metas.Count > 0)
        //            {
        //                retorno.Add(mm);
        //            }
        //            f0 = f0.AddMonths(1);
        //        }
        //    }
        //    else if (range == Range_Meta.Semana)
        //    {
        //        while (f0 < fim)
        //        {

        //            DateTime f0_fim = f0.AddDays(7);
        //            Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
        //            if (mm.Metas.Count > 0)
        //            {
        //                retorno.Add(mm);
        //            }
        //            f0 = f0.AddDays(7);
        //        }
        //    }

        //    else if (range == Range_Meta.Ano)
        //    {
        //        while (f0 < fim)
        //        {

        //            DateTime f0_fim = f0.AddYears(1);
        //            Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
        //            if (mm.Metas.Count > 0)
        //            {
        //                retorno.Add(mm);
        //            }
        //            f0 = f0.AddYears(1);
        //        }
        //    }

        //    return f0;
        //}




        public static List<PLAN_CONTRATO> GetTitulosObras()
        {
            if (_titulos_obras == null)
            {
                _titulos_obras = new List<PLAN_CONTRATO>();
                var lista_fab = DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
                var retorno = new ConcurrentBag<PLAN_CONTRATO>();
                var Tarefas = new List<Task>();
                foreach (var s in lista_fab.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    retorno.Add(new PLAN_CONTRATO(s))
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

                foreach (var t in consulta.Linhas)
                {
                    _obras.Add(new PLAN_OBRA(t));
                }

                var Tarefas = new List<Task>();
                var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_04_obra);

                foreach (var obra in _obras)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        var igual = st_base.Filtrar("pep", obra.PEP, true);
                        if (igual.Count > 0)
                        {
                            obra.SetBase(igual.Linhas.First());
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
            obras = obras.Distinct().ToList().FindAll(x => x.Length > 0);
            if (obras.Count == 0)
            {
                return GetObras(reset);
            }

            var retorno = new List<PLAN_OBRA>();
            foreach (var obra in obras)
            {
                retorno.AddRange(GetObras(reset).FindAll(x => x.PEP.Contains(obra)));
            }
            retorno = retorno.GroupBy(x => x.PEP).Select(x => x.First()).ToList();

            return retorno;
        }

        public static void CriarCache(string contrato)
        {
            if (contrato.Length < 4) { return; }
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
            var mindia = Cfg.Init.DataDummy();


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
            ldb.Add("bobina", peca_log.peca.bobina.SAP);
            ldb.Add("face1", peca_log.peca.bobina.cor1.Nome);
            ldb.Add("face2", peca_log.peca.bobina.cor2.Nome);
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
                foreach (var t in consulta.Linhas)
                {
                    lista.Add(new PLAN_PEDIDO(t, new PLAN_OBRA()));
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
                        var igual = st_base.Filtrar("pep", t.PEP, true);
                        if (igual.Count > 0)
                        {
                            t.SetBase(igual.Linhas.First());
                        }
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
            }




            return _pedidos;
        }


        public static List<PLAN_PEDIDO> GetPedidos(List<string> contrato)
        {

            contrato = contrato.Distinct().ToList().FindAll(x => x.Length > 0);
            if (contrato.Count == 0)
            {
                return GetPedidos();
            }

            List<PLAN_PEDIDO> pedidos = new List<PLAN_PEDIDO>();
            foreach (var s in contrato)
            {
                pedidos.AddRange(GetPedidos().FindAll(x => x.PEP.Contains(s.Replace("%", "").Replace("*", ""))));
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
                    var igual = st_base.Filtrar("pep", etapa.PEP.ToUpper().Replace(".P00", ""), true);
                    if (igual.Count > 0)
                    {
                        etapa.SetBase(igual.Linhas.First());
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

            foreach (var pack in consulta.Linhas.Quebrar(500))
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
                    var igual = st_base.Filtrar("pep", subetapa.PEP.ToUpper().Replace(".P00", ""), true);
                    if (igual.Count > 0)
                    {
                        subetapa.SetBase(igual.Linhas.First());
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
                        chave = $"{tabela}.pep like '%{chaves_consulta[i] }%'";
                    }
                    else
                    {
                        chave = chave + $" or {tabela}.pep like '%{chaves_consulta[i] }%'";
                    }
                }

                var consulta1 = DBases.GetDB().Consulta($"select * from {tabela} where {chave}");

                foreach (var pep in consulta1.Linhas)
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
                    var igual = consulta2.Filtrar("pep", ret.PEP, true);
                    if (igual.Count > 0)
                    {
                        ret.SetBase(igual.Linhas.First());
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
            pedidos = pedidos.FindAll(x => x.Length > 5).Distinct().ToList();

            if (pedidos.Count == 0) { return new List<PLAN_PECA_LOG>(); }
            var retorno = new List<PLAN_PECA_LOG>();

            foreach (var pedido in pedidos)
            {
                var lista_fab_0100 = DBases.GetDB().Consulta($"call comum.getzpp0100_cargas('{pedido}')");
                foreach (var linha in lista_fab_0100.Linhas)
                {
                    retorno.Add(new PLAN_PECA_LOG(pecas, linha, Tipo_Embarque.ZPP0100));
                }
            }
            for (int i = 0; i < pecas.Count; i++)
            {
                if (pecas[i].PEP.Length > 0 && pecas[i].material != "")
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
            foreach (var l in consulta.Linhas)
            {
                retorno.Add(new PLAN_PECA_ZPMP(l));
            }
            return retorno;
        }

        public static List<PLAN_PECA> GetPecasReal(List<string> lista_pedidos, int max_pacote = 10)
        {
            lista_pedidos = lista_pedidos.FindAll(x => x.Length > 5).Distinct().ToList();
            lista_pedidos = lista_pedidos.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "").OrderBy(x => x).ToList();

            if (lista_pedidos.Count == 0) { return new List<PLAN_PECA>(); }
            var retorno = new List<PLAN_PECA>();

            var tabelas_pecas = new List<DLM.db.Tabela>();
            var tabelas_embarques = new List<DLM.db.Tabela>();
            foreach (var pedido in lista_pedidos)
            {
                var lista_pecas = DBases.GetDB().Consulta($"call comum.getpecas('{pedido}')");
                var lista_embarques = DBases.GetDB().Consulta($"call comum.getPecasEmbarques('{pedido}')");
                tabelas_pecas.Add(lista_pecas);
                tabelas_embarques.Add(lista_embarques);
            }

            for (int i = 0; i < tabelas_pecas.Count; i++)
            {
                var lista_pecas = tabelas_pecas[i];
                var lista_embarques = tabelas_embarques[i];
                var _pecas = new ConcurrentBag<PLAN_PECA>();
                var Tarefas = new List<Task>();
                foreach (var linha in lista_pecas.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        _pecas.Add(new PLAN_PECA(linha));
                    }));
                }
                Task.WaitAll(Tarefas.ToArray());

                var plan_pecas = new List<PLAN_PECA>();
                plan_pecas.AddRange(_pecas);

                var peps_chaves = lista_embarques.Linhas.GroupBy(x => Conexoes.Utilz.PEP.Get.Subetapa(x.Get("Elemento_PEP").Valor, true)).ToList();
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

        public static List<string> GetPedidosClean(List<string> contratos, bool reset)
        {
            var pedidos = GetPedidosClean(reset);
            List<string> retorno = new List<string>();
            foreach (var contrato in contratos.Distinct().ToList().FindAll(x => x.Length > 5))
            {
                retorno.AddRange(pedidos.FindAll(x => x.Contains(contrato)));
            }
            return retorno.Distinct().ToList();
        }
        public static List<string> GetPedidosClean(bool update = false)
        {
            if (_pedidos_clean == null | update)
            {
                _pedidos_clean = new List<string>();
                _pedidos_clean.AddRange(DBases.GetDB().Consulta($"SELECT LEFT(pr.pep,13) AS pedido FROM {Cfg.Init.db_comum}.cn47n AS pr GROUP BY LEFT(pr.pep,13)").Linhas.Select(x => x.Get("pedido").Valor).ToList());
                _pedidos_clean.AddRange(DBases.GetDB().Consulta($"SELECT LEFT(pr.pep,13) AS pedido FROM {Cfg.Init.db_comum}.{Cfg.Init.tb_pep_planejamento} AS pr GROUP BY LEFT(pr.pep,13)").Linhas.Select(x => x.Get("pedido").Valor).ToList());

                _pedidos_clean = _pedidos_clean.Distinct().ToList().FindAll(x => x.Length > 3);
            }

            return _pedidos_clean;
        }
        public static void SetPeps(List<PLAN_SUB_ETAPA> subetapas)
        {
            var sem_peps = subetapas.FindAll(x => !x.carregou_peps);
            var peps = DLM.painel.Consultas.GetPepsReal(sem_peps.Select(x => x.subetapa).Distinct().ToList());
            foreach (var sub in sem_peps)
            {
                sub.Set(peps);
            }
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
    }
}
