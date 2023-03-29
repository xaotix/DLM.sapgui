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
            if(previsto<0)
            {
                return "(" + previsto.ToString("N" + decimais.ToString()) + (porcentagem ? " %" : "") + ")";
            }
            return previsto != 0 ? previsto.ToString("N" + decimais.ToString()) + (porcentagem?" %":"") : "";
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
                DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento, false);
            }
        }
        public static List<ORC_PED> GetObrasPGO( bool consolidadas = false)
        {
            List<ORC_PED> retorno = new List<ORC_PED>();
            string tab = $"pmp_orc_resumo";
            if(consolidadas)
            {
                tab = $"pmp_orc_resumo_consolidada";
            }
            var s = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, tab);
            foreach(var p in s.Linhas)
            {
                var ped = new ORC_PED(p, consolidadas ? Tipo_Material.Consolidado : Tipo_Material.Orçamento);
                if(!consolidadas)
                {
                    ped.GetDatas();
                }
                if(ped.PEP.Length>0)
                {
                retorno.Add(ped);
                }
            }

            if(consolidadas)
            {
                var pacotes = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada_arquivos_peps).Linhas;
               foreach(var pedido in retorno)
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
            if(pedido.Count==0)
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
        public enum Tipo_ZPP0100_Resumo
        {
            Subetapa,
            PEP,
        }

        public static List<ZPP0100_Resumo> GetResumoEmbarquesPEP(List<string> pedidos, Tipo_ZPP0100_Resumo tipo)
        {
            string tabela = "zpp0100_peps";
            if(tipo == Tipo_ZPP0100_Resumo.Subetapa)
            {
                tabela = "zpp0100_subetapas";
            }

            var comando = DBases.GetDB().Consulta(pedidos.Select(x=> new Celula("pep", x)).ToList(),false, Cfg.Init.db_comum, tabela,"or");

            List<ZPP0100_Resumo> retorno = new List<ZPP0100_Resumo>();
            foreach(var p in comando.Linhas)
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


        public static void ApagarObra(string contrato)
        {
            contrato = contrato.Replace("*", "").Replace(" ", "").Replace("%", "");

            if (contrato.Length < 5)
            {
                return;
            }

            DBases.GetDB().Apagar("pedido_principal", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_obras_planejamento_copia, false);
            DBases.GetDB().Apagar("pedido", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pedidos_planejamento_copia, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento_m_copia, false);

            ApagarCacheTMP(contrato);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_consolidada, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_datas, false);

            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_pecas, false);

            BufferObrasPesquisa.Clear();
        }

        public static void ApagarCacheTMP(string contrato)
        {
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_pep_planejamento, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpmp_producao, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0066n_logistica, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zppcooisn, false);
            DBases.GetDB().Apagar("CHAVE", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_titulos_planejamento, false);

            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_zpp0100_embarques, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_folhamargem, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_resultado_economico_header, false);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_cji3, false);
            DBases.GetDB().Apagar("Elemento_PEP", $"%{contrato}%", Cfg.Init.db_comum, Cfg.Init.tb_fagll03, false);
            DBases.GetDB().Apagar("pep", $"%{contrato}%", Cfg.Init.db_painel_de_obras2, Cfg.Init.tb_cn47n, false);
        }





        private static List<PLAN_OBRA> _obras { get; set; }
        private static List<string> _pedidos_clean { get; set; }
        private static List<PLAN_PEDIDO> _pedidos { get; set; }



        public static List<PLAN_OBRA> GetObras(List<string> contrato, bool reset =false)
        {
            contrato = contrato.Distinct().ToList().FindAll(x => x.Length > 0);
            if (contrato.Count == 0)
            {
                return GetObras(reset);
            }

            List<PLAN_OBRA> retorno = new List<PLAN_OBRA>();
            foreach (var s in contrato)
            {
                retorno.AddRange(GetObras(reset).FindAll(x => x.PEP.Contains(s)));
            }
            retorno = retorno.GroupBy(x => x.PEP).Select(x => x.First()).ToList();

            return retorno;
        }
        public static List<PLAN_OBRA> GetObras(bool reset =false)
        {

            if(_obras==null | reset)
            {
                _obras = new List<PLAN_OBRA>();
                var consulta = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_obras_planejamento_copia);
                List<Task> Tarefas = new List<Task>();

                ConcurrentBag<PLAN_OBRA> lista = new ConcurrentBag<PLAN_OBRA>();

                foreach (var t in consulta.Linhas)
                {
                    Tarefas.Add(Task.Factory.StartNew(() => lista.Add(new PLAN_OBRA(t))));
                }
                Task.WaitAll(Tarefas.ToArray());
                Tarefas.Clear();

                _obras.AddRange(lista.ToList());
            }
            return _obras;
        }

        public static void CriarCache(List<string> contratos, bool apagar_tmp)
        {
            foreach(var contrato in contratos)
            {
                if (contrato.Length < 4) { return; }
                DBases.Painel_Criar_Cache(contrato);
                DBases.Painel_Apagar_Cache_Pecas(contrato);

            }
            var pecas = Consultas.GetPecasReal(contratos);
            DLM.painel.Relatorios.ExportarEmbarque(pecas, false, null, null, true, false);
            if(apagar_tmp)
            {
               foreach(var contrato in contratos)
                {
                    ApagarCacheTMP(contrato);
                }
            }
        }



        public static List<PLAN_PEDIDO> GetPedidos()
        {
            if (_pedidos != null) { return _pedidos; }
            _pedidos = new List<PLAN_PEDIDO>();
            var consulta = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_pedidos_planejamento_copia);
            var obras = GetObras();

            foreach (var pedido in consulta.Linhas)
            {
                _pedidos.Add(new PLAN_PEDIDO(pedido));
            }
            _pedidos = _pedidos.OrderBy(x => x.pedido).ToList();

            foreach (var obra in obras)
            {
                obra.Set(_pedidos);
            }
           
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
                pedidos.AddRange(GetPedidos().FindAll(x => x.PEP.Contains(s.Replace("%","").Replace("*",""))));
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

            //var titulos = GetTitulosEtapas();
            //var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_02_etapa);
           

            //List<Task> Tarefas = new List<Task>();

            //foreach (var etapa in _etapas)
            //{
            //    Tarefas.Add(Task.Factory.StartNew(() =>
            //    {
                   
            //        etapa.Set(titulos, false);

            //        var igual = st_base.Filtrar("pep", etapa.PEP.ToUpper().Replace(".P00", ""), true);
            //        if (igual.Count > 0)
            //        {
            //            etapa.SetBase(igual.Linhas.First());
            //        }

            //    }));
            //}
            //Task.WaitAll(Tarefas.ToArray());


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
                var ss = Buffer_Subetapas.FindAll(x => x.PEP.Contains(pedido.Replace("*", "")));
                _subetapas.AddRange(ss.FindAll(x => _subetapas.Find(y => y.PEP == x.PEP) == null));
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
            //var lista_resumos = Consultas.getresumo_pecas_subetapas(pedidos);
            //var st_base = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_01_subetapa);
            //var titulos = GetTitulosSubEtapas();


            //foreach (var t in _subetapas)
            //{
            //    Tarefas.Add(Task.Factory.StartNew(() =>
            //    {
            //        //var t0 = lista_resumos.Find(x => x.pep == t.PEP);
            //        //if (t0 != null)
            //        //{
            //        //    t.resumo_pecas = t0;
            //        //}
            //        t.Set(titulos, false);

            //        var igual = st_base.Filtrar("pep", t.PEP.ToUpper().Replace(".P00",""), true);
            //        if (igual.Count > 0)
            //        {
            //            t.SetBase(igual.Linhas.First());
            //        }

            //    }));
            //}
            //Task.WaitAll(Tarefas.ToArray());


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
            //setResumos(retorno);

            //var consulta = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_cbase_00_pep);
            //foreach (var ret in retorno)
            //{
            //    var igual = consulta.Filtrar("pep", ret.PEP, true);
            //    if (igual.Count > 0)
            //    {
            //        ret.SetBase(igual.Linhas.First());
            //    }
            //}


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

            string consulta2 = $"select * from {Cfg.Init.db_comum}.{Cfg.Init.tb_cargas}  as prod where " + chave.Replace("pep", "Elemento_PEP");

            //var lista_fab = dbase.Consulta(consulta);
            var lista_fab_0100 = DBases.GetDB().Consulta(consulta2);
            ConcurrentBag<Logistica_Planejamento> retorno = new ConcurrentBag<Logistica_Planejamento>();

            List<Task> Tarefas = new List<Task>();
     

            foreach (var linha in lista_fab_0100.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Logistica_Planejamento(pecas, linha, Tipo_Embarque.ZPP0100))));
            }
            Task.WaitAll(Tarefas.ToArray());
            Tarefas.Clear();

            var retorn = retorno.OrderBy(x => x.peca + "-" + x.desenho).ToList();


            for (int i = 0; i < pecas.Count; i++)
            {
                if (pecas[i].PEP.Length>0 && pecas[i].material!="")
                {
                    var logs = retorno.ToList().FindAll(x => x.pep == pecas[i].PEP && x.material == pecas[i].material);
                    pecas[i].SetLogistica(logs);
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


            var maximo = 10000;
            string consulta_emb = "" +
                "select prod.Elemento_PEP as Elemento_PEP," +
                "prod.Material as Material," +
                "prod.St_Embarque as St_Embarque," +
                "prod.Qtd_Embarque as Qtd_Embarque," +
                "prod.St_Conf_ as St_Conf_," +
                "prod.Tamanho_dimensao as Tamanho_dimensao" +
                $" from {Cfg.Init.db_painel_de_obras2}.{Cfg.Init.tb_zpp0100_embarques} as prod where ($P$) and prod.St_Conf_ = '@5Y@'".Replace("$P$", chave.Replace("pep", "Elemento_PEP"));
            var lista_fab = DBases.GetDB().Consulta(consulta);
            var lista_zpp0100 = DBases.GetDB().Consulta(consulta_emb);
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

            var peps_chaves = lista_zpp0100.Linhas.GroupBy(x => Conexoes.Utilz.PEP.Get.Subetapa(x.Get("Elemento_PEP").Valor,true));

            Tarefas = new List<Task>();
            foreach(var pep in peps_chaves)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    var querys = pep.ToList();
                    var pcs = retorno.ToList().FindAll(x => Conexoes.Utilz.PEP.Get.Subetapa(x.PEP, true) == pep.Key);
                    foreach (var pc in pcs)
                    {
                        pc.SetStatusByZPP0100(querys);
                    }
                }));

            }



           

            return retorno.OrderBy(x => x.PEP + "-" + x.desenho).ToList();
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

        public static List<string> GetPedidosClean(List<string> contratos, bool reset)
        {
            var pedidos = GetPedidosClean(reset);
            List<string> retorno = new List<string>();
            foreach(var contrato in contratos.Distinct().ToList().FindAll(x=>x.Length>5))
            {
                retorno.AddRange(pedidos.FindAll(x => x.Contains(contrato)));
            }
            return retorno.Distinct().ToList();
        }


        public static List<string> GetPedidosClean(bool update = false)
        {
            if(_pedidos_clean==null| update)
            {
                _pedidos_clean = new List<string>();
                _pedidos_clean.AddRange(DBases.GetDB().Consulta($"SELECT LEFT(pr.pep,13) AS pedido FROM {Cfg.Init.db_painel_de_obras2}.{Cfg.Init.tb_cn47n} AS pr GROUP BY LEFT(pr.pep,13)").Linhas.Select(x => x.Get("pedido").Valor).ToList());
                _pedidos_clean.AddRange(DBases.GetDB().Consulta($"SELECT LEFT(pr.pep,13) AS pedido FROM {Cfg.Init.db_comum}.pep_planejamento AS pr GROUP BY LEFT(pr.pep,13)").Linhas.Select(x => x.Get("pedido").Valor).ToList());

                _pedidos_clean = _pedidos_clean.Distinct().ToList().FindAll(x=>x.Length>3);
            }

            return _pedidos_clean;
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
