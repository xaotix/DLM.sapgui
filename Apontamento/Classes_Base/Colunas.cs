using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.painel
{
   public class Colunas
    {
        public class FAGLL03
        {
            public static int Atribuicao { get; set; } = 0;
            public static int N_documento { get; set; } = 1;
            public static int Divisao { get; set; } = 2;
            public static int Tipo_de_documento { get; set; } = 3;
            public static int Data_do_documento { get; set; } = 4;
            public static int Chave_de_lancamento { get; set; } = 5;
            public static int Montante_em_moeda_interna { get; set; } = 6;
            public static int Moeda_interna { get; set; } = 7;
            public static int Centro_de_lucro { get; set; } = 8;
            public static int Segmento { get; set; } = 9;
            public static int Texto { get; set; } = 10;
            public static int BeneficiamentoCO { get; set; } = 11;
            public static int GGF_CO { get; set; } = 12;
            public static int MaterialCO { get; set; } = 13;
            public static int MOD_CO { get; set; } = 14;
            public static int SubContratacaoCO { get; set; } = 15;
            public static int Periodo_contábil { get; set; } = 16;
            public static int Elemento_PEP { get; set; } = 17;
            public static int Data_de_lancamento { get; set; } = 18;
        }
        public class CJI3
        {
            public static int Data_de_lancamento { get; set; } = 0;
            public static int Tipo_de_objeto { get; set; } = 1;
            public static int Elemento_PEP { get; set; } = 2;
            public static int Objeto { get; set; } = 3;
            public static int Classe_de_custo { get; set; } = 4;
            public static int Valor_moeda_ACC { get; set; } = 5;
            public static int Moeda_da_ACC { get; set; } = 6;
            public static int Valor_moed_transacao { get; set; } = 7;
            public static int Moeda_da_transacao { get; set; } = 8;
            public static int Divisao { get; set; } = 9;
            public static int Denominacao_de_objeto { get; set; } = 10;
            public static int Denom_classe_custo { get; set; } = 11;
            public static int Documento_de_compras { get; set; } = 12;
        }
        public class ZPPCOOISN_Layout
        {
            public static int ELEMENTO_PEP { get; set; } = 0;
            public static int ORDEM { get; set; } = 1;
            public static int MATERIAL { get; set; } = 2;
            public static int OPERACAO { get; set; } = 3;
            public static int TXTBREVE_OPERACAO { get; set; } = 4;
            public static int SEQUENCIA { get; set; } = 5;
            public static int MARCA { get; set; } = 6;
            public static int COD_DA_POSICAO_DO_PROJETO { get; set; } = 7;
            public static int CODIGO_AGRUPADOR { get; set; } = 8;
            public static int DENOMINDSTAND { get; set; } = 9;
            public static int DATA_CRIACAO_ORDEM { get; set; } = 10;
            public static int DATA_APONTAMENTO { get; set; } = 11;
            public static int QTD_APONT { get; set; } = 12;
            public static int STATUS_DA_OPERACAO { get; set; } = 13;
            public static int ESPESSURA { get; set; } = 14;
            public static int CORTE { get; set; } = 15;
            public static int COMPRIMENTO { get; set; } = 16;
            public static int FUROS { get; set; } = 17;
            public static int PINTURA_ESQUEMA { get; set; } = 18;
            public static int PINTURA_TIPO { get; set; } = 19;
            public static int PINTURA_SUPERFICIE { get; set; } = 20;
            public static int SECAO_VARIAVEL { get; set; } = 21;
            public static int F_EXTERNA { get; set; } = 22;
            public static int F_INTERNA { get; set; } = 23;
            public static int MATERIA_PRIMA { get; set; } = 24;
            public static int OBSERVAÇÃO { get; set; } = 25;
            public static int DOBRAS { get; set; } = 26;
            public static int TIPO_DE_ACO { get; set; } = 27;
            public static int DESENHO_1 { get; set; } = 28;
            public static int DESENHO_2 { get; set; } = 29;

        }

        public class ZPPCOOISN
        {
            public static int ORDEM {get;set;} =  0;
            public static int OPERACAO {get;set;} =  1;
            public static int SEQUENCIA {get;set;} =  2;
            public static int CENTRO_DE_TRABALHO {get;set;} =  3;
            public static int TXTBREVE_OPERACAO {get;set;} =  4;
            public static int CHAVE_DE_CONTROLE {get;set;} =  5;
            public static int ELEMENTO_PEP {get;set;} =  6;
            public static int NÚMERO_PACKING_LIST {get;set;} =  7;
            public static int MATERIAL {get;set;} =  8;
            public static int DENOMINDSTAND {get;set;} =  9;
            public static int TEXTO_MATERIAL {get;set;} =  10;
            public static int TAMANHO_DIMENSAO {get;set;} =  11;
            public static int NOME_DA_OBRA {get;set;} =  12;
            public static int CENTRO {get;set;} =  13;
            public static int DATA_CRIACAO_ORDEM {get;set;} =  14;
            public static int DT_BASE_INI_ORDEM {get;set;} =  15;
            public static int DT_BASE_FIM_ORDEM {get;set;} =  16;
            public static int DT_INI_PROG_ORDEM {get;set;} =  17;
            public static int DT_FIM_PROG_ORDEM {get;set;} =  18;
            public static int DT_BASE_INI_PEP {get;set;} =  19;
            public static int DT_BASE_FIM_PEP {get;set;} =  20;
            public static int STATUS_DO_SISTEMA {get;set;} =  21;
            public static int TIPO_DE_CAPACIDADE {get;set;} =  22;
            public static int QUANTIDADE_OPERACAO {get;set;} =  23;
            public static int UNIDMEDIDA_OPERACAO {get;set;} =  24;
            public static int DATA_DO_INICIO_REAL {get;set;} =  25;
            public static int DATA_DE_INICIO {get;set;} =  26;
            public static int DATA_DO_FIM_REAL {get;set;} =  27;
            public static int ENTRADO_POR {get;set;} =  28;
            public static int N_PESSOAL {get;set;} =  29;
            public static int DATA_APONTAMENTO {get;set;} =  30;
            public static int QTD_NECESSARIA {get;set;} =  31;
            public static int QTD_APONTADA {get;set;} =  32;
            public static int UM_QTD_APONTADA {get;set;} =  33;
            public static int PESO_APONTADO {get;set;} =  34;
            public static int QTD_APONT {get;set;} =  35;
            public static int TEMPO_UNIT_APONT {get;set;} =  36;
            public static int TEMPO_TOTAL_APONT {get;set;} =  37;
            public static int UN_APONT {get;set;} =  38;
            public static int DURACAO_PROCESSAMENTO {get;set;} =  39;
            public static int UNIDPROCESSAMENTO {get;set;} =  40;
            public static int STATUS_DA_OPERACAO {get;set;} =  41;
            public static int ESPESSURA_DO_MATERIAL {get;set;} =  42;
            public static int UM_ESPESSURA_DO_MATERIAL {get;set;} =  43;
            public static int CORTE_LARGURA {get;set;} =  44;
            public static int UM_CORTE_LARGURA {get;set;} =  45;
            public static int COMPRIMENTO {get;set;} =  46;
            public static int UM_COMPRIMENTO {get;set;} =  47;
            public static int FILETE_DA_SOLDA_EM_ACESSORIOS {get;set;} =  48;
            public static int UM_FILETE_DA_SOLDA_EM_ACESSORIOS {get;set;} =  49;
            public static int COMP_DA_SOLDA_DE_ACESSORIOS {get;set;} =  50;
            public static int UM_COMP_DA_SOLDA_DE_ACESSORIOS {get;set;} =  51;
            public static int CHANFRO {get;set;} =  52;
            public static int DIAMETRO_DE_FURO_1 {get;set;} =  53;
            public static int DIAMETRO_DE_FURO_2 {get;set;} =  54;
            public static int DIAMETRO_DE_FURO_3 {get;set;} =  55;
            public static int DIAMETRO_DE_FURO_4 {get;set;} =  56;
            public static int DIAMETRO_DE_FURO_5 {get;set;} =  57;
            public static int QUANTIDADE_DE_FURO_DA_PECA {get;set;} =  58;
            public static int PERIMETRO_DE_CORTE {get;set;} =  59;
            public static int UM_PERIMETRO_DE_CORTE {get;set;} =  60;
            public static int QUANTIDADE_DE_RECORTES {get;set;} =  61;
            public static int SUP_AREA_M2_PARA_PINTURA {get;set;} =  62;
            public static int UM_SUP_AREA_M2_PARA_PINTURA {get;set;} =  63;
            public static int TIPO_DE_PINTURA {get;set;} =  64;
            public static int LISTA_TECNICA_ESQ_DE_PINTURA {get;set;} =  65;
            public static int CONTRA_FLECHA_SECAO_VARIAVEL{get;set;} =  66;
            public static int IND_COR_FACE_EXTERNA_CH_FINA {get;set;} =  67;
            public static int IND_COR_FACE_INTERNA_CH_FINA {get;set;} =  68;
            public static int COMPRIMENTO_DA_ROSCA {get;set;} =  69;
            public static int UM_COMPRIMENTO_DA_ROSCA {get;set;} =  70;
            public static int COD_MATÉRIA_PRIMA_SAP {get;set;} =  71;
            public static int COD_DA_MARCA_DO_PROJETO {get;set;} =  72;
            public static int QTD_DA_MARCA_NO_PROJETO {get;set;} =  73;
            public static int QTD_DA_POSICAO_NO_PROJETO {get;set;} =  74;
            public static int PESO_DA_MARCA_NO_PROJETO {get;set;} =  75;
            public static int UM_PESO_DA_MARCA_NO_PROJETO {get;set;} =  76;
            public static int OBSERVACAO {get;set;} =  77;
            public static int CODIGO_DO_PROJETO_ENGENHARIA {get;set;} =  78;
            public static int RANGE_MATERIAL_ROTEIRO {get;set;} =  79;
            public static int RANGE_MATERIAL_ROTEIRO_ACAB {get;set;} =  80;
            public static int GRUPO_DE_MERCADORIA {get;set;} =  81;
            public static int GRUPO_DE_MERCADORIA_EXTERNO {get;set;} =  82;
            public static int CODIGO_AGRUPADOR {get;set;} =  83;
            public static int FILETE_DA_SOLDA_DE_COMPOSICAO {get;set;} =  84;
            public static int UM_FILETE_DA_SOLDA_DE_COMPOSICAO {get;set;} =  85;
            public static int COMP_DA_SOLDA_DE_COMPOSICAO {get;set;} =  86;
            public static int UM_COMP_DA_SOLDA_DE_COMPOSICAO {get;set;} =  87;
            public static int QT_DE_DOBRA_DA_PECA {get;set;} =  88;
            public static int DESCRICAO_TIPO_DO_ACO {get;set;} =  89;
            public static int VOLUME_DE_SOLDA_COMPOSICAO {get;set;} =  90;
            public static int UM_VOLUME_DE_SOLDA_COMPOSICAO {get;set;} =  91;
            public static int COD_DA_POSICAO_DO_PROJETO {get;set;} =  92;
            public static int PESO_DA_POSICAO_NO_PROJETO {get;set;} =  93;
            public static int UM_PESO_DA_POSICAO_NO_PROJETO {get;set;} =  94;
            public static int VOLUME_DE_SOLDA_DO_ACESSORIO {get;set;} =  95;
            public static int UM_VOLUME_DE_SOLDA_DO_ACESSORIO {get;set;} =  96;
            public static int DESENHO_1 {get;set;} =  97;
            public static int DESENHO_2 {get;set;} =  98;

        }
        public class ZPP0066N
        {
            public static int PEP {get;set;} =  0;
            public static int MATERIAL {get;set;} =  1;
            public static int CARGA {get;set;} =  2;
            public static int PACKLIST {get;set;} =  3;
            public static int STATUSMAT {get;set;} =  4;
            public static int DESENHO {get;set;} =  5;
            public static int NUMCARGA {get;set;} =  6;
            public static int QUANTIDADE {get;set;} =  7;
        }
        public class ZPP0066N_Sem_Perfil
        {
            public static int Packing_List {get;set;} =  0;
            public static int Centro {get;set;} =  1;
            public static int NumCarga {get;set;} =  2;
            public static int Elemento_PEP {get;set;} =  3;
            public static int Num_Romaneio {get;set;} =  4;
            public static int DocVendas {get;set;} =  5;
            public static int NFiscal {get;set;} =  6;
            public static int ConfirmCarga {get;set;} =  7;
            public static int SKID {get;set;} =  8;
            public static int DtIniPEP {get;set;} =  9;
            public static int DtFimPEP {get;set;} =  10;
            public static int Item_Pack {get;set;} =  11;
            public static int SeqItmPack {get;set;} =  12;
            public static int SeqCarr {get;set;} =  13;
            public static int Camada {get;set;} =  14;
            public static int QtdePacking {get;set;} =  15;
            public static int OPla {get;set;} =  16;
            public static int Centro_producao {get;set;} =  17;
            public static int Material {get;set;} =  18;
            public static int Mat_Descricao {get;set;} =  19;
            public static int NumDesEngenharia {get;set;} =  20;
            public static int QtdeNec {get;set;} =  21;
            public static int Peso_Mat {get;set;} =  22;
            public static int Nome_Obra {get;set;} =  23;
            public static int Cidade {get;set;} =  24;
            public static int Estado {get;set;} =  25;
            public static int Etiqueta_Mat {get;set;} =  26;
            public static int Etiqueta_Vol {get;set;} =  27;
            public static int Peso_Itm_Pack {get;set;} =  28;
            public static int Compri {get;set;} =  29;
            public static int Altura {get;set;} =  30;
            public static int Largura {get;set;} =  31;
            public static int Status_Mat {get;set;} =  32;
            public static int Conferência {get;set;} =  33;
            public static int N_Pessoal {get;set;} =  34;
            public static int Perc_Packing {get;set;} =  35;
            public static int Enderecamento {get;set;} =  36;
        }
        public enum CN47N
        {
            WBS  =  0,
            STATUS =  1,
            TEXTO_OPERACAO =  2,
            INICIO_BASE =  3,
            ULTIMA_DATA_FIM_BASE  =  4,
            PRIMEIRA_DATA_FIM_PREVISAO =  5,
            ULTIMA_DATA_FIM_PREVISAO =  6,
            DATA_FIM_REAL  =  7,
            INICIO_PREVISTO  =  8,
            FIM_PREVISTO  =  9
        }

        //public class TAB_ZPP0100
        //{
        //    public static int booleano {get;set;} =  0;
        //    public static int Ordem_Embarque {get;set;} =  1;
        //    public static int Qtd_Embarque {get;set;} =  2;
        //    public static int Nro_Carga {get;set;} =  3;
        //    public static int St_Embarque {get;set;} =  4;
        //    public static int St_Carga {get;set;} =  5;
        //    public static int Etq_Material {get;set;} =  6;
        //    public static int Material {get;set;} =  7;
        //    public static int Descricao {get;set;} =  8;
        //    public static int Tamanho_dimensao {get;set;} =  9;
        //    public static int Comprimento {get;set;} =  10;
        //    public static int Peso_item_Tot {get;set;} =  11;
        //    public static int Nome_da_Obra {get;set;} =  12;
        //    public static int Elemento_PEP {get;set;} =  13;
        //    public static int Centro {get;set;} =  14;
        //    public static int Etq_Impressa {get;set;} =  15;
        //    public static int Etq_Volume {get;set;} =  16;
        //    public static int Status {get;set;} =  17;
        //    public static int Qtd_Carregada {get;set;} =  18;
        //    public static int Sld_1202 {get;set;} =  19;
        //    public static int Sld_1203 {get;set;} =  20;
        //    public static int Sld_1204 {get;set;} =  21;
        //    public static int St_Conf_ {get;set;} =  22;
        //    public static int St_DtProg_ {get;set;} =  23;
        //    public static int Data {get;set;} =  24;
        //    public static int Ordem_Prod_ {get;set;} =  25;
        //    public static int Apontamento_Fert {get;set;} =  26;

        //}



        //public class TAB_ZCONTRATOS
        //{
        //    public static int Empresa {get;set;} =  0;
        //    public static int Cen {get;set;} =  1;
        //    public static int Contrato {get;set;} =  2;
        //    public static int Cliente {get;set;} =  3;
        //    public static int CNPJ {get;set;} =  4;
        //    public static int Razao_Social_Cliente {get;set;} =  5;
        //    public static int SetInd {get;set;} =  6;
        //    public static int UF {get;set;} =  7;
        //    public static int Situacao {get;set;} =  8;
        //    public static int Devolucoes {get;set;} =  9;
        //    public static int Nome_da_obra {get;set;} =  10;
        //    public static int Elemento_PEP {get;set;} =  11;
        //    public static int Contas_a_receber {get;set;} =  12;
        //    public static int Receita {get;set;} =  13;
        //    public static int Cotacao {get;set;} =  14;
        //    public static int Itm {get;set;} =  15;
        //    public static int Ordem_venda {get;set;} =  16;
        //    public static int TpDV {get;set;} =  17;
        //    public static int Fatura {get;set;} =  18;
        //    public static int TipFt {get;set;} =  19;
        //    public static int Descr_tipo_fat {get;set;} =  20;
        //    public static int NF {get;set;} =  21;
        //    public static int Data_emissao {get;set;} =  22;
        //    public static int Material {get;set;} =  23;
        //    public static int CFOP {get;set;} =  24;
        //    public static int Quantidade {get;set;} =  25;
        //    public static int UM {get;set;} =  26;
        //    public static int Valor_unit {get;set;} =  27;
        //    public static int Peso_liq {get;set;} =  28;
        //    public static int Valor_total_NF {get;set;} =  29;

        //}

        //public class TAB_ZPP0112
        //{
        //    public static int sel { get; set; } = 0;
        //    public static int Nro_Carga { get; set; } = 1;
        //    public static int Elemento_PEP { get; set; } = 2;
        //    public static int Centro { get; set; } = 3;
        //    public static int Fornecedor { get; set; } = 4;
        //    public static int Tipo_Veiculo { get; set; } = 5;
        //    public static int Num_Placa { get; set; } = 6;
        //    public static int Motorista { get; set; } = 7;
        //    public static int RG { get; set; } = 8;
        //    public static int Telefone { get; set; } = 9;
        //    public static int VAL2 { get; set; } = 10;
        //    public static int Telefone_2 { get; set; } = 11;
        //    public static int Fornecedor_2 { get; set; } = 12;
        //    public static int Telefone_3 { get; set; } = 13;
        //    public static int Observacoes { get; set; } = 14;
        //}


        public class ZPMP
        {
            public static int Elemento_PEP {get;set;} =  0;
            public static int Denominacao {get;set;} =  1;
            public static int Centro {get;set;} =  2;
            public static int Centro_producao {get;set;} =  3;
            public static int Material {get;set;} =  4;
            public static int Texto_breve_material {get;set;} =  5;
            public static int Denom_grupo_merc {get;set;} =  6;
            public static int Peso_necessario {get;set;} =  7;
            public static int Peso_produzido {get;set;} =  8;
            public static int Qtd_necessaria {get;set;} =  9;
            public static int Qtd_mercad_entrada {get;set;} =  10;
            public static int Fim_Engenharia_Base {get;set;} =  11;
            public static int Fim_Engenharia_Real {get;set;} =  12;
            public static int Fim_Fabrica_Base {get;set;} =  13;
            public static int Fim_Fabrica_Real {get;set;} =  14;
            public static int Fim_Logistica_Base {get;set;} =  15;
            public static int Fim_Logistica_Real {get;set;} =  16;
            public static int Fim_Montagem_Base {get;set;} =  17;
            public static int Fim_Montagem_Real {get;set;} =  18;
            public static int Inicio_Montagem_Base {get;set;} =  19;
            public static int Saldo_peso_produzido {get;set;} =  20;
            public static int Status_Sistema_PEP {get;set;} =  21;
            public static int Status_Usuario_PEP {get;set;} =  22;
            public static int Status_Sistema_Tarefa {get;set;} =  23;
        }
    }
}
