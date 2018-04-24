using MyLibs.v2.local.DataBase;
using MyLibs.v2.local.Register;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using System;
using System.Collections.Generic;

namespace ROBO.Transacoes
{
    public class T41Entity
    {
        #region Propriedades
        public int Id;
        public int TransType;
        public char ReadBySAP;
        /////
        public string OWOR_U_ORDER_JOMAR;
        /// <summary>
        /// Informa se existe o mesmo número da ordem Jomar no SAP.
        /// </summary>
        public bool Exist_OWOR
        {
            get
            {
                OWOR_DocNum = MyQuery.GetValue("OWOR",OWOR_U_ORDER_JOMAR,"U_ORDER_JOMAR","DocNum").ToInt();                    
                return (OWOR_DocNum > 0);
            }
        }

        public char OWOR_Type;
        public BoProductionOrderTypeEnum OWOR_TypeBo
        {
            get 
            {
                switch(OWOR_Type)
                {
                    case 'S':
                        return BoProductionOrderTypeEnum.bopotStandard;
                    case 'P':
                        return BoProductionOrderTypeEnum.bopotSpecial;
                    case 'D':
                        return BoProductionOrderTypeEnum.bopotDisassembly;
                    default :
                        return BoProductionOrderTypeEnum.bopotStandard;
                }
            }
        }

        public char OWOR_Status;
        public BoProductionOrderStatusEnum OWOR_StatusBo
        {
            get 
            {
                switch(OWOR_Status)
                {
                    case 'P':
                        return BoProductionOrderStatusEnum.boposPlanned;
                    case 'R' :
                        return BoProductionOrderStatusEnum.boposReleased;
                    case 'L':
                        return BoProductionOrderStatusEnum.boposClosed;
                    case 'C':
                        return BoProductionOrderStatusEnum.boposCancelled;
                    default:
                        return BoProductionOrderStatusEnum.boposPlanned;
                }
            }
        }

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
                        throw new Exception("Não foi localizado o item/recurso com o código de barra " + OITM_CodeBars);
                }
            }
        }
        public double OWOR_PlannedQty;
        public string OWOR_WareHouse;
        public int OWOR_DocNum = -1;
        public DateTime OWOR_PostDate;
        public DateTime OWOR_DueDate;
        public int OWOR_OriginNum;
        public string OWOR_CardCode;
        public string OWOR_Project;
        public string OWOR_Remarks;
        private string msg_SAP;

        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'","\"").Trim(); }
        }

        #endregion 
    }

    public class T41DAO
    {
        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <param name="isnew">Buscar somente novas entradas?</param>
        /// <returns></returns>
        public static List<T41Entity> GetDados(bool isnew)
        {
            var transacoes = new List<T41Entity>();

            principal.GravaAudit("MyRecordSet ini");
            using (var conn = new MyRecordSet())
            {
                //if (isnew) // TODO @BFAGUNDES - 

                conn.DoQuery("SELECT distinct (Select Top 1 Docentry from OWOR where U_Order_Jomar = T0.owor_order_jomar and status = 'R') As OWOR_DocNum1, TransactionType, ReadySAP, OWOR_ORDER_JOMAR FROM [dbo].[Sage_Transaction41] T0 WITH (NOLOCK) WHERE [TransactionType] = 41 AND ReadySAP <> 'Y' ");
                //else
                    //conn.DoQuery("SELECT TOP 500 * FROM [dbo].[Sage_Transaction_WO] WITH (NOLOCK) WHERE [TransactionType] = 40 AND OWOR_DocNum > 0  ORDER BY ReadBySAP DESC");
                while (conn.HasNext())
                {
                 
                    var trans = new T41Entity();
                    try
                    {
                        trans.TransType = conn.GetFieldValue("TransactionType").ToInt();
                        trans.ReadBySAP = conn.GetFieldValue("ReadySAP").ToChar();
                        trans.OWOR_U_ORDER_JOMAR = conn.GetFieldValue("OWOR_ORDER_JOMAR").ToString();
                        trans.OWOR_DocNum = conn.GetFieldValue("OWOR_DocNum1").ToInt();
                        //trans.OWOR_WareHouse = conn.GetFieldValue("OWOR_WareHouse").ToString();
                        //trans.OWOR_PostDate = conn.GetFieldValue("OWOR_PostDate").ToDate();
                        //trans.OWOR_DueDate = conn.GetFieldValue("OWOR_DueDate").ToDate();
                        //trans.OWOR_Project = conn.GetFieldValue("OWOR_Project").ToString();
                        //trans.OWOR_Remarks = String.Format("ID {0} - Transação 40. {1}", trans.Id, conn.GetFieldValue("OWOR_Remarks").ToString());

                        transacoes.Add(trans);
                    }
                    catch (Exception ex)
                    {
                        trans.ReadBySAP = 'E';
                        trans.MSG_SAP = ex.Message;

                        UpdateStatus(trans);

                        principal.GravaAudit("Erro empacotar ID " +  trans.Id.ToString() + " " + ex.Message.ToString());
                    }                    
                }
                principal.GravaAudit("Termino While");
            }

            return transacoes;
        }

        /// <summary>
        /// Informa se há transação em aberto
        /// </summary>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        public static bool TransactionOpen()
        {
            try
            {
                using (var rs = new MyRecordSet())
                {
                    rs.DoQuery("SELECT COUNT(*) FROM [Sage_Transaction41] WITH (NOLOCK) WHERE [TransactionType] = 41 AND ReadySAP <> 'Y'");
                    if (rs.HasNext())
                    {
                        if (rs.GetFieldValue(0).ToInt() > 0)
                        {
                            principal.GravaAudit("Achou transação em aberto");
                        }
                        else
                        {
                            principal.GravaAudit("Nenhuma transação em aberto");
                        }

                        return (rs.GetFieldValue(0).ToInt() > 0);
                    }
                    else
                    {
                        principal.GravaAudit("Erro a consultar a tabela  Sage_Transaction41");

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit(ex.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// Atualiza o Status da Transação
        /// </summary>
        /// <param name="oPedido"></param>
        public static void UpdateStatus(List<T41Entity> tentitys)
        {
            using (var rs = new MyRecordSet())
            {
                foreach (var tentity in tentitys)
                {
                    UpdateStatus(tentity);
                }
            }
        }

        /// <summary>
        /// Atualiza o Status do pedido na tabela de transações WO
        /// </summary>
        /// <param name="two"></param>
        public static void UpdateStatus(T41Entity two)
        {
            using (var rs = new MyRecordSet())
            {
                var sql = "UPDATE [Sage_Transaction41] SET ReadySAP = '{0}', OWOR_DocNum = '{1}',[MSG_SBO] = '{3}' WHERE Id = '{2}'";

                principal.GravaAudit(String.Format(sql, two.ReadBySAP, two.OWOR_DocNum, two.Id, two.MSG_SAP.Replace("'", "''")));
                rs.DoQuery(sql, two.ReadBySAP, two.OWOR_DocNum, two.Id, two.MSG_SAP.Replace("'", "''"));
            }
        }

        public static void UpdateStatus41(T41Entity two)
        {
            principal.GravaAudit("Inicio 41");
            using (var rs = new MyRecordSet())
            {
                string sql = "UPDATE [Sage_Transaction41] SET ReadySAP = '" + two.ReadBySAP + "' , ";
                sql += "OWOR_DocNum = '" + two.OWOR_DocNum + "' ";
                sql += "WHERE OWOR_Order_Jomar =  '" + two.OWOR_U_ORDER_JOMAR + "'";

                principal.GravaAudit(sql);
                principal.GravaAudit("Read " + two.ReadBySAP + " Docnum " + two.OWOR_DocNum);
                rs.DoQuery(sql);
            }
            principal.GravaAudit("Termino 41");
        }

        public static void UpdateLineOP(T41Entity two, ProductionOrders prd)
        {
            try
            {
                using (var rs = new MyRecordSet())
                {
                    try
                    {
                        principal.GravaAudit("Inicio remove linhas OP");
                        string sql1 = "delete from WOR1 where docentry = " + two.OWOR_DocNum  + "and ItemType = 4 ";

                        rs.DoQuery(sql1);    
                        principal.GravaAudit("Termino remove linhas OP ");
                        
                    }
                    catch (Exception ex)
                    {
                        principal.GravaAudit("EX " + ex.Message.ToString());
                        principal.GravaAudit("Continua");
                    }

                    string sql = "Select distinct T1.DocEntry, T3.ItemCode, 'PP' As 'ProductionWarehouse', ConsumptionQuantity , T0.OItm_CodeBars, T0.OWor_DocNum from [Sage_Transaction41] T0 ";
                    sql += "left join OWOR T1 on T1.U_ORDER_JOMAR = T0.OWOR_Order_Jomar and T1.Type = 'S'";
                    sql += "left join OITM T3 on T3.CodeBars = T0.OITM_CodeBars ";
                    sql += "left join ITT1 T2 on T2.Code = T3.ItemCode and T2.Type = 4 ";
                    sql += "where T0.OWOR_Order_Jomar = '" + two.OWOR_U_ORDER_JOMAR + "' and readySAP <> 'Y' ";
                
                    principal.GravaAudit("Inicio add linhas OP");
                    principal.GravaAudit(sql);
                    rs.DoQuery(sql);

                    int a = 0;
                    int ii = 0;
                    while (rs.HasNext())
                    {
                        if (prd.GetByKey(rs.GetFieldValue(0).ToInt()))
                        {
                            principal.GravaAudit("Item " + rs.GetFieldValue(1).ToString());
                            principal.GravaAudit("PlannedQuantity " + rs.GetFieldValue(3).ToDouble().ToString());
                            principal.GravaAudit("Warehouse " + rs.GetFieldValue(2).ToString());
                            principal.GravaAudit("U_Line_Jomar " + two.OWOR_U_ORDER_JOMAR);

                            prd.Lines.Add();
                            prd.Lines.ItemNo = rs.GetFieldValue(1).ToString();
                            prd.Lines.ItemType = ProductionItemType.pit_Item;
                            prd.Lines.PlannedQuantity = rs.GetFieldValue(3).ToDouble();
                            //prd.Lines.BaseQuantity = rs.GetFieldValue(0).ToInt();
                            prd.Lines.Warehouse = rs.GetFieldValue(2).ToString();
                            prd.Lines.UserFields.Fields.Item("U_Line_Jomar").Value = two.OWOR_U_ORDER_JOMAR;
                            ii = prd.Update();
                        }
                    }

                    principal.GravaAudit("Termino add linhas OP");
                

                    if(ii != 0)
                    {
                        principal.GravaAudit("Erro " + MyLibs.v2.sbo.SAPConnection.DI.GetLastErrorDescription());
                    }
                    else
                    {
                        two.ReadBySAP = 'Y';
                        UpdateStatus41(two);
                        principal.GravaAudit("Concluido com sucesso");
                    }
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Erro " + ex.Message.ToString());
            }
        }

        public static void UpdateStatusReleased(T41Entity two, ProductionOrders prd)
        {
            principal.GravaAudit("Inicia OP Liberado");
            using (var rs = new MyRecordSet())
            {
                principal.GravaAudit("Pesquisa OP " + two.OWOR_DocNum);
                if (prd.GetByKey(two.OWOR_DocNum))
                {
                    principal.GravaAudit("Acho OP");
                    prd.ProductionOrderStatus = BoProductionOrderStatusEnum.boposReleased;
                }

                int ii = prd.Update();

                if (ii != 0)
                {
                    principal.GravaAudit("Erro " + MyLibs.v2.sbo.SAPConnection.DI.GetLastErrorDescription());
                }
                else
                {
                    principal.GravaAudit("Atualizado para liberado com sucesso");
                }
            }
            principal.GravaAudit("termino OP Liberado");
        }
    }
}
