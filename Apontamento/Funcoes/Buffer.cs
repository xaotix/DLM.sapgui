using Conexoes;
using DLM.sapgui;
using DLM.vars;
using System.Collections.Generic;
using System.Linq;

namespace DLM.painel
{
    public class Buffer
    {
        private static List<StatusSAP_Planejamento> _Status { get; set; }
        private static List<PLAN_OBRAS> _obras_seg { get; set; }
        private static List<PLAN_PEDIDO> _Pedidos_Principais { get; set; }
        private static List<PLAN_OBRA> _Garantias { get; set; }
        private static List<ORC_PED> _Obras_PGO_Consolidadas { get; set; }
        public static List<Pedido_PMP> _pedidos_pmp { get; set; }


        public static List<Pedido_PMP> Pedidos_PMP(bool recarregar = false)
        {

            if (true)
            {
                _pedidos_pmp = new List<Pedido_PMP>();
                var pmps = new List<Pedido_PMP>();
                var reais = Consultas.GetPedidos(recarregar);
                var cons = Obras_PGO_Consolidadas(recarregar);
                var contratos = reais.Select(x => x.pedido).Distinct().ToList();
                contratos.AddRange(cons.Select(x => x.PEP).Distinct().ToList());


                contratos = contratos.OrderBy(x => x).Distinct().ToList();
                foreach (var ct in contratos)
                {
                    var real = reais.Find(x => x.pedido == ct);
                    var con = cons.Find(x => x.PEP == ct);
                    if (real != null |  con != null)
                    {
                        pmps.Add(new Pedido_PMP(real, null, con));
                    }
                    else
                    {

                    }
                }


                var reais_SAP = DLM.SAP.GetPedidos(recarregar);

                var peds = new List<string>();
                peds.AddRange(reais.Select(x => x.PEP));
                peds.AddRange(pmps.Select(x => x.pep));
                peds = peds.Distinct().ToList().OrderBy(x => x).ToList();

                foreach (var pedido in peds)
                {
                    var pmp = pmps.Find(x => x.pep == pedido);
                    var real = reais_SAP.Find(x => x.PEP == pedido);
                    if (pmp != null)
                    {
                        if (real != null)
                        {
                            pmp.SAP = true;
                            pmp.Finalizado = real.Finalizado;
                            pmp.Terceirizacao = real.Terceirizacao;
                        }
                        _pedidos_pmp.Add(pmp);
                    }
                    else if (real != null)
                    {
                        if(!real.Finalizado)
                        {
                            _pedidos_pmp.Add(new DLM.painel.Pedido_PMP(real));
                        }
                    }
                }
            }
            return _pedidos_pmp;
        }


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
            Consultas.GetPedidosContratos();
            Consultas.GetObras();
            Consultas.GetPedidos();
            Buffer.ObrasPorSegmento();
        }



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



        public static List<PLAN_OBRAS> ObrasPorSegmento()
        {
            if (_obras_seg == null)
            {
                _obras_seg = new List<PLAN_OBRAS>();

                var lista = Consultas.GetObras();

                _obras_seg.Add(new PLAN_OBRAS(lista.FindAll(x => !x.contrato.StartsW("90") && x.setor_atividade!="80"), "", "Nacional"));
                _obras_seg.Add(new PLAN_OBRAS(lista.FindAll(x => x.contrato.StartsW("90") && x.setor_atividade!="80"), "", "Exportação"));
                _obras_seg.Add(new PLAN_OBRAS(lista.FindAll(x =>  x.setor_atividade=="80"), "", "Serviços Técnicos"));

                //var segs = lista.Select(x => x.setor_atividade).Distinct().ToList().OrderBy(x => x).ToList();
                //var familias = DBases.GetSegmentos().FindAll(y => segs.Find(z => z == y.id.ToString()) != null).Select(x => x.FAMILIA).Distinct().ToList();

                //foreach (var fam in familias)
                //{
                //    var obras = new List<PLAN_OBRA>();
                //    var segmentos = DBases.GetSegmentos().FindAll(x => x.FAMILIA == fam).Select(x => x.id.ToString()).Distinct().ToList();
                //    string nome = DBases.GetSegmentos().Find(x => x.FAMILIA == fam).FAMILIA_DESC;
                //    foreach (var ss in segmentos)
                //    {
                //        obras.AddRange(lista.FindAll(x => x.setor_atividade == ss));
                //    }
                //    var nova = new PLAN_OBRAS(obras, fam, nome);
                //    _ObrasPorSegmento.Add(nova);
                //}

            }
            return _obras_seg;
        }

        public static List<PLAN_OBRA> GetGarantias()
        {
            if (_Garantias == null)
            {
                _Garantias = Consultas.GetObras(new List<string> { ".G" });
            }
            return _Garantias;
        }



        public static List<StatusSAP_Planejamento> GetStatus()
        {
            if (_Status == null)
            {
                _Status = new List<StatusSAP_Planejamento>();
                var consulta = DBases.GetDB().Consulta(Cfg.Init.db_comum, Cfg.Init.tb_status_sap);
                foreach (var linha in consulta)
                {
                    _Status.Add(new StatusSAP_Planejamento(linha));
                }
            }
            return _Status;
        }

    }

}
