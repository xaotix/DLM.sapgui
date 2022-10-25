using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.sapgui
{
    //public enum CJ20N_Tipo
    //{
    //    Pedido,
    //    Etapa,
    //    SubEtapa,
    //    PEP,
    //    Tarefa,
    //    Desconhecido,
    //}
    public enum ZPP0100_Tipo
    {
        Monitor_Embarque,

    }
    public enum Tipo_Valor
    {
        Realizado,
        Empenhado,
        Previsto,
    }
    public enum Tipo_Lancamento
    {
        _,
        PEP_Consolidada_ENG,
        PEP_Consolidada_FAB,
        PEP_Consolidada_LOG,
        PEP_Consolidada_MON,
        PEP_Real,
        Receita_Bruta,
        Receita_Bruta_Projeto,
        Receita_Bruta_Materiais,
        Receita_Bruta_Montagem,
        Deducoes,
        Receita_Liquida,
        Custos_Projeto,
        Custos_Material,
        Custos_Material_MP,
        Custos_Material_MOD,
        Custos_Material_GGF,
        Custos_Material_Terceirizacao,
        Custos_Logistica,
        Custos_Montagem,
        Custos_Montagem_MO_DI,
        Custos_Montagem_Equipamentos,
        Custos_Montagem_Equipe_Propria,
        Custos_Montagem_Supervisor,
        Custos_Seguros,
        Custos_Terceirizacao_Projeto,
        Custos_Demais,
        Engenharia_Peso,
        Engenharia_Custo,
        Engenharia_Custo_KG,
        Fabricação_Peso,
        Fabricação_Custo,
        Fabricação_Custo_KG,
        Logística_Peso,
        Logística_Custo,
        Logística_Custo_KG,
        EBITDA,
    }
    public enum TAB_ZPP0112
    {
        sel = 0,
        Nro_Carga = 1,
        Elemento_PEP = 2,
        Centro = 3,
        Fornecedor = 4,
        Tipo_Veiculo = 5,
        Num_Placa = 6,
        Motorista = 7,
        RG = 8,
        Telefone = 9,
        VAL2 = 10,
        Telefone_2 = 11,
        Fornecedor_2 = 12,
        Telefone_3 = 13,
        Observacoes = 14,
    }
    public enum Setor
    {
        ENGENHARIA,
        FÁBRICA,
        LOGÍSTICA,
        MONTAGEM
    }
    public enum Range_Meta
    {
        Semana,
        Mes,
        Ano,
    }
    public enum Tipo_Meta
    {
        Engenharia,
        Fabrica,
        Logistca,
        Montagem,
        Tudo
    }
    public enum Tipo_Filtro_Meta
    {
        Etapa,
        Peca,
    }
    public enum Tipo_Material
    {
        Real,
        Orçamento,
        Consolidado,
        _,
    }
    public enum Tipo_Embarque
    {
        ZPP0100,
        ZPP0066N,
    }

    public enum TAB_ZPP0100
    {
        booleano = 0,
        Ordem_Embarque = 1,
        Qtd_Embarque = 2,
        Nro_Carga = 3,
        St_Embarque = 4,
        St_Carga = 5,
        Etq_Material = 6,
        Material = 7,
        Descricao = 8,
        Tamanho_dimensao = 9,
        Comprimento = 10,
        Peso_item_Tot = 11,
        Nome_da_Obra = 12,
        Elemento_PEP = 13,
        Centro = 14,
        Etq_Impressa = 15,
        Etq_Volume = 16,
        Status = 17,
        Qtd_Carregada = 18,
        Sld_1202 = 19,
        Sld_1203 = 20,
        Sld_1204 = 21,
        St_Conf_ = 22,
        St_DtProg_ = 23,
        Data = 24,
        Ordem_Prod_ = 25,
        Apontamento_Fert = 26,
    }
    public enum TAB_ZCONTRATOS
    {
        Empresa = 0,
        Cen = 1,
        Contrato = 2,
        Cliente = 3,
        CNPJ = 4,
        Razao_Social_Cliente = 5,
        SetInd = 6,
        UF = 7,
        Situacao = 8,
        Devolucoes = 9,
        Nome_da_obra = 10,
        Elemento_PEP = 11,
        Contas_a_receber = 12,
        Receita = 13,
        Cotacao = 14,
        Itm = 15,
        Ordem_venda = 16,
        TpDV = 17,
        Fatura = 18,
        TipFt = 19,
        Descr_tipo_fat = 20,
        NF = 21,
        Data_emissao = 22,
        Material = 23,
        CFOP = 24,
        Quantidade = 25,
        UM = 26,
        Valor_unit = 27,
        Peso_liq = 28,
        Valor_total_NF = 29,
    }

}
