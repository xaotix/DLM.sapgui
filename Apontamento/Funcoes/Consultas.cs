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
using DLM.db;

namespace DLM.painel
{
    public class Util
    {
        public static string tostr(double previsto, bool porcentagem = false, int decimais = 0)
        {
            if (previsto < 0)
            {
                return "(" + previsto.ToString("N" + decimais.ToString()) + (porcentagem ? " %" : "") + ")";
            }
            return previsto != 0 ? previsto.ToString("N" + decimais.ToString()) + (porcentagem ? " %" : "") : "";
        }
    }
    public static class Consultas
    {

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
            contratos.AddRange(orcs.Select(x => x.PEP).Distinct().ToList());
            contratos.AddRange(cons.Select(x => x.PEP).Distinct().ToList());
            contratos = contratos.OrderBy(x => x).Distinct().ToList();
            foreach (var ct in contratos)
            {
                var real = reais.Find(x => x.pedido == ct);
                var orc = orcs.Find(x => x.PEP == ct);
                var con = cons.Find(x => x.PEP == ct);
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
            if (pedido.Count == 0)
            {
                return new List<PLAN_PECA>();
            }
            List<PLAN_PECA> retorno = new List<PLAN_PECA>();
            var sub_lista = quebrar_lista(pedido, max_pacote);
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

        public static List<ZPP0100_Resumo> GetResumoEmbarquesPEP(List<string> pedidos, Tipo_ZPP0100_Resumo tipo)
        {
            string tabela = "zpp0100_peps";
            if (tipo == Tipo_ZPP0100_Resumo.Subetapa)
            {
                tabela = "zpp0100_subetapas";
            }

            var comando = DBases.GetDB().Consulta(pedidos.Select(x => new Celula("pep", x)).ToList(), false, Cfg.Init.db_comum, tabela, "or");

            List<ZPP0100_Resumo> retorno = new List<ZPP0100_Resumo>();
            foreach (var p in comando.Linhas)
            {
                retorno.Add(new ZPP0100_Resumo(p));
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

            DBases.GetDB().Apagar("pedido_principal", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_obras_copia, false);
            DBases.GetDB().Apagar("pedido", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pedidos_copia, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pep_copia, false);

            DBases.GetDB().Apagar("contrato", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0066n_logistica, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn, false);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_cn47n, false);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_datas, false);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas, false);

            BufferObrasPesquisa.Clear();
        }




        public static List<StatusSAP_Planejamento> GetStatus(List<string> descricoes)
        {
            List<string> fim = new List<string>();
            foreach (var desc in descricoes)
            {
                fim.AddRange(desc.ToUpper().Split(' ').ToList().FindAll(x => x != "").Distinct().ToList());
            }
            fim = fim.Distinct().ToList().OrderBy(x => x).ToList();
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
            DateTime min_sistema = Cfg.Init.DataDummy();


            if (Tipo == Tipo_Meta.Engenharia)
            {
                var datas = lista.Select(x => x.engenharia_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList();

                datas.AddRange(lista.Select(x => x.engenharia_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
                if (datas.Count > 0)
                {
                    DateTime inicio = (DateTime)datas.Min();
                    DateTime fim = (DateTime)datas.Max();
                    fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
                    DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
                    f0 = GetRanges(lista, range, Tipo, Filtro, retorno, fim, f0);

                }


            }
            else if (Tipo == Tipo_Meta.Fabrica)
            {
                var datas = lista.Select(x => x.resumo_pecas.Fim).ToList().FindAll(x => x > min_sistema).ToList();
                datas.AddRange(lista.Select(x => x.resumo_pecas.Inicio).ToList().FindAll(x => x > min_sistema).ToList());
                datas.AddRange(lista.Select(x => x.fabrica_cronograma_inicio).ToList().FindAll(x => x > min_sistema).ToList());
                datas.AddRange(lista.Select(x => x.fabrica_cronograma).ToList().FindAll(x => x > min_sistema).ToList());
                if (datas.Count > 0)
                {
                    DateTime inicio = (DateTime)datas.Min();
                    DateTime fim = (DateTime)datas.Max();
                    fim = new DateTime(fim.Year, fim.Month, DateTime.DaysInMonth(fim.Year, fim.Month));
                    DateTime f0 = new DateTime(inicio.Year, inicio.Month, 01);
                    f0 = GetRanges(lista, range, Tipo, Filtro, retorno, fim, f0);

                }
            }


            return retorno.FindAll(x => x.SubEtapas.Count > 0).ToList();
        }
        private static DateTime GetRanges(List<PLAN_SUB_ETAPA> lista, Range_Meta range, Tipo_Meta Tipo, Tipo_Filtro_Meta Filtro, List<Meta> retorno, DateTime fim, DateTime f0)
        {
            if (range == Range_Meta.Mes)
            {
                while (f0 < fim)
                {

                    DateTime f0_fim = new DateTime(f0.Year, f0.Month, DateTime.DaysInMonth(f0.Day, f0.Month));
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
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
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
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
                    Meta mm = new Meta(lista, f0, f0_fim, Tipo, Filtro);
                    if (mm.Metas.Count > 0)
                    {
                        retorno.Add(mm);
                    }
                    f0 = f0.AddYears(1);
                }
            }

            return f0;
        }

        private static List<Resumo_Pecas> _resumo_pecas_peps { get; set; }
        private static List<PLAN_OBRA> _obras { get; set; }
        private static List<PLAN_PEDIDO> _pedidos { get; set; }
        private static List<PLAN_CONTRATO> _titulos_obras { get; set; }


        public static List<PLAN_CONTRATO> GetTitulosObras()
        {
            if (_titulos_obras == null)
            {
                _titulos_obras = new List<PLAN_CONTRATO>();

                var lista_fab = DBases.GetDB().Consulta(Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_contratos_copia);
                ConcurrentBag<PLAN_CONTRATO> retorno = new ConcurrentBag<PLAN_CONTRATO>();
                List<Task> Tarefas = new List<Task>();
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

                List<Task> Tarefas = new List<Task>();
                var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_04_obra);
                var titulos = GetTitulosObras();


                foreach (var obra in _obras)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        obra.Set(titulos, true);

                        db.Tabela igual = st_base.Filtrar("pep", obra.PEP, true);
                        if (igual.Count > 0)
                        {
                            obra.SetBase(igual.Linhas.First());
                        }

                    }));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();


                //_obras.AddRange(lista.ToList());
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
            Consultas.Pecas_Criar_Cache(contrato);
        }

        public static void Pecas_Criar_Cache(string contrato)
        {
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas, false);
            var pecas = Consultas.GetPecasReal(new List<string> { contrato });
            DLM.painel.Relatorios.ExportarEmbarque(pecas, false, null, null, true, false);
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
            var _etapas = new List<PLAN_ETAPA>();
            var subetapas = GetSubEtapas(pedidos);

            foreach (var et in subetapas.Select(x => x.etapa).OrderBy(x => x).Distinct().ToList())
            {
                _etapas.Add(new PLAN_ETAPA(subetapas.FindAll(x => x.etapa == et)));
            }

            var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_02_etapa);


            List<Task> Tarefas = new List<Task>();

            foreach (var etapa in _etapas)
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
                var ss = Buffer_Subetapas.FindAll(x => x.PEP.Contains(pedido.Replace("*", "")));
                _subetapas.AddRange(ss.FindAll(x => _subetapas.Find(y => y.PEP == x.PEP) == null));
                if (ss.Count == 0)
                {
                    consultas.Add(pedido);
                }
            }

            foreach (var pedido in consultas)
            {
                chave_pedidos = chave_pedidos + (chave_pedidos != "" ? " or " : $"select *  from {Cfg.Init.db_painel_de_obras2}.{Cfg.Init.tb_pep_copia} as pr where ") + $" pr.pep like '%{pedido}%'";
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
            //var lista_resumos = Consultas.getresumo_pecas_pep(pedidos);
            var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_01_subetapa);


            foreach (var t in _subetapas)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    //var t0 = lista_resumos.Find(x => x.pep == t.PEP);
                    //if (t0 != null)
                    //{
                    //    t.resumo_pecas = t0;
                    //}

                    var igual = st_base.Filtrar("pep", t.PEP.ToUpper().Replace(".P00", ""), true);
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



            var consulta = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_00_pep);
            foreach (var ret in retorno)
            {
                var igual = consulta.Filtrar("pep", ret.PEP, true);
                if (igual.Count > 0)
                {
                    ret.SetBase(igual.Linhas.First());
                }
            }


            return retorno;
        }

        public static List<PLAN_PECA_LOG> GetLogistica(List<PLAN_PECA> pecas, out List<PLAN_PECA> orfas)
        {
            orfas = new List<PLAN_PECA>();
            var pedidos = pecas.Select(x => x.pedido_completo).Distinct().ToList();
            pedidos = pedidos.FindAll(x => x.Length > 5).Distinct().ToList();

            if (pedidos.Count == 0) { return new List<PLAN_PECA_LOG>(); }
            var retorno = new List<PLAN_PECA_LOG>();

            foreach(var pedido in pedidos)
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

            var sem_peca = retorno.FindAll(x => x.peca == null).ToList();
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
            var consulta = Conexoes.DBases.GetDB().Consulta("pep", pep, Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao,false);
            var retorno = new List<PLAN_PECA_ZPMP>();
            foreach(var l in consulta.Linhas)
            {
                retorno.Add(new PLAN_PECA_ZPMP(l));
            }
            return retorno;
        }

        public static List<PLAN_PECA> GetPecasReal(List<string> lista_pedidos, int max_pacote = 10)
        {
            lista_pedidos = lista_pedidos.FindAll(x => x.Length > 5).Distinct().ToList();
            lista_pedidos = lista_pedidos.Select(x => x.Replace("*", "").Replace(" ", "")).ToList().FindAll(x => x != "").OrderBy(x=>x).ToList();

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
                var plan_pecas = new List<PLAN_PECA>();

                foreach (var linha in lista_pecas.Linhas)
                {
                    plan_pecas.Add(new PLAN_PECA(linha));
                }

                var peps_chaves = lista_embarques.Linhas.GroupBy(x => Conexoes.Utilz.PEP.Get.Subetapa(x.Get("Elemento_PEP").Valor, true)).ToList();
                foreach (var pep in peps_chaves)
                {
                    var pecas_pep = plan_pecas.FindAll(x => Conexoes.Utilz.PEP.Get.Subetapa(x.PEP, true) == pep.Key);
                    foreach (var peca in pecas_pep)
                    {
                        peca.SetStatusByZPP0100(pep.ToList());
                    }
                }
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
        public static List<PLAN_OBRA> BufferObrasPesquisa { get; set; } = new List<PLAN_OBRA>();

        private static List<string> _pedidos_clean { get; set; }
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

        public static List<PLAN_SUB_ETAPA> Buffer_Subetapas { get; set; } = new List<PLAN_SUB_ETAPA>();
        public static void SetPeps(List<PLAN_SUB_ETAPA> subetapas)
        {
            var sem_peps = subetapas.FindAll(x => !x.carregou_peps);
            var peps = DLM.painel.Consultas.GetPeps(sem_peps.Select(x => x.subetapa).Distinct().ToList());
            foreach (var sub in sem_peps)
            {
                sub.Set(peps);
            }
        }

        public static void LimparCOOISN(string chave)
        {
            DBases.GetDB().Apagar("pep", $"%{chave}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn, false);
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




        public static List<List<T>> quebrar_lista<T>(this List<T> locations, int maximo = 30)
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
