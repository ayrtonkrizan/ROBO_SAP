using System;
using System.Collections.Generic;
using MyLibs.v2.local;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using MyLibs.v2.local.Register;

namespace ROBO.Transacoes
{
    public class T38Entity
    {
        public int Id;
        public DateTime AddDate;
        public int TransactionType;
        public char ReadBySAP;
        public string JOMAR_OrderNum;
        public int owor_DocNum = 0;
        public int OWOR_DocNum
        {
            get
            {
                if(owor_DocNum == 0)
                {
                    var docNum = MyQuery.GetValue("OWOR", JOMAR_OrderNum, "U_ORDER_JOMAR", "DocNum").ToInt();
                    //if(docNum > 0)
                        owor_DocNum = docNum;
                    //else
                    //    throw new Exception("Não existe Ordem de Produção no SAP com a Ordem Jomar nº \"" + JOMAR_OrderNum + "\"");
                }

                return owor_DocNum;
            }

            set
            {
                owor_DocNum = value;
            }
        }
        
        public int WOR1_DocNum;
        public string To_WareHouse;
        public string From_WareHouse;
        public string OITM_CodeBars;
        public string OITM_ItemCode
        {
            get
            {
                using (var conn = new MyRecordSet())
                {
                    conn.DoQuery("SELECT Tipo, Codigo FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", OITM_CodeBars);
                    if (conn.HasNext())
                        return conn.GetFieldValue(1).ToString();
                    else
                        throw new Exception("Não foi localizado item/recurso com o código de barras \"" + OITM_CodeBars + "\"");
                }
            }
        }
        public ProductionItemType ItemTypeBo
        {
            get
            {
                using (var conn = new MyRecordSet())
                {
                    conn.DoQuery("SELECT Tipo, Codigo FROM RSD_IsItemOrResource WHERE Referencia = '{0}'", OITM_CodeBars);
                    if (conn.HasNext())
                    {
                        var tipo = conn.GetFieldValue(0).ToChar();
                        switch (conn.GetFieldValue(0).ToChar())
                        {
                            case 'R': return ProductionItemType.pit_Resource;
                            case 'I': return ProductionItemType.pit_Item;
                            default: return ProductionItemType.pit_Item;
                        }
                    }
                    else
                    {
                        throw new Exception("Não foi localizado item/recurso com o código de barras \"" + OITM_CodeBars + "\"");
                    }


                }
            }
        }
        public int JOMAR_LineNum;
        public double WOR1_BaseQty;
        public double WOR1_PlannedQty;
        public int OIGE_DocNum;
        public int OIGN_DocNum;
        public double IGE1_Quantity;
        private string msg_SAP;

        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'", "\"").Trim(); }
        }
        public char Update_CodeBars;
        public int WOR1_LineNum;
    }
    
    

    public class T38DAO
    {
        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <param name="JOMAR_OrderNum">Número de Ordem Jomar (0 todos)</param>
        /// <returns></returns>
        public static List<T38Entity> GetDados(int JOMAR_OrderNum = 0)
        {
            var transacoes = new List<T38Entity>();

            using (var conn = new MyRecordSet())
            {
                if(JOMAR_OrderNum > 0)
                    conn.DoQuery("SELECT * FROM [Sage_Transaction38] WITH (NOLOCK) WHERE TransactionType = '38' AND [JOMAR_OrderNum] = '{0}' AND (WOR1_DocNum IS NULL OR WOR1_DocNum = 0)", JOMAR_OrderNum);
                else
                    conn.DoQuery("SELECT * FROM [Sage_Transaction38] T0 WITH (NOLOCK) inner join OITM T1 on T1.CodeBars = T0.oitm_Codebars inner join OWOR T3 on T3.U_ORDER_JOMAR = T0.Jomar_OrderNum inner join WOR1 T2 on T2.DocEntry = T3.DocEntry and T2.itemcode = T1.ItemCode WHERE TransactionType = '38' AND [ReadBySAP] <> 'Y' AND (OIGE_DocNum IS NULL OR OIGE_DocNum = 0) order by T0.Jomar_OrderNum "); //(WOR1_DocNum IS NULL OR WOR1_DocNum = 0) AND 

                principal.GravaAudit("Inicio While ");
                while (conn.HasNext())
                {
                    var t38 = new T38Entity();
                    try
                    {
                        principal.GravaAudit(conn.GetFieldValue("JOMAR_OrderNum").ToString());
                        t38.Id = conn.GetFieldValue("Id").ToInt();
                        t38.AddDate = conn.GetFieldValue("AddDate").ToDate();
                        t38.Update_CodeBars = conn.GetFieldValue("Update_CodeBars").ToChar();
                        t38.TransactionType = conn.GetFieldValue("TransactionType").ToInt();
                        t38.ReadBySAP = conn.GetFieldValue("ReadBySAP").ToChar();
                        t38.JOMAR_OrderNum = conn.GetFieldValue("JOMAR_OrderNum").ToString();
                        t38.WOR1_DocNum = conn.GetFieldValue("WOR1_DocNum").ToInt();
                        t38.To_WareHouse = "PP";
                        t38.From_WareHouse = conn.GetFieldValue("WOR1_WareHouse").ToString();
                        t38.OITM_CodeBars = conn.GetFieldValue("OITM_CodeBars").ToString();
                        t38.JOMAR_LineNum = conn.GetFieldValue("JOMAR_LineNum").ToInt();
                        t38.WOR1_BaseQty = conn.GetFieldValue("WOR1_BaseQty").ToDouble();
                        t38.WOR1_PlannedQty = conn.GetFieldValue("WOR1_PlannedQty").ToDouble();
                        t38.IGE1_Quantity= conn.GetFieldValue("IGE1_Quantity").ToDouble();
                        t38.OWOR_DocNum = MyQuery.GetValue("OWOR", t38.JOMAR_OrderNum, "U_ORDER_JOMAR", "DocNum").ToInt();

                        principal.GravaAudit("Existe OPs " + t38.OWOR_DocNum);
                        if ((Program.contador % 2) == 0)
                        {
                            //if (t38.ReadBySAP != 'E')
                                transacoes.Add(t38);
                        }
                        else
                        {
                            transacoes.Add(t38);
                        }

                    } 
                    catch(Exception ex) {
                        t38.ReadBySAP = 'E';
                        t38.MSG_SAP = ex.Message;

                        UpdateStatus(t38);

                        principal.GravaAudit("Transação 38 - " + t38.Id.ToString() + " " + ex.Message.ToString());
                    }    

                }
                
                principal.GravaAudit("Termino While");

             return transacoes;
            }
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
                rs.DoQuery("SELECT COUNT(*) FROM [Sage_Transaction38] WITH (NOLOCK) WHERE ReadBySAP <> 'Y' AND TransactionType = '38'");

                if (rs.HasNext())
                {
                    if (rs.GetFieldValue(0).ToInt() > 0)
                    {
                        principal.GravaAudit("Transação 38 - Encontrado {0} transações não processadas " + rs.GetFieldValue(0).ToString());
                    }
                    else
                    {
                        principal.GravaAudit("Transação 38 Nenhuma transação em aberto.");
                    }

                    return (rs.GetFieldValue(0).ToInt() > 0);
                }
                    return false;
            }
        }

        /// <summary>
        /// Atualiza o Status da Transação
        /// </summary>
        /// <param name="oPedido"></param>
        public static void UpdateStatus(List<T38Entity> t38Entitys)
        {
            using (var rs0 = new MyRecordSet())
            {
                foreach (var t38Entity in t38Entitys)
                {
                    UpdateStatus(t38Entity);
                }
            }
        }

        public static void UpdateStatus(T38Entity t38)
        {
            using (var rs0 = new MyRecordSet())
            {
                var sql = "UPDATE [Sage_Transaction38] SET ReadBySAP = '{0}', [OIGE_DocNum] = '{1}',[MSG_SAP] = '{3}',Update_CodeBars = '{4}', WOR1_DocNum = '{5}'  WHERE Id = '{2}'";          
                rs0.DoQuery(sql, t38.ReadBySAP, t38.OIGE_DocNum, t38.Id, t38.MSG_SAP, t38.Update_CodeBars, t38.owor_DocNum);                
            }
        }
    }
}
