using System.Collections.Generic;
using System.Linq;

namespace DLM.painel
{
    public class Classificadores
    {
       
        public static List<Tipos_Pintura> GetPinturas(List<PLAN_PECA> Pecas)
        {
            List<Tipos_Pintura> pinturas = new List<Tipos_Pintura>();
            var s = Pecas.FindAll(x => x.superficie > 0).FindAll(x => x.TIPO_DE_PINTURA != "").Select(x => x.esq_de_pintura + " - " + x.TIPO_DE_PINTURA).Distinct().ToList();
            foreach (var p in s.OrderBy(x => x))
            {
                var ocs = Pecas.FindAll(x => x.esq_de_pintura + " - " + x.TIPO_DE_PINTURA == p);
                pinturas.Add(new DLM.painel.Tipos_Pintura(ocs));
            }
            return pinturas;
        }
        public static List<Resumo_SubEtapa> GetResumoSubEtapa(List<PLAN_PECA> Pecas)
        {
            List<DLM.painel.Resumo_SubEtapa> grupos = new List<DLM.painel.Resumo_SubEtapa>();

            var s = Pecas.Select(x => x.subetapa).Distinct().ToList();
            foreach (var p in s.OrderBy(x => x))
            {
                var ocs = Pecas.FindAll(x => x.subetapa == p);
                grupos.Add(new DLM.painel.Resumo_SubEtapa(ocs));
            }
            return grupos;
        }
        public static List<Unidade_fabril> GetUnidadesFabris(List<PLAN_PECA> Pecas)
        {
            List<DLM.painel.Unidade_fabril> grupos = new List<DLM.painel.Unidade_fabril>();
            var s = Pecas.Select(x => x.centro).Distinct().ToList();
            foreach (var p in s.OrderBy(x => x))
            {
                var ocs = Pecas.FindAll(x => x.centro == p);
                grupos.Add(new DLM.painel.Unidade_fabril(ocs));
            }
            return grupos;
        }
        public static List<Grupo_Mercadoria> GetGrupo_Mercadorias(List<PLAN_PECA> pecas, bool filtro_pep =false)
        {
            List<Grupo_Mercadoria> retorno = new List<Grupo_Mercadoria>();
         
            if (filtro_pep)
            {
                var peps = pecas.Select(x => x.PEP).Distinct().ToList().OrderBy(x => x).ToList();

              

                foreach (var pep in peps)
                {
                    var pecaspep = pecas.FindAll(x => x.PEP == pep).ToList();



                    var s = pecaspep.Select(x => x.grupo_mercadoria).Distinct().ToList();
                    foreach (var p in s.OrderBy(x => x))
                    {
                        var ocs = pecaspep.FindAll(x => x.grupo_mercadoria == p);
                        retorno.Add(new DLM.painel.Grupo_Mercadoria(ocs));
                    }


                }
                
            }
            else
            {
                var s = pecas.Select(x => x.grupo_mercadoria).Distinct().ToList();
                foreach (var p in s.OrderBy(x => x))
                {
                    var ocs = pecas.FindAll(x => x.grupo_mercadoria == p);
                    retorno.Add(new DLM.painel.Grupo_Mercadoria(ocs));
                }
            }
            return retorno;
        }

        public static List<Materia_Prima> GetMateriaPrima(List<PLAN_PECA> Pecas,bool separar_cortes = true)
        {
            List<Materia_Prima> retorno = new List<Materia_Prima>();
            var codigos = Pecas.Select(x => x.chave_material).Distinct().ToList().FindAll(x => x != "");
            foreach(var sub in codigos.FindAll(x=>x.Replace("0","").Replace(" ","")!=""))
            {
                var pcs = Pecas.FindAll(x => x.chave_material == sub);
                if(separar_cortes)
                {
                    var cortes = pcs.Select(x => x.corte_largura).Distinct().ToList();
                    foreach(var co in cortes)
                    {
                        retorno.Add(new Materia_Prima(pcs.FindAll(x => x.corte_largura == co)));
                    }
                }
                else
                {
   
                        retorno.Add(new Materia_Prima(pcs.FindAll(x => x.chave_material == sub)));
                }
            }
            return retorno;
        }

        public static List<Viga> GetVigas(List<PLAN_PECA> Pecas)
        {
            List<Viga> retorno = new List<Viga>();
            var pcs = Pecas.FindAll(x => x.grupo_mercadoria.Contains("VIGA") |x.grupo_mercadoria.Contains("PERFIL SOLDADO") | x.grupo_mercadoria.Contains("PERFIL LAMINADO"));
            var chaves = pcs.Select(x => x.grupo_mercadoria + "/" + x.Complexidade).Distinct().ToList();
            foreach(var chave in chaves)
            {
                retorno.Add(new Viga(pcs.FindAll(x => x.grupo_mercadoria + "/" + x.Complexidade == chave)));
            }
            return retorno;
        }
    }
}
