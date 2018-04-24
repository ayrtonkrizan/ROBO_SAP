namespace ROBO
{
    public enum TipoObjeto
    {
        tNOOBJECT,
        tMASTERDATA,
        tMASTERLINE,
        tDOCUMENT,
        tDOCUMENTLINES
    }

    public class custUserTable
    {
        private string sIdTable;
        private string sDescription;
        private TipoObjeto sType;

        public string NomeTabela
        {
            get
            {
                return sIdTable;
            }
        }
        public string Descricao
        {
            get
            {
                return sDescription;
            }
        }
        public TipoObjeto Tipo
        {
            get
            {
                return sType;
            }
        }

        public custUserTable(string sNomeTabela, string sDescricao, TipoObjeto tTipo)
        {
            if (sNomeTabela.Length > 19)
            {
                sIdTable = sNomeTabela.Substring(0, 19);
            }
            else
            {
                sIdTable = sNomeTabela;
            }
            if (sDescricao.Length > 30)
            {
                sDescription = sDescricao.Substring(0, 30);
            }
            else
            {
                sDescription = sDescricao;
            }
            sType = tTipo;
        }
    }
}
