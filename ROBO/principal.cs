using System;
using System.Collections.Generic;
//using System.Text;
using SAPbobsCOM;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace ROBO
{
    public class principal
    {
        public static int contLC;
        public static Company oCompany;
        public static Company conexao(string Server, string Company, string DbUser, string DbPass, string UserSap, string PassSAP, string DbServerType, string License)
        {
            oCompany = new Company();
            oCompany.Server = Server;
            oCompany.CompanyDB = Company;
            //oCompany.DbUserName = DbUser;
            //oCompany.DbPassword = DbPass;
            oCompany.UserName = UserSap;
            oCompany.Password = PassSAP;

            switch (DbServerType)
            {
                case "dst_MSSQL2008":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2008;
                    break;

                case "dst_MSSQL2012":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2012;
                    break;

                case "dst_MSSQL2014":
                    oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2014;
                    break;

                case "dst_HANADB":
                    oCompany.DbServerType = BoDataServerTypes.dst_HANADB;
                    break;
            }
            oCompany.LicenseServer = License;

            // tenta conectar no sap
            if (oCompany.Connect() != 0)
            {
                return oCompany;
            }
            else
            {
                return oCompany;
            }
        }

        #region Cria Tabelas e Campos

        //public static Int32 CriaTabela(string Nome, String Descricao, BoUTBTableType tp)
        //{
        //    Int32 i = 0;
        //    UserTablesMD oUserTablesMD;
        //    oUserTablesMD = (UserTablesMD)oCompany.GetBusinessObject(BoObjectTypes.oUserTables);
        //    oUserTablesMD.TableName = Nome;
        //    oUserTablesMD.TableDescription = Descricao;
        //    oUserTablesMD.TableType = tp;

        //    i = oUserTablesMD.Add();

        //    return i;
        //}

        public static Int32 CriaCampo(String Nome, string descricao, int tamanho, BoFieldTypes fldtype, BoFldSubTypes fldsubtype, string tabela, string tabelalink = "", Boolean validvalue = false, string ValidValue1 = "", string ValidDscr1 = "", string ValidValue2 = "", string ValidDscr2 = "", string ValidValue3 = "", string ValidDscr3 = "", string ValidValue4 = "", string ValidDscr4 = "", string ValidValue5 = "", string ValidDscr5 = "")
        {

            Int32 i = 0;
            try
            {
                UserFieldsMD oUserFieldsMD;
                oUserFieldsMD = (UserFieldsMD)oCompany.GetBusinessObject(BoObjectTypes.oUserFields);
                oUserFieldsMD.Name = Nome;
                oUserFieldsMD.Type = fldtype;
                oUserFieldsMD.Description = descricao;
                oUserFieldsMD.Size = tamanho;
                oUserFieldsMD.SubType = fldsubtype;
                oUserFieldsMD.TableName = tabela;
                oUserFieldsMD.LinkedTable = tabelalink;

                if (validvalue == true)
                {
                    oUserFieldsMD.ValidValues.Value = ValidValue1;
                    oUserFieldsMD.ValidValues.Description = ValidDscr1;
                    oUserFieldsMD.ValidValues.Add();

                    oUserFieldsMD.ValidValues.Value = ValidValue2;
                    oUserFieldsMD.ValidValues.Description = ValidDscr2;
                    oUserFieldsMD.ValidValues.Add();

                    oUserFieldsMD.ValidValues.Value = ValidValue3;
                    oUserFieldsMD.ValidValues.Description = ValidDscr3;
                    oUserFieldsMD.ValidValues.Add();

                    if (ValidValue4 != "")
                    {
                        oUserFieldsMD.ValidValues.Value = ValidValue4;
                        oUserFieldsMD.ValidValues.Description = ValidDscr4;
                        oUserFieldsMD.ValidValues.Add();
                    }

                    if (ValidValue5 != "")
                    {
                        oUserFieldsMD.ValidValues.Value = ValidValue5;
                        oUserFieldsMD.ValidValues.Description = ValidDscr5;
                        oUserFieldsMD.ValidValues.Add();
                    }
                }

                i = oUserFieldsMD.Add();

                //if (i != 0)
                //    throw new Exception(ocompany.GetLastErrorDescription());

                return i;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return i;
            }
        }

        #endregion 


        [DllImport("PClink6.dll")]
        private static extern int W9091Serial(int Canal);

        [DllImport("PClink6.dll")]
        private static extern int Select_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern void Close_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern void Deleta_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern int Update_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern String Gross_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern String Net_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern String Tare_Canal(int Canal);

        [DllImport("PClink6.dll")]
        private static extern String Estado_EmMovimento(int Canal);

        [DllImport("PClink6.dll")]
        private static extern void ProgSerial(int Canal, int Baud, int Dados, int Paridade, int Stop);

        [DllImport("PClink6.dll")]
        private static extern float Gross_Canal_Float(int Canal);

        [DllImport("PClink6.dll")]
        private static extern float Net_Canal_Float(int Canal);

        [DllImport("PClink6.dll")]
        private static extern float Tare_Canal_Float(int Canal);

        public static float PesoTara { get; set; }
        public static float PesoBruto { get; set; }
        public static float PesoLiquido { get; set; }

        private string MontaMsgResposta(int cod_resp, string msg_resp, string peso = null)
        {
            string[] arr_retorno = new string[3];

            arr_retorno[0] = string.Format("status:'{0}'", cod_resp);
            arr_retorno[1] = string.Format("mensagem:'{0}'", msg_resp);

            if (peso != null)
                arr_retorno[2] = string.Format("peso:'{0}'", peso);

            return string.Join(";", arr_retorno);
        }

        public void PrintTicket()
        {
            try
            {
                CrystalDecisions.CrystalReports.Engine.ReportDocument etq = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                string spath = System.Windows.Forms.Application.StartupPath;
                etq.Load(spath + "\\" + @"\Reports\DRE_v3.RPT");

                CrystalDecisions.Shared.IConnectionInfo crConnectionInfo = etq.DataSourceConnections[0];

                string DbUser = Properties.Settings.Default.DbUserName;
                string DbPwd = Properties.Settings.Default.DbPassword;
                string Server = Properties.Settings.Default.Server;
                string CompanyDB = Properties.Settings.Default.CompanyDB;

                crConnectionInfo.SetConnection(Server, CompanyDB, DbUser, DbPwd);

                //Parametro 0
                CrystalDecisions.Shared.ParameterField objParameterField = etq.ParameterFields[0];
                CrystalDecisions.Shared.ParameterDiscreteValue objParameterDiscreteValue;

                objParameterDiscreteValue = new CrystalDecisions.Shared.ParameterDiscreteValue();

                objParameterField.CurrentValues.Clear();

                objParameterDiscreteValue.Description = "2017-10-01";

                objParameterDiscreteValue.Value = "2017-10-01";

                objParameterField.CurrentValues.Add(objParameterDiscreteValue);

                //Parametro 1
                CrystalDecisions.Shared.ParameterField objParameterField1 = etq.ParameterFields[1];
                CrystalDecisions.Shared.ParameterDiscreteValue objParameterDiscreteValue1;

                objParameterDiscreteValue1 = new CrystalDecisions.Shared.ParameterDiscreteValue();

                objParameterField1.CurrentValues.Clear();

                objParameterDiscreteValue1.Description = "2017-10-31";

                objParameterDiscreteValue1.Value = "2017-10-31";

                objParameterField1.CurrentValues.Add(objParameterDiscreteValue1);

                //etq.PrintOptions.PrinterName = Properties.Settings.Default.PrinterNameTerra;

                etq.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, @"C:\Temp\RPT.pdf");
                etq.Close();

            }
            catch (Exception ex)
            {
                
            }
        }

        public static bool ParamGravaAudit = Properties.Settings.Default.Log;

        public static void GravaAudit(string message, string exception = null)
        {
            if (!ParamGravaAudit)
                return;

            string data_aaaammdd = DateTime.Now.ToString("yyyyMMdd");
            string data_hora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string path = AppDomain.CurrentDomain.BaseDirectory + "log\\";
            string filename = string.Format("audit_{0}.log", data_aaaammdd);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            StreamWriter file = File.AppendText(path + filename);

            file.Flush();
            file.WriteLine(data_hora + " - " + message);

            if (exception != null)
                file.WriteLine("Exception: " + exception);

            file.Close();
        }

        public string LerPeso()
        {
            Config.PATH_APP_PESAG = @"C:\Temp\Petruz";
            Config.NOME_APP_PESAG = "LePeso.exe";
            Config.NOME_ARQ_ENTRD = "Pesagem.txt";

            // monta o caminho completo do aplicativo e do arquivo de entrada
            string fullpath_app = Path.Combine(Config.PATH_APP_PESAG, Config.NOME_APP_PESAG);
            string fullpath_arq = Path.Combine(Config.PATH_APP_PESAG, Config.NOME_ARQ_ENTRD);

            // remove o arquivo de entrada anterior caso existir
            if (File.Exists(fullpath_arq))
                File.Delete(fullpath_arq);

            // verifica se o aplicativo existe no diretório configurado
            if (!File.Exists(fullpath_app))
                return MontaMsgResposta(CodErro.EXE_INEXISTENTE, MsgErro.EXE_INEXISTENTE);

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();

                // seta os parâmetros do processo
                info.UseShellExecute = true;
                info.WorkingDirectory = Config.PATH_APP_PESAG;
                info.FileName = Config.NOME_APP_PESAG;
                info.Verb = "runas";

                // executa uma aplicação externa para gerar o arquivo de entrada
                Process process = Process.Start(info);

                int timeout = Convert.ToInt32(Config.TIMEOUT);

                // espera o processo encerrar para continuar a execução do código
                if (!process.WaitForExit(timeout))
                {
                    // encerra o processo caso esgotar o tempo de timeout
                    process.Kill();
                    return MontaMsgResposta(CodErro.LEITURA_ARQUIVO, MsgErro.LEITURA_ARQUIVO);
                }

                // verifica se o aplicativo gerou o arquivo de entrada
                if (!File.Exists(fullpath_arq))
                    return MontaMsgResposta(CodErro.ARQ_INEXISTENTE, MsgErro.ARQ_INEXISTENTE);

                // lê as linhas do arquivo 'Pesagem.txt'
                string[] lines = File.ReadAllLines(fullpath_arq);

                // lê a segunda linha do arquivo e monta um array separando pelos caracteres ' ' e 'tab'
                string[] data = lines[1].Split(' ', '\t');

                // obtém o campo 'quantidade' da linha com formato: '+99999,99\t kg'
                string peso = data[0].Replace(",", ".");

                // retorna uma mensagem de sucesso
                return MontaMsgResposta(CodErro.SUCESSO, MsgErro.SUCESSO, peso);
            }
            catch (Exception)
            {
                // retorna uma mensagem de erro
                return MontaMsgResposta(CodErro.LEITURA_ARQUIVO, MsgErro.LEITURA_ARQUIVO);
            }
        }
    }
}
