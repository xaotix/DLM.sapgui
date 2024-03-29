﻿using Conexoes;
using DLM.vars;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
    public class Buffer
    {
        public static List<Pedido_PMP> _Obras_PMP { get; set; }


        public static List<Pedido_PMP> Obras_PMP(bool recarregar = false)
        {

            if (true)
            {
                _Obras_PMP = new List<Pedido_PMP>();
                var reais = Pedidos();
                var orcs = Obras_PGO(recarregar);
                var cons = Obras_PGO_Consolidadas( recarregar);
                var contratos = reais.Select(x => x.pedido).Distinct().ToList();
                contratos.AddRange(orcs.Select(x => x.PEP).Distinct().ToList());
                contratos.AddRange(cons.Select(x => x.PEP).Distinct().ToList());
                contratos = contratos.OrderBy(x => x).Distinct().ToList().FindAll(x=>x.Length>5).ToList();
                foreach (var ct in contratos)
                {
                    var real = reais.Find(x => x.pedido == ct);
                    var orc = orcs.Find(x => x.PEP == ct);
                    var con = cons.Find(x => x.PEP == ct);
                    if (real != null | orc != null | con != null)
                    {
                        _Obras_PMP.Add(new Pedido_PMP(real, orc, con));
                    }
                    else
                    {

                    }
                }
            }
            return _Obras_PMP;
        }
        private static List<ORC_PED> _Obras_PGO { get; set; }
        public static List<ORC_PED> Obras_PGO(bool recarregar = false)
        {
            if (_Obras_PGO == null | recarregar)
            {
                _Obras_PGO = new List<ORC_PED>();
                _Obras_PGO = Consultas.GetObrasPGO();
            }
            return _Obras_PGO;
        }

        private static List<ORC_PED> _Obras_PGO_Consolidadas { get; set; }
        public static List<ORC_PED> Obras_PGO_Consolidadas(bool recarregar = false)
        {
            if (_Obras_PGO_Consolidadas == null | recarregar)
            {
                _Obras_PGO_Consolidadas = new List<ORC_PED>();
                _Obras_PGO_Consolidadas = Consultas.GetObrasPGO(true);

            }
            return _Obras_PGO_Consolidadas;
        }
        public static List<Conexoes.Bobina> Bobinas { get; set; } = new List<Conexoes.Bobina>();
        public static void Carregar()
        {
            List<Task> Tarefas = new List<Task>();
            Tarefas.Add(Task.Factory.StartNew(() => GetTitulosPedidos()));
            Tarefas.Add(Task.Factory.StartNew(() => Consultas.GetTitulosObras()));
            Tarefas.Add(Task.Factory.StartNew(() => Consultas.GetTitulosEtapas()));
            Tarefas.Add(Task.Factory.StartNew(() => Consultas.GetTitulosEtapas()));

            Tarefas.Add(Task.Factory.StartNew(() => Consultas.getresumo_pecas_obras()));
            Tarefas.Add(Task.Factory.StartNew(() => Consultas.getresumo_pecas_pedidos()));
            Tarefas.Add(Task.Factory.StartNew(() => Consultas.getresumo_pecas_subetapas()));
            Task.WaitAll(Tarefas.ToArray());
            Tarefas.Clear();

            Buffer.Obras();
            Buffer.Pedidos();
            Buffer.ObrasPorSegmento();
        }

        private static List<PLAN_OBRA> _Garantias { get; set; }
        private static List<PLAN_PEDIDO> _Pedidos { get; set; }
        private static List<PLAN_PEDIDO> _Pedidos_Principais { get; set; }

        public static List<PLAN_OBRA> GetObrasClone(bool Principais,bool Garantias)
        {
            List<PLAN_OBRA> retorno = new List<PLAN_OBRA>();
            if (Principais)
            {
                retorno.AddRange(GetObrasPrincipaisClone());
            }
            if (Garantias)
            {
                retorno.AddRange(GetObrasGarantiasClone());
            }

            return retorno;
        }
        public static List<PLAN_OBRA> GetObrasPrincipaisClone()
        {
            return Obras().Select(x => x.Clonar()).ToList();
        }

        public static List<PLAN_OBRA> GetObrasGarantiasClone()
        {
            return GetGarantias().Select(x => x.Clonar()).ToList();
        }
        public static List<PLAN_PEDIDO> GetPedidosPrincipais()
        {
            if (_Pedidos_Principais == null)
            {
                _Pedidos_Principais = Consultas.GetPedidos(new List<string> { "%P00" });
            }
            return _Pedidos_Principais;

        }
        public static List<PLAN_PEDIDO> Pedidos()
        {
            if (_Pedidos == null)
            {
                _Pedidos = Consultas.GetPedidos();
            }
            return _Pedidos;
        }
        private static List<PLAN_OBRA> _Obras { get; set; }
        public static List<PLAN_OBRA> Obras(bool copia = true, bool reset = false)
        {
            if (_Obras == null)
            {
                _Obras = Consultas.GetObras(copia,reset);
            }
            return _Obras;
        }

        private static List<PLAN_OBRAS> _ObrasPorSegmento { get; set; }
        public static List<PLAN_OBRAS> ObrasPorSegmento()
        {
            if (_ObrasPorSegmento == null)
            {
                _ObrasPorSegmento = new List<PLAN_OBRAS>();

                var segs = Obras().Select(x => x.setor_atividade).Distinct().ToList().OrderBy(x => x).ToList();
                var familias = DBases.GetSegmentos().FindAll(y => segs.Find(z => z == y.COD) != null).Select(x => x.FAMILIA).Distinct().ToList();

                foreach (var fam in familias)
                {
                    List<PLAN_OBRA> obras = new List<PLAN_OBRA>();
                    var segmentos = DBases.GetSegmentos().FindAll(x => x.FAMILIA == fam).Select(x => x.COD).Distinct().ToList();
                    string nome = DBases.GetSegmentos().Find(x => x.FAMILIA == fam).FAMILIA_DESC;
                    foreach (var ss in segmentos)
                    {
                        obras.AddRange(Obras().FindAll(x => x.setor_atividade == ss));
                    }
                    var nova = new PLAN_OBRAS(obras, fam, nome);



                    _ObrasPorSegmento.Add(nova);
                }

            }
            return _ObrasPorSegmento;
        }

        public static List<PLAN_OBRA> GetGarantias()
        {
            if (_Garantias == null)
            {
                _Garantias = Consultas.GetObras(new List<string> { ".G" },true,false);
            }
            return _Garantias;
        }
        private static List<Titulo_Planejamento> _Titulos_Pedidos { get; set; }
        public static List<Titulo_Planejamento> GetTitulosPedidos()
        {
            if (_Titulos_Pedidos != null) { return _Titulos_Pedidos; }
            _Titulos_Pedidos = new List<Titulo_Planejamento>();
            var lista_fab = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_titulos_pedidos);
            ConcurrentBag<Titulo_Planejamento> retorno = new ConcurrentBag<Titulo_Planejamento>();
            List<Task> Tarefas = new List<Task>();
            foreach (var s in lista_fab.Linhas)
            {
                Tarefas.Add(Task.Factory.StartNew(() => retorno.Add(new Titulo_Planejamento(s))));
            }
            Task.WaitAll(Tarefas.ToArray());
            _Titulos_Pedidos.AddRange(retorno);

            
            var lista_orcamento = DBases.GetDBPGO().Consulta(Cfg.Init.db_orcamento, Cfg.Init.tb_pmp_orc_resumo);

            foreach (var s in lista_orcamento.Linhas)
            {
                _Titulos_Pedidos.Add(new Titulo_Planejamento(s, true));
            }


            return _Titulos_Pedidos.OrderBy(x => x.CHAVE).ToList();
        }

        private static List<StatusSAP_Planejamento> _Status { get; set; }

        public static List<StatusSAP_Planejamento> GetStatus()
        {
            if (_Status == null)
            {
                _Status = new List<StatusSAP_Planejamento>();
                var lista_log = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_status_sap);
                foreach (var t in lista_log.Linhas)
                {
                    _Status.Add(new StatusSAP_Planejamento(t));
                }
            }
            return _Status;
        }

    }

}
