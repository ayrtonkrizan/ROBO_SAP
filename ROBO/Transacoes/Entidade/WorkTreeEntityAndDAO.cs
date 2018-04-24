using System;
using System.Collections.Generic;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using MyLibs.v2.local.DataBase;

namespace ROBO.Transacoes.Entidade
{
    public class WorkTreeEntity
    {
        public string ID_CodeBars;
        public char ReadBySAP;
        public double OITT_Quantity;
        public int OITT_PriceList;
        public string OITT_ToWH;
        public string OITT_Project;

        public string OITT_Code
        {
            get 
            {
                var rs = (SAPbobsCOM.Recordset)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                rs.DoQuery(String.Format("SELECT * FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", ID_CodeBars.Trim()));
                if(!rs.EoF)
                {
                    return rs.Fields.Item("Codigo").Value.ToString();
                }
                else 
                {
                    throw new LogException(this.GetType()
                                , System.Diagnostics.EventLogEntryType.Error
                                , Properties.LogMessage.GLOBAL_0029_1);
                }
            }                                             
        }
        
        public List<WorkTreeL1Entity> Lines = new List<WorkTreeL1Entity>();
        private string msg_SAP;

        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'", "\"").Trim(); }
        }
    }

    public class WorkTreeL1Entity
    {
        public int ITT1_ChildNum;
        public int ITT1_Type;

        public ProductionItemType ITT1_TypeBo
        {
            get
            {
                    var rs = (SAPbobsCOM.Recordset)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    rs.DoQuery(String.Format("SELECT * FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", OITM_CodeBars.Trim()));
                    if (!rs.EoF)
                    {                       
                        switch (rs.Fields.Item("Tipo").Value.ToString())
                        {
                            case "I":
                                return ProductionItemType.pit_Item;
                            case "R":
                                return ProductionItemType.pit_Resource;
                            //case -18:
                            //    return ProductionItemType.pit_Text;
                            default:
                                throw new LogException(this.GetType()
                                    , System.Diagnostics.EventLogEntryType.Error
                                    , Properties.LogMessage.ProductTree_e0019_0);
                        }
                    }
                    else
                    {
                        throw new LogException(this.GetType()
                                    , System.Diagnostics.EventLogEntryType.Error
                                    , Properties.LogMessage.GLOBAL_0029_1, OITM_CodeBars.Trim());
                    }
            }            
            set 
            {

                int lista = (int) value;

                switch (lista)
                {    
                    case 0:
                        ITT1_Type = 4; break;
                    case 1:
                        ITT1_Type = 290; break;
                    case 2:
                        ITT1_Type = -18; break;
                    default:
                        throw new LogException(this.GetType()
                            , System.Diagnostics.EventLogEntryType.Error
                            , Properties.LogMessage.ProductTree_e0019_0);
                }
            }
        }

        public string OITM_CodeBars;
        public string ITT1_WareHouse;
        public double ITT1_Price;
        public string ITT1_ItemCode
        {
            get
            {
                var rs = (SAPbobsCOM.Recordset)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                rs.DoQuery(String.Format("SELECT Codigo FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", OITM_CodeBars.Trim()));
                if (!rs.EoF)
                {
                    return rs.Fields.Item("Codigo").Value.ToString();
                }
                else
                {
                    throw new LogException(this.GetType()
                                , System.Diagnostics.EventLogEntryType.Error
                                , Properties.LogMessage.GLOBAL_0029_1
                                , OITM_CodeBars.Trim());
                }
            }
        }
        public double ITT1_Quantity;
        public char ITT1_IssueMthd;
        public SAPbobsCOM.BoIssueMethod ITT1_IssueMthdBo
        {
            get
            {
                switch(ITT1_IssueMthd)
                {
                    case 'M':
                        return BoIssueMethod.im_Manual;
                    case 'B':
                        return BoIssueMethod.im_Backflush;
                    default:
                        return BoIssueMethod.im_Manual;
                }            
            }
        }
    }

    public class WorkTreeDAO
    {
        /// <summary>
        /// Retorna as estruturas dos itens não lidas ou com erro.
        /// </summary>
        /// <returns></returns>
        public static List<WorkTreeEntity> GetDados()
        {
            var ListWorkTree = new List<WorkTreeEntity>();

            using (var rs0 = new ConnSqlServer())
            {
                rs0.DoQuery("SELECT * FROM [Sage_WorkTree] WITH (NOLOCK) WHERE ReadBySAP <> 'Y'");

                while (rs0.HasNext())
                {
                    var wte = new WorkTreeEntity();
                    try
                    {                        
                        // Documento
                        wte.ID_CodeBars = rs0.GetFieldValue("ID_CodeBars").ToString();
                        wte.OITT_PriceList = rs0.GetFieldValue("OITT_PriceList").ToInt();
                        wte.OITT_Project = rs0.GetFieldValue("OITT_Project").ToString();
                        wte.OITT_Quantity = rs0.GetFieldValue("OITT_Quantity").ToDouble();
                        wte.OITT_ToWH = rs0.GetFieldValue("OITT_ToWH").ToString();
                        wte.ReadBySAP = rs0.GetFieldValue("ReadBySAP").ToChar();
                        wte.MSG_SAP = rs0.GetFieldValue("MSG_SAP").ToString();

                        var rs1 = new ConnSqlServer();
                        rs1.DoQuery("SELECT * FROM [Sage_WorkTreeL1] WITH (NOLOCK) WHERE [ID_CodeBars] = '{0}'", wte.ID_CodeBars);
                        while (rs1.HasNext())
                        {
                            var wtl1e = new WorkTreeL1Entity();
                            // Linha
                            //wtl1e.ITT1_ChildNum = rs_header.GetFieldValue("ITT1_ChildNum").ToInt();
                            wtl1e.ITT1_Quantity = rs1.GetFieldValue("ITT1_Quantity").ToDouble();
                            //wtl1e.ITT1_Type = rs_header.GetFieldValue("ITT1_Type").ToInt();
                            wtl1e.OITM_CodeBars = rs1.GetFieldValue("OITM_CodeBars").ToString();


                            if (!MyQuery.Exist("RSD_IsItemOrResource", wtl1e.OITM_CodeBars, "Referencia"))
                                throw new Exception("Código de barras não cadastrado " + wtl1e.OITM_CodeBars);

                            if (MyQuery.GetValue("RSD_IsItemOrResource", wtl1e.OITM_CodeBars, "Referencia", "Tipo").ToChar() == 'I')
                            {

                                if (!MyQuery.GetValue("OITM", wtl1e.ITT1_ItemCode, "ItemCode", "InvntItem").ToBoolean())
                                    wtl1e.ITT1_IssueMthd = 'B';
                                else
                                    wtl1e.ITT1_IssueMthd = rs1.GetFieldValue("ITT1_IssueMthd").ToChar();
                            } 
                            else
                            {
                                wtl1e.ITT1_IssueMthd = rs1.GetFieldValue("ITT1_IssueMthd").ToChar();
                            }

                            wtl1e.ITT1_WareHouse = rs1.GetFieldValue("ITT1_WareHouse").ToString();
                            wtl1e.ITT1_Price = rs1.GetFieldValue("ITT1_Price").ToDouble();

                            wte.Lines.Add(wtl1e);
                        }

                        ListWorkTree.Add(wte);
                    }
                    catch(Exception ex)
                    {
                        wte.ReadBySAP = 'E';
                        wte.MSG_SAP = ex.Message;
                        WorkTreeDAO.UpdateStatus(wte);

                        Log.Register("WorkTree", System.Diagnostics.EventLogEntryType.Error
                            , ex.Message);
                    }                    
                }
            }
            Log.Register("WorkTree", System.Diagnostics.EventLogEntryType.Information, "0 - Foram criados {0} pacotes", ListWorkTree.Count);

            return ListWorkTree;
        }

        /// <summary>
        /// Atualiza o status.
        /// </summary>
        /// <param name="t38"></param>
        public static void UpdateStatus(WorkTreeEntity tentity)
        {
            using (var rs = new MyRecordSet())
            {
                var sql = "UPDATE [Sage_WorkTree] SET ReadBySAP = '{0}', MSG_SAP = '{2}' WHERE ID_CodeBars = '{1}'";
                rs.DoQuery(sql, tentity.ReadBySAP, tentity.ID_CodeBars, tentity.MSG_SAP.Replace("'","\""));
            }
        }
    }
}
