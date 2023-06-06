using Conexoes;
using DLM.vars;
using System.Collections.Generic;
using System.Linq;

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
                var reais = Consultas.GetPedidos();
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
            Consultas.GetTitulosObras();
            Consultas.GetObras();
            Consultas.GetPedidos();
            Buffer.ObrasPorSegmento();
        }

        private static List<PLAN_OBRA> _Garantias { get; set; }

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
            return Consultas.GetObras().Select(x => x.Clonar()).ToList();
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



        private static List<PLAN_OBRAS> _ObrasPorSegmento { get; set; }
        public static List<PLAN_OBRAS> ObrasPorSegmento()
        {
            if (_ObrasPorSegmento == null)
            {
                _ObrasPorSegmento = new List<PLAN_OBRAS>();

                var segs = Consultas.GetObras().Select(x => x.setor_atividade).Distinct().ToList().OrderBy(x => x).ToList();
                var familias = DBases.GetSegmentos().FindAll(y => segs.Find(z => z == y.COD) != null).Select(x => x.FAMILIA).Distinct().ToList();

                foreach (var fam in familias)
                {
                    List<PLAN_OBRA> obras = new List<PLAN_OBRA>();
                    var segmentos = DBases.GetSegmentos().FindAll(x => x.FAMILIA == fam).Select(x => x.COD).Distinct().ToList();
                    string nome = DBases.GetSegmentos().Find(x => x.FAMILIA == fam).FAMILIA_DESC;
                    foreach (var ss in segmentos)
                    {
                        obras.AddRange(Consultas.GetObras().FindAll(x => x.setor_atividade == ss));
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
                _Garantias = Consultas.GetObras(new List<string> { ".G" });
            }
            return _Garantias;
        }


        private static List<StatusSAP_Planejamento> _Status { get; set; }

        public static List<StatusSAP_Planejamento> GetStatus()
        {
            if (_Status == null)
            {
                _Status = new List<StatusSAP_Planejamento>();
                var lista_log = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_status_sap);
                foreach (var linha in lista_log.Linhas)
                {
                    _Status.Add(new StatusSAP_Planejamento(linha));
                }
            }
            return _Status;
        }

    }

}
