using System;
using System.Collections.Generic;
using System.Text;
using MyLibs.v2.sbo.DI;
using MyLibs.v2.local.Register;

namespace ROBO.Transacoes
{
    
    public class T32Entity
    {

        #region Propriedades
        public int Id;
        public int transType = 32;
        public char ReadBySAP;        
        private string msg_SAP;
        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'", "\"").Trim(); }
        }

        private int ordr_DocEntry = 0;

        public int ORDR_DocEntry
        {
            get 
            {
                if (ordr_DocEntry == 0)
                {
                    if (MyQuery.Exist("ORDR", JOMAR_OrderNum.ToString(), "DocNum"))
                        return ordr_DocEntry;
                    else
                        return 0;
                } 
                else
                {
                    return ordr_DocEntry;
                }
            }
            
            
            set { ordr_DocEntry = value; }
        }

        public int SalesHeaderIndex;
        public DateTime ORDR_DocDate;
        public DateTime ORDR_DocDueDate;
        public DateTime ORDR_TaxDate;
        public string ORDR_NumAtCard;
        public int ORDR_BplId;
        public string ORDR_Comments;
        public int JOMAR_OrderNum;
        public int TransactionType;
        public int JOMAR_OIM;
        public string JOMAR_CardCode;

        public List<T32EntityLine> Lines = new List<T32EntityLine>();        
        #endregion

    }
    
    public class T32EntityLine
    {
        public int JOMAR_OrderNum;
        public char ReadBySAP;
        public int Jomar_LineNum;
        public int ORDR_DocEntry;
        public int OINV_DocEntry;
        public string OITM_CodeBars;
        public double RDR1_Quantity;
        public char RDR1_LineStatus;
        public string RDR1_WhsCode;
        public double JOMAR_Peso;
        public int SalesOrderIndex;
        private string msg_SAP;
        public int Id;
        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'", "\"").Trim(); }
        }

        private string oitm_ItemCode = null;
        public int RDR1_LineNum;
        public string OITM_ItemCode
        {
            get
            {
                if (String.IsNullOrEmpty(oitm_ItemCode))
                {
                    using (var conn = new MyRecordSet())
                    {
                        conn.DoQuery("SELECT Tipo, Codigo FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", OITM_CodeBars);
                        if (conn.HasNext())
                            oitm_ItemCode = conn.GetFieldValue(1).ToString();
                        else
                            throw new Exception("Não foi localizado item/recurso com o código de barras \"" + OITM_CodeBars + "\"");

                    }
                }
                    return oitm_ItemCode;
            }

            set
            {
                oitm_ItemCode = value;
            }
        }
       
    }

    public class T32DAO
    {
        public static List<T32Entity> GetNewOINV()
        {
            var t32list = new List<T32Entity>();

            using (var rs_lines = new MyRecordSet())
            {
                rs_lines.DoQuery(@"
SELECT JOMAR_OrderNum, 'N' as 'ReadBySAP',Jomar_LineNum,RDR1_LineNum,ORDR_DocEntry,OINV_DocEntry,OITM_CodeBars,RDR1_WhsCode,SUM(RDR1_Quantity) as 'RDR1_Quantity', SUM(JOMAR_Peso) as 'JOMAR_Peso', 0 as 'SalesOrderIndex', '' as 'MSG_SAP'
FROM [Sage_Transacoes32L1] WITH (NOLOCK) WHERE ORDR_DocEntry > 0 AND (OINV_DocEntry < 1 OR OINV_DocEntry IS NULL) 
GROUP BY JOMAR_OrderNum,Jomar_LineNum,RDR1_LineNum,ORDR_DocEntry,OINV_DocEntry,OITM_CodeBars,RDR1_WhsCode
ORDER BY JOMAR_OrderNum");

                var t32 = new T32Entity();
                var t32line = new T32EntityLine();

                var control = -1;
                while (rs_lines.HasNext())
                {
                    try
                    {
                        if (control == -1) // Controle Inicial
                            control = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt(); // Nº da ordem no controle
                        else if (control != rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt())
                        {
                            t32list.Add(t32); // Guarda o pacote na lista
                            t32 = new T32Entity(); // Cria um novo header
                            control = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt(); // Nº da ordem no controle
                        }

                        #region Linhas
                        t32line = new T32EntityLine();

                        t32line.Id = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                        t32line.JOMAR_OrderNum = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                        t32line.ReadBySAP = rs_lines.GetFieldValue("ReadBySAP").ToChar();
                        t32line.Jomar_LineNum = rs_lines.GetFieldValue("Jomar_LineNum").ToInt();
                        t32line.ORDR_DocEntry = rs_lines.GetFieldValue("ORDR_DocEntry").ToInt();
                        t32line.OINV_DocEntry = rs_lines.GetFieldValue("OINV_DocEntry").ToInt();
                        t32line.OITM_CodeBars = rs_lines.GetFieldValue("OITM_CodeBars").ToString();
                        t32line.RDR1_Quantity = rs_lines.GetFieldValue("RDR1_Quantity").ToDouble();
                        t32line.RDR1_WhsCode = rs_lines.GetFieldValue("RDR1_WhsCode").ToString();
                        t32line.JOMAR_Peso = rs_lines.GetFieldValue("JOMAR_Peso").ToDouble();
                        t32line.SalesOrderIndex = rs_lines.GetFieldValue("SalesOrderIndex").ToInt();
                        t32line.MSG_SAP = rs_lines.GetFieldValue("MSG_SAP").ToString();
                        #endregion

                        var rs_header = new MyRecordSet();

                        rs_header.DoQuery("SELECT * FROM [Sage_Transacoes32] WITH (NOLOCK) WHERE JOMAR_OrderNum = '{0}'", rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt());
                        {
                            #region Cabeçalho
                            if (t32line.JOMAR_OrderNum != t32.JOMAR_OrderNum)
                            {
                                t32.Id = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                                t32.JOMAR_OrderNum = rs_header.GetFieldValue("JOMAR_OrderNum").ToInt();
                                t32.ReadBySAP = rs_header.GetFieldValue("ReadBySAP").ToChar();
                                t32.TransactionType = rs_header.GetFieldValue("TransactionType").ToInt();
                                t32.JOMAR_OIM = rs_header.GetFieldValue("JOMAR_OIM").ToInt();
                                t32.ORDR_DocEntry = rs_header.GetFieldValue("ORDR_DocEntry").ToInt();
                                t32.JOMAR_CardCode = rs_header.GetFieldValue("JOMAR_CardCode").ToString();
                                t32.ORDR_DocDate = rs_header.GetFieldValue("ORDR_DocDate").ToDate();
                                t32.ORDR_DocDueDate = rs_header.GetFieldValue("ORDR_DocDueDate").ToDate();
                                t32.ORDR_TaxDate = rs_header.GetFieldValue("ORDR_TaxDate").ToDate();
                                t32.ORDR_NumAtCard = rs_header.GetFieldValue("ORDR_NumAtCard").ToString();
                                t32.ORDR_BplId = rs_header.GetFieldValue("ORDR_BplId").ToInt();
                                t32.ORDR_Comments = rs_header.GetFieldValue("ORDR_Comments").ToString();
                                t32.SalesHeaderIndex = rs_header.GetFieldValue("SalesHeaderIndex").ToInt();
                                t32.MSG_SAP = rs_header.GetFieldValue("MSG_SAP").ToString();
                            }
                            #endregion
                        }

                        t32line.ORDR_DocEntry = t32.ORDR_DocEntry;

                    }
                    catch (Exception ex)
                    {
                        principal.GravaAudit(ex.Message.ToString());
                    }

                    t32.Lines.Add(t32line);
                }

                // Pega o ultimo que sobra
                if (rs_lines.CountLines() > 0)
                    t32list.Add(t32);
            }


            return t32list;
        }

        /// <summary>
        /// Obtem os dados para criar um novo Pedido de Venda e Nota Fiscal (Rascunho).
        /// </summary>
        /// <returns></returns>
        public static List<T32Entity> GetNewORDR()
        {
            var t32list = new List<T32Entity>();

            using(var rs_lines = new MyRecordSet())
            {
                rs_lines.DoQuery("SELECT * FROM [Sage_Transacoes32L1] WITH (NOLOCK)  WHERE ReadBySAP <> 'Y' AND (ORDR_DocEntry <= 0 OR ORDR_DocEntry IS NULL) ORDER BY JOMAR_OrderNum ASC");

                var t32 = new T32Entity();
                var t32line = new T32EntityLine();

                var control = -1;   
                while(rs_lines.HasNext())
                {
                    try
                    {
                        if(control == -1) // Controle Inicial
                            control = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt(); // Nº da ordem no controle
                        else if(control != rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt())
                        {
                            t32list.Add(t32); // Guarda o pacote na lista
                            t32 = new T32Entity(); // Cria um novo header
                            control = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt(); // Nº da ordem no controle
                        }

                        #region Linhas
                        t32line = new T32EntityLine();

                        t32line.Id = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                        t32line.JOMAR_OrderNum = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                        t32line.ReadBySAP = rs_lines.GetFieldValue("ReadBySAP").ToChar();
                        t32line.Jomar_LineNum = rs_lines.GetFieldValue("Jomar_LineNum").ToInt();
                        t32line.ORDR_DocEntry = rs_lines.GetFieldValue("ORDR_DocEntry").ToInt();
                        t32line.OINV_DocEntry = rs_lines.GetFieldValue("OINV_DocEntry").ToInt();
                        t32line.OITM_CodeBars = rs_lines.GetFieldValue("OITM_CodeBars").ToString();
                        t32line.RDR1_Quantity = rs_lines.GetFieldValue("RDR1_Quantity").ToDouble();
                        t32line.RDR1_WhsCode = rs_lines.GetFieldValue("RDR1_WhsCode").ToString();
                        t32line.JOMAR_Peso = rs_lines.GetFieldValue("JOMAR_Peso").ToDouble();
                        t32line.SalesOrderIndex = rs_lines.GetFieldValue("SalesOrderIndex").ToInt();
                        t32line.MSG_SAP = rs_lines.GetFieldValue("MSG_SAP").ToString();
                        #endregion

                        var rs_header = new MyRecordSet();

                        rs_header.DoQuery("SELECT * FROM [Sage_Transacoes32] WITH (NOLOCK) WHERE JOMAR_OrderNum = '{0}'", rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt());
                        {                       
                            #region Cabeçalho
                            if(t32line.JOMAR_OrderNum != t32.JOMAR_OrderNum)
                            {
                                t32.Id = rs_lines.GetFieldValue("JOMAR_OrderNum").ToInt();
                                t32.JOMAR_OrderNum = rs_header.GetFieldValue("JOMAR_OrderNum").ToInt();
                                t32.ReadBySAP = rs_header.GetFieldValue("ReadBySAP").ToChar();
                                t32.TransactionType = rs_header.GetFieldValue("TransactionType").ToInt();
                                t32.JOMAR_OIM = rs_header.GetFieldValue("JOMAR_OIM").ToInt();
                                t32.ORDR_DocEntry = rs_header.GetFieldValue("ORDR_DocEntry").ToInt();
                                t32.JOMAR_CardCode = rs_header.GetFieldValue("JOMAR_CardCode").ToString();
                                t32.ORDR_DocDate = rs_header.GetFieldValue("ORDR_DocDate").ToDate();
                                t32.ORDR_DocDueDate = rs_header.GetFieldValue("ORDR_DocDueDate").ToDate();
                                t32.ORDR_TaxDate = rs_header.GetFieldValue("ORDR_TaxDate").ToDate();
                                t32.ORDR_NumAtCard = rs_header.GetFieldValue("ORDR_NumAtCard").ToString();
                                t32.ORDR_BplId = rs_header.GetFieldValue("ORDR_BplId").ToInt();
                                t32.ORDR_Comments = rs_header.GetFieldValue("ORDR_Comments").ToString();
                                t32.SalesHeaderIndex = rs_header.GetFieldValue("SalesHeaderIndex").ToInt();
                                t32.MSG_SAP = rs_header.GetFieldValue("MSG_SAP").ToString();
                            }
                            #endregion
                        }

                        t32line.ORDR_DocEntry = t32.ORDR_DocEntry;

                    } 
                    catch(Exception ex)
                    {
                        principal.GravaAudit(ex.Message.ToString());
                    }

                    t32.Lines.Add(t32line);                        
                }

                // Pega o ultimo que sobra
                if(rs_lines.CountLines() > 0)
                    t32list.Add(t32);
            }


            return t32list;
        }

        /// <summary>
        /// Informa se há transação em aberto
        /// </summary>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        public static bool TransactionOpen()
        {
            using (var rs = new MyRecordSet())
            {
                rs.DoQuery("SELECT COUNT(*) FROM SAGE_TRANSACOES32L1 WITH (NOLOCK) WHERE ReadBySAP <> 'Y'");

                if (rs.HasNext())
                {

                    return (rs.GetFieldValue(0).ToInt() > 0);
                }

                return false;
            }
        }

        public static void UpdateStatus(T32Entity t32, bool grupo = false)
        {
            if(grupo)
            {
                foreach (var line in t32.Lines)
                {
                    var sql1 = new StringBuilder();
                    sql1.AppendLine("UPDATE");
                    sql1.AppendLine("[dbo].[Sage_Transacoes32L1] SET");

                    sql1.AppendLine(String.Format("ReadBySAP = '{0}'", line.ReadBySAP));
                    sql1.AppendLine(String.Format(",ORDR_DocEntry = '{0}'", line.ORDR_DocEntry));
                    sql1.AppendLine(String.Format(",OINV_DocEntry = '{0}'", line.OINV_DocEntry));                    
                    sql1.AppendLine(String.Format(",MSG_SAP = '{0}'", line.MSG_SAP));

                    sql1.AppendLine(String.Format("WHERE"));
                    sql1.AppendLine(String.Format("JOMAR_OrderNum = '{0}' AND ORDR_DocEntry = '{1}'", line.JOMAR_OrderNum, line.ORDR_DocEntry));

                    (new MyRecordSet()).DoQuery(sql1.ToString());
                    sql1 = null;
                }

                return;
            }

            StringBuilder sql = new StringBuilder();

            sql.AppendLine("UPDATE");
            sql.AppendLine("[dbo].[Sage_Transacoes32] SET");

            sql.AppendLine(String.Format("ReadBySAP = '{0}'",t32.ReadBySAP));
            sql.AppendLine(String.Format(",ORDR_DocEntry = '{0}'",t32.ORDR_DocEntry));
            sql.AppendLine(String.Format(",ORDR_Comments = '{0}'",t32.ORDR_Comments));
            sql.AppendLine(String.Format(",MSG_SAP = '{0}'",t32.MSG_SAP));           

            sql.AppendLine(String.Format("WHERE"));
            sql.AppendLine(String.Format("JOMAR_OrderNum = '{0}'",t32.JOMAR_OrderNum));

            (new MyRecordSet()).DoQuery(sql.ToString());
            sql = null;

            foreach(var line in t32.Lines)
            {
                sql = new StringBuilder();
                sql.AppendLine("UPDATE");
                sql.AppendLine("[dbo].[Sage_Transacoes32L1] SET");

                sql.AppendLine(String.Format("ReadBySAP = '{0}'", line.ReadBySAP));
                sql.AppendLine(String.Format(",ORDR_DocEntry = '{0}'", line.ORDR_DocEntry));
                sql.AppendLine(String.Format(",OINV_DocEntry = '{0}'", line.OINV_DocEntry));
                sql.AppendLine(String.Format(",RDR1_LineNum = '{0}'", line.RDR1_LineNum));
                sql.AppendLine(String.Format(",MSG_SAP = '{0}'", line.MSG_SAP));

                sql.AppendLine(String.Format("WHERE"));
                sql.AppendLine(String.Format("JOMAR_OrderNum = '{0}' AND SalesOrderIndex = '{1}'", line.JOMAR_OrderNum, line.SalesOrderIndex));

                (new MyRecordSet()).DoQuery(sql.ToString());
                sql = null;
            }         
        }

    }
}
