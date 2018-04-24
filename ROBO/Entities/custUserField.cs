using System;

namespace ROBO
{
    public enum TipoCampo
    {
        tALPHA,
        tMEMO,
        tDATE,
        tTIME,
        tQUANT,
        tVALOR,
        tPERC,
        tNUMERO,
        tPRECO,
        tLINK,
        tADDRESS,
        tPHONE,
        tTAXA,
        tMEDIDA,
        tIMAGE,
    }

    public enum TipoTabela
    {
        tUSER,
        tSBO
    }

    public class custUserField
    {
        public string NomeTabela { get; set; }
        public string NomeCampo { get; set; }
        public string Descricao { get; set; }
        public TipoCampo Tipo { get; private set; }
        public int Tamanho { get; private set; }
        public TipoTabela TipoTabela { get; private set; }
        public string ValoresValidos { get; private set; }
        public bool Obrigatorio { get; private set; }
        public string TabelaLigada { get; private set; }
        public string ValorPadrao { get; private set; }
        public int FieldId { get; set; }




        //TODO: Somente para compatibilidade
        [Obsolete("addedUserField is deprecated")]
        public custUserField(string sNomeTabela, string sNomeCampo, string sDescricao, TipoCampo tTipo, int iTamanho, TipoTabela tTipoTabela, string sValoresValidos, string sObrigatorio = "", string sTabelaLigada = "") :
            this(sNomeTabela, sNomeCampo, sDescricao, tTipo, iTamanho, tTipoTabela, string.IsNullOrEmpty(sObrigatorio) ? false : true, sValoresValidos, sObrigatorio, sTabelaLigada)
        { }

        public custUserField(string sNomeTabela, string sNomeCampo, string sDescricao, TipoCampo tTipo, int iTamanho, TipoTabela tTipoTabela, bool sObrigatorio = false, string sValoresValidos = "", string valorPadrao = "", string sTabelaLigada = "")
        {
            NomeTabela = sNomeTabela;
            NomeCampo = sNomeCampo;
            Descricao = sDescricao;
            Tipo = tTipo;
            Tamanho = iTamanho;
            TipoTabela = tTipoTabela;
            Obrigatorio = sObrigatorio;
            ValoresValidos = sValoresValidos;
            TabelaLigada = sTabelaLigada;
            ValorPadrao = valorPadrao;
        }
    }
}
