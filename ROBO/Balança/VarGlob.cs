using System.Configuration;

namespace ROBO
{
    public class CodErro
    {
        public static int SUCESSO = 0;
        public static int EXE_INEXISTENTE = 1;
        public static int ARQ_INEXISTENTE = 2;
        public static int LEITURA_ARQUIVO = 3;
    }

    public class MsgErro
    {
        public static string SUCESSO = "Transação OK.";
        public static string EXE_INEXISTENTE = "O executável não existe no diretório de destino.";
        public static string ARQ_INEXISTENTE = "O arquivo de entrada não existe no diretório de destino.";
        public static string LEITURA_ARQUIVO = "Falha na leitura do arquivo de entrada.";
    }

    public class Config
    {
        public static string TIMEOUT = ConfigurationManager.AppSettings["TimeOut"];
        public static string PATH_APP_PESAG = ConfigurationManager.AppSettings["PathAppPesag"];
        public static string NOME_APP_PESAG = ConfigurationManager.AppSettings["NomeAppPesag"];
        public static string NOME_ARQ_ENTRD = ConfigurationManager.AppSettings["NomeArqEntrada"];
    }
    
}