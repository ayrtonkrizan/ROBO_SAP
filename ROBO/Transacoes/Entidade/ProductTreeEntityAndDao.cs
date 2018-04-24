using System;
using System.Collections.Generic;
using System.Text;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using MyLibs.v2.local.Register;
using MyLibs.v2.local.DataBase;


namespace ROBO.Transacoes
{
    [ObsoleteAttribute] 
    public class ProductTreeEntity
    {
        public int Id;
        public DateTime AddDate;
        public int TransactionType;
        public char ReadBySAP;

        public string OITM_CodeBars;
        private string OITT_code;

        public string OITT_Code
        {
            get 
            {
                if (string.IsNullOrEmpty(OITT_code))
                    return MyQuery.GetValue("OITM", OITT_code, "CodeBars", "ItemCode").ToString();
                else
                    return OITT_Code; 
            }
            set { OITT_Code = value; }
        }
        public double OITT_Quantity;
        public string OITT_ToWH;
        public int OITT_PriceList;
        public string OITT_Project;
        public char OITT_TreeType;

        public BoItemTreeTypes OITT_TreeTypeBo
        {
            get 
            { 
                switch(OITT_TreeType)
                {
                    case 'A':
                        return BoItemTreeTypes.iAssemblyTree;
                    case 'S':
                        return BoItemTreeTypes.iSalesTree;
                    case 'P':
                        return BoItemTreeTypes.iProductionTree;
                    case 'T':
                        return BoItemTreeTypes.iTemplateTree;
                    default :
                        throw new LogException(this.GetType()
                            ,System.Diagnostics.EventLogEntryType.Error
                            ,Properties.LogMessage.ProductTree_e0018_0);
                        
                }                
            }
        }

        public List<ProductTreeEntityLine> Lines = new List<ProductTreeEntityLine>();
    }
    [ObsoleteAttribute] 
    public class ProductTreeEntityLine
    {
        public int ITT1_Type;
        public ProductionItemType ITT1_TypeBo
        {
            get
            {
                switch(ITT1_Type)
                {
                    case 4:
                        return ProductionItemType.pit_Item;
                    case 290:
                        return ProductionItemType.pit_Resource;
                    case -18:
                        return ProductionItemType.pit_Text;
                    default:
                        throw new LogException(this.GetType()
                            , System.Diagnostics.EventLogEntryType.Error
                            , Properties.LogMessage.ProductTree_e0019_0);
                }
            }
        }
        public string OITM_CodeBars;
        private string ITT1_code;
        public string ITT1_Code
        {
            get 
            { 
                if(String.IsNullOrEmpty(ITT1_code))
                {
                    MyLibs.v2.sbo.DI.MyQuery.GetValue("OITM", OITM_CodeBars, "CodeBars", "ItemCode");

                    //SELECT CodeBars,ItemCode,* FROM OITM
                }

                return ITT1_code; 
            }
            set { ITT1_code = value; }
        }
        public double ITT1_Quantity;
        public double ITT1_Price;
        public string ITT1_WareHouse;
        public char ITT1_IssuedMthd;

        public BoIssueMethod ITT1_IssuedMthdBo
        {
            get 
            {
                switch(ITT1_IssuedMthd)
                {
                    case 'B':
                        return BoIssueMethod.im_Backflush;
                    case 'M':
                        return BoIssueMethod.im_Manual;
                    default:
                        throw new LogException(this.GetType()
                            ,System.Diagnostics.EventLogEntryType.Error
                            , Properties.LogMessage.ProductTree_e0020_0);

                }
            }
        }
        public string ITT1_Comment;
    }
    [ObsoleteAttribute] 
    public class ProductTreeDAO
    {
        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <param name="itemCode">Código do item</param>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        public static ProductTreeEntity GetDados(string itemCode)
        {
            var prodTree = new ProductTreeEntity();   

            using (var conn = new ConnSqlServer())
            {
                var codeBars = MyQuery.GetValue("OITM",itemCode,"ItemCode","CodeBars");

                conn.DoQuery("SELECT * FROM [Sage_WorkTree] WITH (NOLOCK) WHERE ReadBySAP <> 'Y' AND  OITM_CodeBars = '{0}'", codeBars);

                //if(conn.)


                while (conn.HasNext())
                {
                    prodTree.Id = conn.GetFieldValue("Id").ToInt();
                    prodTree.AddDate = conn.GetFieldValue("AddDate").ToDate();
                    prodTree.TransactionType = conn.GetFieldValue("TransactionType").ToInt();
                    prodTree.ReadBySAP = conn.GetFieldValue("ReadBySAP").ToChar();

                    prodTree.OITM_CodeBars = conn.GetFieldValue("OITM_CodeBars").ToString();
                    prodTree.OITT_Code = conn.GetFieldValue("OITT_Code").ToString();
                    prodTree.OITT_Quantity = conn.GetFieldValue("OITT_Quantity").ToDouble();
                    prodTree.OITT_ToWH = conn.GetFieldValue("OITT_ToWH").ToString();
                    prodTree.OITT_PriceList = conn.GetFieldValue("OITT_PriceList").ToInt();
                    prodTree.OITT_Project = conn.GetFieldValue("OITT_Project").ToString();
                    prodTree.OITT_TreeType = conn.GetFieldValue("OITT_TreeType").ToChar();           
         
                    using (var connLine = new MyLibs.v2.local.DataBase.ConnSqlServer())
                    {
                        var line = new ProductTreeEntityLine();

                        connLine.DoQuery("SELECT * FROM [ProductTreeLine] WHERE ReadBySAP <> 'Y'");
                
                        line.ITT1_Type = connLine.GetFieldValue("ITT1_Type").ToInt();
                        line.OITM_CodeBars = connLine.GetFieldValue("OITM_CodeBars").ToString();
                        line.ITT1_Code = connLine.GetFieldValue("ITT1_Code").ToString();
                        line.ITT1_Quantity = connLine.GetFieldValue("ITT1_Quantity").ToDouble();
                        line.ITT1_WareHouse = connLine.GetFieldValue("ITT1_WareHouse").ToString();
                        line.ITT1_IssuedMthd = connLine.GetFieldValue("ITT1_IssuedMthd").ToChar();
                        line.ITT1_Comment = connLine.GetFieldValue("ITT1_Comment").ToString();
                        line.ITT1_Price = connLine.GetFieldValue("ITT1_Price").ToDouble();

                        prodTree.Lines.Add(line);
                    }
                }
            }
            return prodTree;
        }

        /// <summary>
        /// Informa se há transação em aberto
        /// </summary>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        //public static bool TransactionOpen()
        //{
        //    using (var rs0 = new MyRecordSet())
        //    {
        //        rs0.DoQuery("SELECT * FROM [ProductTree] WHERE ReadBySAP <> 'Y' ");

        //        return !rs0.IsNullRows;
        //    }
        //}

        ///// <summary>
        ///// Atualiza o Status da Transação
        ///// </summary>
        ///// <param name="oPedido"></param>
        public static void UpdateStatus(List<ProductTreeEntity> tentitys)
        {
            using (var rs = new MyRecordSet())
            {
                foreach (var tentity in tentitys)
                {
                    UpdateStatus(tentity);
                }
            }
        }

        public static void UpdateStatus(ProductTreeEntity tentity)
        {
            using (var rs = new MyRecordSet())
            {
                var sql = "UPDATE [ProductTree] SET ReadBySAP = '{0}' WHERE Id = '{1}'";
                rs.DoQuery(sql, tentity.ReadBySAP, tentity.Id);

                
            }
        }
    }

}
