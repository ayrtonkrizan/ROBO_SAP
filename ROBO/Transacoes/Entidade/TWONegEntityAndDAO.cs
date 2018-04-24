using MyLibs.v2.local.DataBase;
using MyLibs.v2.local.Register;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using System;
using System.Collections.Generic;

namespace ROBO.Transacoes
{
    /// <summary>
    /// Transação -WO - Desmontagem.
    /// </summary>
    public class TWONegEntity
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
        public string OWOR_Comments;
        private string msg_SAP;

        public string MSG_SAP
        {
            get { return msg_SAP; }
            set { msg_SAP = value.Replace("'", "\"").Trim(); }
        }

        public string OIGN_Comments;
        public int OIGN_DocNum = 0;
        public int IGN1_BaseRef;

        #endregion 
    }
    
    public class TWONegDAO
    {
        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <returns></returns>
        public static List<TWOEntity> GetForOWOR()
        {
            var transacoes = new List<TWOEntity>();

            using (var conn = new ConnSqlServer())
            {
                conn.DoQuery(Properties.Querys.Transacao_nWOOWOR_0);
                while (conn.HasNext())
                {
                    var trans = new TWOEntity();

                    try
                    {
                        trans.OWOR_Type = 'D'; // Desmontagem
                        trans.TransType = conn.GetFieldValue("TransactionType").ToInt();                        
                        trans.OWOR_U_ORDER_JOMAR = conn.GetFieldValue("OWOR_ORDER_JOMAR").ToString();
                        trans.OITM_CodeBars = conn.GetFieldValue("OITM_CodeBars").ToString();
                        trans.OWOR_PlannedQty = conn.GetFieldValue("OWOR_PlannedQty").ToDouble();
                        trans.OWOR_WareHouse = conn.GetFieldValue("OWOR_WareHouse").ToString();
                        trans.OWOR_PostDate = DateTime.Now;
                        trans.OWOR_DueDate = DateTime.Now;
                        trans.OWOR_Remarks = "Transação -08 GroupID : " + conn.GetFieldValue("GroupID").ToString();

                        if (MyQuery.Exist("OITT",trans.OITM_ItemCode,"Code"))
                        {
                            if ((Program.contador % 2) == 0)
                            {
                                if (trans.ReadBySAP != 'E')
                                    transacoes.Add(trans);
                            }
                            else
                            {
                                transacoes.Add(trans);
                            }
                        }                            
                        else
                            throw new Exception(String.Format("Não existe estrutura de produto cadastrado para ItemCode \"{0}\".",trans.OITM_ItemCode));
                    }
                    catch (Exception ex)
                    {
                        trans.ReadBySAP = 'E';
                        trans.MSG_SAP = ex.Message;
                        UpdateStatus(trans);

                        principal.GravaAudit("Transação -WO Erro empacotar : " + ex.Message.ToString());
                    }
                }
            }

            
            return transacoes;
        }

        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <returns></returns>
        public static List<TWOEntity> GetTransForOIGE()
        {
            var transacoes = new List<TWOEntity>();

            using (var conn = new ConnSqlServer())
            {
                conn.DoQuery(Properties.Querys.Transacao_nW0OIGE_0);

                while (conn.HasNext())
                {
                    var trans = new TWOEntity();

                    try
                    {
                        trans.OWOR_Type = 'D'; // Desmontagem
                        trans.TransType = conn.GetFieldValue("TransactionType").ToInt();
                        trans.OWOR_U_ORDER_JOMAR = conn.GetFieldValue("OWOR_ORDER_JOMAR").ToString();
                        trans.OITM_CodeBars = conn.GetFieldValue("OITM_CodeBars").ToString();
                        //trans.OWOR_PlannedQty = conn.GetFieldValue("OWOR_PlannedQty").ToDouble();
                        trans.OWOR_WareHouse = conn.GetFieldValue("OWOR_WareHouse").ToString();
                        trans.OWOR_PostDate = DateTime.Now;
                        trans.OWOR_DueDate = DateTime.Now;
                        trans.OWOR_DocNum = conn.GetFieldValue("OWOR_DocNum").ToInt();
                        //trans.OIGE_DocNum = conn.GetFieldValue("OIGE_DocNum").ToInt();
                        trans.OWOR_Remarks = "Transação -WO GroupID : " + conn.GetFieldValue("GroupID").ToString();

                        if (MyQuery.Exist("OITT", trans.OITM_ItemCode, "Code"))
                        {
                            if ((Program.contador % 2) == 0)
                            {
                                if (trans.ReadBySAP != 'E')
                                    transacoes.Add(trans);
                            }
                            else
                            {
                                transacoes.Add(trans);
                            }
                        }  
                        else
                            throw new Exception(String.Format("0 - Não existe estrutura de produto cadastrado para ItemCode \"{0}\".", trans.OITM_ItemCode));
                    }
                    catch (Exception ex)
                    {
                        trans.ReadBySAP = 'E';
                        trans.MSG_SAP = ex.Message;
                        UpdateStatus(trans);

                        principal.GravaAudit("Transação -WO Erro empacotar ID " + trans.Id.ToString() + " " + ex.Message.ToString());
                    }
                }
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
            using (var rs = new MyRecordSet())
            {
                rs.DoQuery("SELECT COUNT(*) FROM [Sage_Transaction_WO] WITH (NOLOCK) WHERE [TransactionType] = -8 AND ReadBySAP <> 'Y'");

                if (rs.HasNext())
                {
                    if (rs.GetFieldValue(0).ToInt() > 0)
                    {
                        principal.GravaAudit("Transação -WO - Encontrado {0} transações não processadas " + rs.GetFieldValue(0).ToString());
                    }
                    else
                    {
                        principal.GravaAudit("Transação -WO - Nenhuma transação em aberto.");
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
        public static void UpdateStatus(List<TWOEntity> tentitys)
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
        public static void UpdateStatus(TWOEntity two)
        {
            using (var rs = new MyRecordSet())
            {
                principal.GravaAudit("two.Id " + two.Id.ToString());
                principal.GravaAudit("two.OWOR_U_ORDER_JOMAR " + two.OWOR_U_ORDER_JOMAR.ToString());
                if (two.Id == 0)
                { 
                    string sql = "UPDATE   [Sage_Transaction_WO]  SET ReadBySAP = '" + two.ReadBySAP + "' , ";
                           sql += "OWOR_DocNum = '" + two.OWOR_DocNum + "' ";
                           sql += ",[MSG_SBO] = '" + two.MSG_SAP.Replace("'", "''") + "' ";
                           sql += "WHERE OWOR_Order_Jomar =  '" + two.OWOR_U_ORDER_JOMAR + "'";

//                    var sql = @"
//                            UPDATE   [Sage_Transaction_WO] 
//                            SET      ReadBySAP = 'Y'
//                                    ,OWOR_DocNum = '{1}'
//                                    ,[MSG_SBO] = '{3}'
//                            WHERE (CAST(TransactionType as nvarchar(100)) 
//		                            +	CAST(OWOR_ORDER_JOMAR as nvarchar(100)) 
//		                            +	CAST(OITM_CodeBars AS nvarchar(20))
//		                            +	CAST(OWOR_DocNum AS nvarchar(20))
//		                            +	OWOR_WareHouse) = '{2}'";

                    principal.GravaAudit(sql);
                    principal.GravaAudit("Read " + two.ReadBySAP + " Docnum " + two.OWOR_DocNum + " MSG " + two.MSG_SAP.Replace("'", "''"));
                    rs.DoQuery(sql);
                }
                else
                {
                    principal.GravaAudit("Update Status 5");
                    string sql = "UPDATE   [Sage_Transaction_WO]  SET ReadBySAP = '" + two.ReadBySAP + "', ";
                            sql += "OWOR_DocNum = '" + two.OWOR_DocNum + "' ";
                            sql += ",[MSG_SBO] = '" + two.MSG_SAP.Replace("'", "''") + "' ";
                            sql += "Id = '" + two.Id + "'";

//                    var sql = @"
//                        UPDATE   [Sage_Transaction_WO] 
//                        SET      ReadBySAP = '{0}'
//                                ,OWOR_DocNum = '{1}'
//                                ,[MSG_SBO] = '{3}'
//                        WHERE Id = {2}";
                    rs.DoQuery(sql, two.ReadBySAP, two.OWOR_DocNum, two.Id, two.MSG_SAP.Replace("'", "''"));
                }
            }
        }

    }
}
