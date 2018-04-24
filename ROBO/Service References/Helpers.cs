using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Data;
using SAPbouiCOM;
using SAPbobsCOM;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using B1WizardBase;
using System.Globalization;

namespace ROBO
{
    public enum TipoCelula
    {
        tEDIT,
        tCOMBO,
        tCHECKBOX
    }

    public enum TipoNumerico
    {
        tPONTO,
        tVIRGULA,
        tSOMENTENUMEROS
    }

    public enum TipoDialog
    {
        tSAVE,
        tOPEN,
        tFOLDER
    }

    public enum TipoXslt
    {
        tBUSINESSPARTNERS,
        tITEMS
    }

    public enum IntervaloData
    {
        Dias,
        Meses,
        Anos
    }

    public class Helpers
    {
        static string initialDirectory;
        static string initialFile;
        static string filterFiles;
        static string selectedFile;
        static TipoDialog typeDialog;


        public static string SAPResource
        {
            get
            {

                if (B1Connections.diCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    return "Consultas.Hana";
                else
                    return "Consultas.Sql";
            }

        }

        public static bool CreateTables(custUserTable[] listTables, string sAddInName)
        {
            bool bTemTabelas = false;
            string tmpTabelas = "", tmpTipoObjeto = "";
            Recordset objRecordSet = null;
            string strQuery;

            try
            {
                tmpTabelas = "<?xml version=\"1.0\" encoding=\"UTF-16\"?><BOM>";
                foreach (custUserTable uTabela in listTables)
                {

                    strQuery = string.Format(Helpers.GetResource("ConsultarExisteTabela"), uTabela.NomeTabela, B1Connections.diCompany.CompanyDB);
                    objRecordSet = GetRecordSet(strQuery);

                    if (int.Parse(objRecordSet.Fields.Item(0).Value.ToString()) <= 0)
                    {
                        Helpers.SetText("Carregando... " + sAddInName + " --> Aguarde... Criando a Tabela [" + uTabela.NomeTabela + "] --> [" + uTabela.Descricao + "]...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                        if (uTabela.Tipo == TipoObjeto.tNOOBJECT)
                        {
                            tmpTipoObjeto = "bott_NoObject";
                        }
                        else if (uTabela.Tipo == TipoObjeto.tMASTERDATA)
                        {
                            tmpTipoObjeto = "bott_MasterData";
                        }
                        else if (uTabela.Tipo == TipoObjeto.tMASTERLINE)
                        {
                            tmpTipoObjeto = "bott_MasterDataLines";
                        }
                        else if (uTabela.Tipo == TipoObjeto.tDOCUMENT)
                        {
                            tmpTipoObjeto = "bott_Document";
                        }
                        else if (uTabela.Tipo == TipoObjeto.tDOCUMENTLINES)
                        {
                            tmpTipoObjeto = "bott_DocumentLines";
                        }

                        tmpTabelas += "<BO><AdmInfo><Object>153</Object><Version>2</Version></AdmInfo><UserTablesMD><row>" +
                                      "<TableName>" + uTabela.NomeTabela + "</TableName>" +
                                      "<TableDescription>" + uTabela.Descricao + "</TableDescription>" +
                                      "<TableType>" + tmpTipoObjeto + "</TableType>" +
                                      "</row></UserTablesMD></BO>";
                        bTemTabelas = true;
                    }

                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(objRecordSet);
                    LimparRecordSetDaMemoria(objRecordSet);
                }

                if (bTemTabelas == true)
                {
                    tmpTabelas += "</BOM>";

                    CreateTablesFromXML(tmpTabelas, sAddInName);
                }
            }
            catch (Exception ex)
            {
                Helpers.SetText(string.Format("Helpers-UDT - {0}", ex));
            }
            finally
            {
                if (objRecordSet != null)
                    LimparRecordSetDaMemoria(objRecordSet);
            }

            return bTemTabelas;
        }

        public static bool CreateFields(custUserField[] listFields, string sAddInName)
        {
            bool updateLine = false;
            bool atualizarCampos = false;

            bool criarCampos = false;
            string tmpCamposAdd = ""
                , tmpCamposUpdate = ""
                , tmpNomeTabela = ""
                , tmpTamanho = ""
                , tmpObrigatorio = ""
                , tmpValorPadrao = "";

            Recordset objRecordSet = null;
            string query;
            var listaValoresValidos = new List<custValoresValidos>();
            var listaUserFields = new List<custUserField>();

            try
            {
                tmpCamposAdd = "<?xml version=\"1.0\" encoding=\"UTF-16\"?><BOM>";
                tmpCamposUpdate = "<?xml version=\"1.0\" encoding=\"UTF-16\"?><BOM>";

                foreach (custUserField uCampo in listFields)
                {
                    //---------------------------------------------------------------------------//

                    if (B1Connections.diCompany.Version >= 920000)
                    {
                        if (uCampo.NomeTabela.Length > 20)
                            uCampo.NomeTabela = uCampo.NomeTabela.Substring(0, 20);
                        else
                            uCampo.NomeTabela = uCampo.NomeTabela;

                        if (uCampo.NomeCampo.Length > 50)
                            uCampo.NomeCampo = uCampo.NomeCampo.Substring(0, 50);
                        else
                            uCampo.NomeCampo = uCampo.NomeCampo;

                        if (uCampo.Descricao.Length > 80)
                            uCampo.Descricao = uCampo.Descricao.Substring(0, 80);
                        else
                            uCampo.Descricao = uCampo.Descricao;
                    }
                    else
                    {
                        if (uCampo.NomeTabela.Length > 20)
                            uCampo.NomeTabela = uCampo.NomeTabela.Substring(0, 20);
                        else
                            uCampo.NomeTabela = uCampo.NomeTabela;

                        if (uCampo.NomeCampo.Length > 18)
                            uCampo.NomeCampo = uCampo.NomeCampo.Substring(0, 18);
                        else
                            uCampo.NomeCampo = uCampo.NomeCampo;

                        if (uCampo.Descricao.Length > 30)
                            uCampo.Descricao = uCampo.Descricao.Substring(0, 30);
                        else
                            uCampo.Descricao = uCampo.Descricao;
                    }


                    if (uCampo.TipoTabela == TipoTabela.tUSER)
                        tmpNomeTabela = "@" + uCampo.NomeTabela;
                    else if (uCampo.TipoTabela == TipoTabela.tSBO)
                        tmpNomeTabela = uCampo.NomeTabela;


                    query = Helpers.GetResource("ConsultarDadosCampo");

                    LimparRecordSetDaMemoria(objRecordSet);
                    objRecordSet = null;
                    GC.Collect();

                    objRecordSet = GetRecordSet(string.Format(query, tmpNomeTabela, uCampo.NomeCampo, B1Connections.diCompany.CompanyDB));

                    if (objRecordSet.RecordCount <= 0)
                    {
                        tmpCamposAdd += GetTypes(uCampo);
                        criarCampos = true;
                    }


                    LimparRecordSetDaMemoria(objRecordSet);
                }

                LimparRecordSetDaMemoria(objRecordSet);

                if (criarCampos)
                {
                    tmpCamposAdd += "</BOM>";
                    CreateFieldsFromXML(tmpCamposAdd, sAddInName);
                }

                if (atualizarCampos)
                {
                    tmpCamposUpdate += "</BOM>";
                    CreateFieldsFromXML(tmpCamposUpdate, sAddInName);
                }

                //Atualiza valores validos
                if (listaValoresValidos.Count > 0)
                    ValidarValoresValidos(listaValoresValidos, sAddInName);

                //Atualiza campos alterados
                if (listaUserFields.Count > 0)
                    UpdateFields(listaUserFields, sAddInName);



            }
            catch (Exception ex)
            {
                Helpers.SetText(string.Format("Helpers {0}", ex.Message));
            }
            finally
            {
                if (objRecordSet != null)
                {
                    Marshal.ReleaseComObject(objRecordSet);
                    objRecordSet = null;
                    GC.Collect();
                }
            }

            return criarCampos;
        }

        private static void CreateTablesFromXML(string strTables, string sAddInName)
        {
            var objUserTable = (UserTablesMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserTables);
            try
            {
                B1Connections.diCompany.XmlExportType = BoXmlExportTypes.xet_ExportImportMode;
                B1Connections.diCompany.XMLAsString = true;


                for (var i = 0; i < B1Connections.diCompany.GetXMLelementCount(strTables); i++)
                {
                    objUserTable = (UserTablesMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserTables);
                    objUserTable.Browser.ReadXml(strTables, i);

                    var intResult = objUserTable.Add();
                    if ((intResult != 0) && (intResult != -2035))
                    {
                        var sErro = B1Connections.diCompany.GetLastErrorDescription();

                        Helpers.SetText(string.Format("Helpers - Erro ao criar tabela[{0}] - {1}", objUserTable.TableName, sErro));
                    }

                    Marshal.ReleaseComObject(objUserTable);
                }

            }
            catch (Exception e)
            {
                Helpers.SetText(string.Format("Helpers - {0}", e.Message));
            }
            finally
            {
                if (objUserTable != null)
                    Marshal.ReleaseComObject(objUserTable);

                GC.Collect();
            }
        }

        private static void CreateFieldsFromXML(string strFields, string sAddInName)
        {
            var objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);

            try
            {
                B1Connections.diCompany.XmlExportType = BoXmlExportTypes.xet_ExportImportMode;
                B1Connections.diCompany.XMLAsString = true;

                for (var i = 0; i < B1Connections.diCompany.GetXMLelementCount(strFields); i++)
                {
                    objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);
                    objUserFields.Browser.ReadXml(strFields, i);

                    Helpers.SetText("Carregando... " + sAddInName + " --> Aguarde... Criando o Campo [" + objUserFields.Name + "] na Tabela [" + objUserFields.TableName + "]...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);


                    var intResult = objUserFields.Add();
                    if ((intResult != 0) && (intResult != -2035))
                        Helpers.SetText(string.Format("Erro ao criar campo de usuario[{0}.{1}]:{2}", objUserFields.TableName, objUserFields.Name, B1Connections.diCompany.GetLastErrorDescription()));


                    Marshal.ReleaseComObject(objUserFields);
                }
            }
            catch (Exception er)
            {
                Helpers.SetText(string.Format("HELPERS - {0}", er));
            }
            finally
            {
                if (objUserFields != null)
                    Marshal.ReleaseComObject(objUserFields);

                GC.Collect();
            }
        }

        private struct custValoresValidos
        {
            internal string Tabela;
            internal string Campo;
            internal string Valor;
            internal string Descricao;
            internal string ValoresValidos;
        };

        private static void UpdateFields(List<custUserField> listaUserFields, string sAddInName)
        {
            var objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);

            try
            {
                foreach (var item in listaUserFields)
                {
                    objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);
                    if (objUserFields.GetByKey(item.NomeTabela, item.FieldId))
                    {
                        Helpers.SetText("Carregando... " + sAddInName + " --> Aguarde... Atualizando o Campo [" + item.NomeCampo + "] na Tabela [" + item.NomeTabela + "]...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                        objUserFields.Description = item.Descricao;
                        objUserFields.DefaultValue = item.ValorPadrao;
                        objUserFields.Mandatory = item.Obrigatorio ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                        if (item.Tamanho > 0)
                        {
                            objUserFields.Size = item.Tamanho;
                            objUserFields.EditSize = item.Tamanho;
                        }

                        var intResult = objUserFields.Update();
                        if ((intResult != 0) && (intResult != -2035))
                        {
                            Helpers.SetText("Erro ao Atualizar Campo de Usuário: " + objUserFields.TableName + ": " + objUserFields.Name + " ::: " + B1Connections.diCompany.GetLastErrorDescription());
                        }
                    }

                    Marshal.ReleaseComObject(objUserFields);
                }

            }
            catch (Exception er)
            {
                Helpers.SetText(string.Format("HELPERS - {0}", er));
            }
            finally
            {
                if (objUserFields != null)
                    Marshal.ReleaseComObject(objUserFields);

                GC.Collect();
            }
        }
        
        public static string getValueFromUserDataSource(SAPbouiCOM.Form objForm, string sItemUID)
        {
            string sValor = "";

            try
            {
                sValor = objForm.DataSources.UserDataSources.Item(sItemUID).ValueEx;
            }
            catch (Exception e)
            {
                //ExceptionAddOn.GetInstance().throwException("HELPERS", "3.0", e);
                B1Connections.theAppl.SetStatusBarMessage(String.Format("Helpers.getValueFormUserDataSource - {0}", e.Message));
            }

            return sValor;
        }

        public static string getValueFromEditText(SAPbouiCOM.Form objForm, string sItemUID)
        {
            string sValor = "";

            try
            {
                sValor = ((EditText)(objForm.Items.Item(sItemUID).Specific)).String;
            }
            catch (Exception e)
            {
                //ExceptionAddOn.GetInstance().throwException("HELPERS", "3.0", e);
                B1Connections.theAppl.SetStatusBarMessage(String.Format("Helpers.getValueFormUserDataSource - {0}", e.Message));
            }

            return sValor;
        }

        public static string getValueFromComboBox(SAPbouiCOM.Form objForm, string sItemUID)
        {
            string sValor = "";

            try
            {
                SAPbouiCOM.ComboBox x;
                var y = (objForm.Items.Item(sItemUID).Specific);

                var j = ((SAPbouiCOM.ComboBox)y).Selected;
                if (((SAPbouiCOM.ComboBox)(objForm.Items.Item(sItemUID).Specific)).Selected != null)
                {
                    sValor = ((SAPbouiCOM.ComboBox)(objForm.Items.Item(sItemUID).Specific)).Selected.Value;
                }
            }
            catch (Exception e)
            {
                //ExceptionAddOn.GetInstance().throwException("HELPERS", "3.0", e);
                B1Connections.theAppl.SetStatusBarMessage(String.Format("Helpers.getValueFormUserDataSource - {0}", e.Message));
            }

            return sValor;
        }

        public static string getDescriptionFromComboBox(SAPbouiCOM.Form objForm, string sItemUID)
        {
            SAPbouiCOM.ComboBox objComboBox = null;
            string sValor = "";

            try
            {
                objComboBox = ((SAPbouiCOM.ComboBox)(objForm.Items.Item(sItemUID).Specific));
                if (objComboBox.Selected != null)
                {
                    sValor = objComboBox.Selected.Description;
                }
            }
            catch (Exception e)
            {
                //ExceptionAddOn.GetInstance().throwException("HELPERS", "3.0", e);
                B1Connections.theAppl.SetStatusBarMessage(String.Format("Helpers.getValueFormUserDataSource - {0}", e.Message));
            }

            return sValor;
        }


        public static void SetText(string msg, BoMessageTime time = BoMessageTime.bmt_Medium, BoStatusBarMessageType tipo = BoStatusBarMessageType.smt_Error, Assembly pAssembly = null)
        {
            try
            {
                if (pAssembly == null)
                    pAssembly = Assembly.GetCallingAssembly();

                B1Connections.theAppl.StatusBar.SetText(string.Format("[{0}] {1}", pAssembly.GetName().Name, msg), time, tipo);
            }
            catch (Exception e)
            {
               Helpers.SetText(string.Format("Helpers.SetText - {0}", e.Message));
            }

        }

        public static string AbrirDialog(string sDiretorio, string sArquivo, string sFiltro, TipoDialog tTipo)
        {
            if (sDiretorio.Trim() == "")
            {
                initialDirectory = @"C:\";
            }
            else
            {
                initialDirectory = sDiretorio;
            }
            initialFile = sArquivo;
            filterFiles = sFiltro;
            typeDialog = tTipo;

            //------------------------------------------------------//

            Thread t = new Thread(new ThreadStart(SelectTextFile));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            //------------------------------------------------------//

            return (selectedFile);
        }

        [STAThread]
        private static void SelectTextFile()
        {
            System.Windows.Forms.Form objForm = new System.Windows.Forms.Form();

            objForm.TopMost = true;
            objForm.Height = 0;
            objForm.Width = 0;
            objForm.WindowState = FormWindowState.Minimized;
            objForm.Visible = true;
            //----------------------------------------------------------------//
            if (typeDialog == TipoDialog.tSAVE)
            {
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Title = "Informe o nome do Arquivo";
                dialog.Filter = filterFiles; //"TXT files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.InitialDirectory = initialDirectory;
                dialog.FileName = initialFile;
                //----------------------------------------------------------------//
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    objForm.Close();
                    selectedFile = dialog.FileName;
                }
                else
                {
                    objForm.Close();
                    selectedFile = "";
                }
            }
            else if (typeDialog == TipoDialog.tOPEN)
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Title = "Informe o nome do Arquivo";
                dialog.Filter = filterFiles; //"TXT files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.InitialDirectory = initialDirectory;
                dialog.FileName = initialFile;
                //----------------------------------------------------------------//
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    objForm.Close();
                    selectedFile = dialog.FileName;
                }
                else
                {
                    objForm.Close();
                    selectedFile = "";
                }
            }
            else if (typeDialog == TipoDialog.tFOLDER)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();

                dialog.Description = "Informe a pasta";
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                //----------------------------------------------------------------//
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    objForm.Close();
                    selectedFile = dialog.SelectedPath;
                }
                else
                {
                    objForm.Close();
                    selectedFile = "";
                }
            }
        }

        public static string GetResource(string filename, Assembly pAssembly = null)
        {

            if (pAssembly == null)
                pAssembly = Assembly.GetCallingAssembly();

            

            string retorno;
            using (var s = pAssembly.GetManifestResourceStream(string.Format("{0}.{1}.{2}.sql", "MooveWestAddon", SAPResource, filename)))
            {
                using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding(CultureInfo.GetCultureInfo("pt-BR").TextInfo.ANSICodePage)))
                {
                    retorno = sr.ReadToEnd();
                }
            }


            return retorno;

        }

        public static Recordset GetRecordSet(string query)
        {

            try
            {
                var rs = (Recordset)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(query);
                return rs;
            }
            catch (Exception ex)
            {
                Helpers.SetText(string.Format("Erro ao executar consulta: [{0}].  Consulta: [{1}]", ex.Message, query));
            }

            return null;

        }

        public static string[] RSToArray(Recordset objRs, string separador = "", bool marcadorEspaco = false)
        {
            List<string> strings = new List<string>();
            try
            {
                objRs.MoveFirst();
                do
                {
                    string linha = "";
                    for (int i = 0; i < objRs.Fields.Count; i++)
                    {
                        string valor = objRs.Fields.Item(i).Value.ToString();
                        if (marcadorEspaco)
                        {
                            valor = valor.Substring(1).Substring(0, valor.Length - 2);
                        }
                        linha += valor + separador;
                    }
                    strings.Add(linha);
                    objRs.MoveNext();
                } while (!objRs.EoF);

                return strings.ToArray();
            }
            catch
            {
                throw;
            }
        }

        public static void LimparRecordSetDaMemoria(Recordset objParaLimpar)
        {
            if (objParaLimpar != null)
            {
                Marshal.ReleaseComObject(objParaLimpar);
                objParaLimpar = null;
                GC.Collect();
            }
        }

        private static string GetTypes(custUserField uCampo)
        {

            var tmpTamanho = string.Empty;
            var tmpValorPadrao = string.Empty;
            var tmpObrigatorio = string.Empty;
            var tmpTabelaLigada = string.Empty;
            var tmpValoresValidos = string.Empty;
            var tmpTipoCampo = string.Empty;
            var tmpNomeTabela = string.Empty;
            var tmpFieldId = string.Empty;

            if (uCampo.TipoTabela == TipoTabela.tUSER)
            {
                tmpNomeTabela = "@" + uCampo.NomeTabela;
            }
            else if (uCampo.TipoTabela == TipoTabela.tSBO)
            {
                tmpNomeTabela = uCampo.NomeTabela;
            }


            switch (uCampo.Tipo)
            {
                case TipoCampo.tALPHA:
                    tmpTipoCampo = "<Type>db_Alpha</Type><SubType>st_None</SubType>";
                    break;
                case TipoCampo.tADDRESS:
                    tmpTipoCampo = "<Type>db_Alpha</Type><SubType>st_Address</SubType>";
                    break;
                case TipoCampo.tPHONE:
                    tmpTipoCampo = "<Type>db_Alpha</Type><SubType>st_Phone</SubType>";
                    break;
                case TipoCampo.tMEMO:
                    tmpTipoCampo = "<Type>db_Memo</Type><SubType>st_None</SubType>";
                    break;
                case TipoCampo.tNUMERO:
                    tmpTipoCampo = "<Type>db_Numeric</Type><SubType>st_None</SubType>";
                    break;
                case TipoCampo.tDATE:
                    tmpTipoCampo = "<Type>db_Date</Type><SubType>st_None</SubType>";
                    break;
                case TipoCampo.tTIME:
                    tmpTipoCampo = "<Type>db_Date</Type><SubType>st_Time</SubType>";
                    break;
                case TipoCampo.tTAXA:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Rate</SubType>";
                    break;
                case TipoCampo.tVALOR:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Sum</SubType>";
                    break;
                case TipoCampo.tPRECO:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Price</SubType>";
                    break;
                case TipoCampo.tQUANT:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Quantity</SubType>";
                    break;
                case TipoCampo.tPERC:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Percentage</SubType>";
                    break;
                case TipoCampo.tMEDIDA:
                    tmpTipoCampo = "<Type>db_Float</Type><SubType>st_Measurement</SubType>";
                    break;
                case TipoCampo.tLINK:
                    tmpTipoCampo = "<Type>db_Memo</Type><SubType>st_Link</SubType>";
                    break;
                case TipoCampo.tIMAGE:
                    tmpTipoCampo = "<Type>db_Alpha</Type><SubType>st_Image</SubType>";
                    break;

            }

            if (uCampo.Tamanho > 0)
                tmpTamanho = "<EditSize>" + uCampo.Tamanho + "</EditSize>";
            else
                tmpTamanho = "<EditSize></EditSize>";

            //---------------------------------------------------------------------------//
            if (string.IsNullOrEmpty(uCampo.ValorPadrao))
                tmpValorPadrao = "<DefaultValue></DefaultValue>";
            else
                tmpValorPadrao = "<DefaultValue>" + uCampo.ValorPadrao + "</DefaultValue>";
            //---------------------------------------------------------------------------//
            if (uCampo.Obrigatorio)
                tmpObrigatorio = "<Mandatory>tYES</Mandatory>";
            else
                tmpObrigatorio = "<Mandatory>tNO</Mandatory>";
            //---------------------------------------------------------------------------//
            if (string.IsNullOrEmpty(uCampo.TabelaLigada))
                tmpTabelaLigada = "<LinkedTable></LinkedTable>";
            else
                tmpTabelaLigada = "<LinkedTable>" + uCampo.TabelaLigada + "</LinkedTable>";
            //---------------------------------------------------------------------------//


            if (string.IsNullOrEmpty(uCampo.ValoresValidos))
                tmpValoresValidos = "<ValidValuesMD></ValidValuesMD>";
            else
            {

                tmpValoresValidos = "<ValidValuesMD>";
                var auxValores1 = uCampo.ValoresValidos.Split('|');
                foreach (string auxValor in auxValores1)
                {
                    var auxValores2 = auxValor.Split(';');
                    tmpValoresValidos += "<row><Value>" + auxValores2[0] + "</Value>" +
                                         "<Description>" + auxValores2[1] + "</Description></row>";
                }

                tmpValoresValidos += "</ValidValuesMD>";
            }

            //---------------------------------------------------------------------------//

            return "<BO><AdmInfo><Object>152</Object><Version>2</Version></AdmInfo><UserFieldsMD><row>" +
                    "<TableName>" + tmpNomeTabela + "</TableName>" +
                    "<Name>" + uCampo.NomeCampo + "</Name>" +
                    "<Description>" + uCampo.Descricao + "</Description>" +
                     tmpTipoCampo + tmpTamanho + tmpValorPadrao + tmpObrigatorio + tmpTabelaLigada + tmpFieldId +
                    "</row></UserFieldsMD>" + tmpValoresValidos + "</BO>";

        }

        private static void ValidarValoresValidos(List<custValoresValidos> listaValoresValidos, string sAddInName)
        {
            var lista = new List<custValoresValidos>();

            foreach (var item in listaValoresValidos)
            {
                var auxValores1 = item.ValoresValidos.Split('|');

                foreach (string auxValor in auxValores1)
                {
                    var auxValores2 = auxValor.Split(';');

                    //Procura o valor válido na lista
                    var naoExiste = true;
                    var oksValoresValidos = new custValoresValidos();
                    oksValoresValidos = new custValoresValidos();
                    oksValoresValidos.Tabela = item.Tabela;
                    oksValoresValidos.Campo = item.Campo;
                    oksValoresValidos.ValoresValidos = item.ValoresValidos;
                    oksValoresValidos.Valor = auxValores2[0];
                    oksValoresValidos.Descricao = auxValores2[1];

                    lista.Add(oksValoresValidos);
                }
            }

            if (lista.Count > 0)
                AtualizarValoresValidos(lista, sAddInName);
        }

        private static void AtualizarValoresValidos(List<custValoresValidos> lista, string sAddInName)
        {
            var objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);
            int intResult;
            try
            {
                //Agrupa os valores validos
                foreach (var item in lista)
                {
                    objUserFields = (UserFieldsMD)B1Connections.diCompany.GetBusinessObject(BoObjectTypes.oUserFields);

                    if (objUserFields.GetByKey(item.Tabela, int.Parse(item.Campo)))
                    {

                        Helpers.SetText("Carregando... " + sAddInName + " --> Aguarde... Atualizando valores válidos para o campo [" + objUserFields.Name + "] na Tabela [" + objUserFields.TableName + "]...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);


                        var validValCount = objUserFields.ValidValues.Count;
                        for (int i = validValCount - 1; i >= 0; i--)
                        {
                            objUserFields.ValidValues.SetCurrentLine(i);
                            objUserFields.ValidValues.Delete();
                        }


                        intResult = objUserFields.Update();
                        if ((intResult != 0) && (intResult != -2035))
                        {
                            Helpers.SetText(string.Format("HELPERS - Erro ao Atualizar valores válidos Campo de Usuário: {0}.{1} - {2}", objUserFields.TableName, objUserFields.Name, B1Connections.diCompany.GetLastErrorDescription()));
                        }
                        else
                        {
                            var auxValores1 = item.ValoresValidos.Split('|');
                            foreach (string auxValor in auxValores1)
                            {
                                var auxValores2 = auxValor.Split(';');
                                objUserFields.ValidValues.Value = auxValores2[0];
                                objUserFields.ValidValues.Description = auxValores2[1];
                                objUserFields.ValidValues.Add();
                            }

                            intResult = objUserFields.Update();
                            if ((intResult != 0) && (intResult != -2035))
                                Helpers.SetText(string.Format("HELPERS - Erro ao Atualizar valores válidos Campo de Usuário: {0}.{1} - {2}", objUserFields.TableName, objUserFields.Name, B1Connections.diCompany.GetLastErrorDescription()));

                        }
                    }

                    Marshal.ReleaseComObject(objUserFields);
                }

                Helpers.SetText("Carregando... " + sAddInName + " --> Aguarde... Concluindo transação. ", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception er)
            {
                Helpers.SetText(string.Format("HELPERS - {0}", er));
            }
            finally
            {
                if (objUserFields != null)
                    Marshal.ReleaseComObject(objUserFields);

                GC.Collect();
            }
        }
    }
}
