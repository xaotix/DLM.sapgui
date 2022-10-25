using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
    public class Avanco_Pecas
    {
        public void Salvar()
        {
            DLM.db.Linha l = new DLM.db.Linha();
            l.Add("pep", this.pep);
            l.Add("predio", this.predio);
            l.Add("status_geral", this.status_geral);
            l.Add("etapa", this.etapa);
            l.Add("marca", this.marca);
            l.Add("carga", this.carga);
            l.Add("tipo", this.tipo);
            l.Add("peso", this.peso);
            l.Add("quantidade", this.quantidade);
            l.Add("peso_pedido", this.peso_pedido);
            if(this.pep=="")
            {
                return;
            }
            DBases.GetDB().Cadastro(l.Celulas, Cfg.Init.db_comum, "avanco_pecas");
        }
        public string pep { get; set; } = "";
        public string predio { get; set; } = "";
        public string status_geral { get; set; } = "";
        public string etapa { get; set; } = "";
        public string marca { get; set; } = "";
        public string carga { get; set; } = "";
        public string tipo { get; set; } = "";
        public double peso { get; set; } = 0;
        public double quantidade { get; set; } = 0;
        public double peso_pedido { get; set; } = 0;

        public double porcentagem_tot
        {
            get
            {
                if(peso_pedido>0 && peso>0)
                {
                    return (peso * quantidade)/ peso_pedido;
                }
                return 0;
            }
        }
        public double porcentagem_unit
        {
            get
            {
                if (peso_pedido > 0 && peso > 0)
                {
                    return (peso) / peso_pedido;
                }
                return 0;
            }
        }
        public Avanco_Pecas(Logistica_Planejamento s, PLAN_PEDIDO pedido)
        {
            this.pep = s.pep;
            this.predio = pedido.nome;
            this.etapa = Conexoes.Utilz.PEP.Get.Etapa(this.pep);
            this.marca = marca;
            this.carga = s.num_carga;
            this.tipo = s.peca.grupo_mercadoria;
            this.peso = s.peca.peso_unitario;
            this.peso_pedido = pedido.peso_planejado;
            this.quantidade = s.quantidade;

           
            if(s.peca.total_fabricado>0 && s.peca.total_fabricado<s.peca.qtd_necessaria)
            {
                if(s.quantidade<this.quantidade)
                {
                    status_geral = "02 - FABRICAÇÃO";
                }
            }
            else if(s.peca.total_fabricado ==0)
            {
                status_geral = "01 - ENGENHARIA";
            }


        }
        public Avanco_Pecas()
        {

        }
        public Avanco_Pecas(DLM.db.Linha db)
        {
            this.pep = db.Get("pep").ToString();
            this.predio = db.Get("predio").ToString();
            this.status_geral = db.Get("status_geral").ToString();
            this.etapa = db.Get("etapa").ToString();
            this.marca = db.Get("marca").ToString();
            this.carga = db.Get("carga").ToString();
            this.tipo = db.Get("tipo").ToString();
            this.peso = db.Get("peso").Double();
            this.quantidade = db.Get("quantidade").Double();
            this.peso_pedido = db.Get("peso_pedido").Double();
        }
    }
}
