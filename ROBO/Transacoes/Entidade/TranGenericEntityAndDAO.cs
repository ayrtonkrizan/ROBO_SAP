using System;
using System.Collections.Generic;
using System.Text;
using MyLibs.v2.local.Register;
using MyLibs.v2.sbo.DI;
using MyLibs.v2.sbo;
using System.Windows.Forms;
using SAPbobsCOM;

namespace ROBO.Transacoes {
    public class TransGenericEntity
    {
        #region Propriedades
        private int id;
        private int transType;        
        private DateTime docDate;
        private DateTime taxDate;
        private int groupNum;
        private int bplId;
        private string comments;
        private List<TransEntityLine> lines = new List<TransEntityLine>();
        
        #endregion

        #region GET/SET
        /// <summary>
        /// Id
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// Linhas dos itens
        /// </summary>
        internal List<TransEntityLine> Lines
        {
            get { return lines; }
            set { lines = value; }
        }
        /// <summary>
        /// Código da transação
        /// </summary>
        public int TransType
        {
            get { return transType; }
            set { transType = value; }
        }
        /// <summary>
        /// Número do documento.
        /// Caso inserido, a propriedade ReadBySAP será mudada para true;
        /// </summary>
        public int DocNum
        {
            get { return lines[0].DocEntry; }
            set 
            { 
                if( value > 0)
                {
                    foreach (var line in lines)
                    {
                        line.ReadBySAP = true;
                        line.DocEntry = value;
                    }
                }
            }
        }
        /// <summary>
        /// Data de lançamento
        /// </summary>
        public DateTime DocDate
        {
            get { return docDate; }
            set { docDate = value; }
        }
        /// <summary>
        /// Data de vencimento
        /// </summary>
        public DateTime TaxDate
        {
            get { return taxDate; }
            set { taxDate = value; }
        }
        /// <summary>
        /// Lista de preço
        /// </summary>
        public int GroupNum
        {
            get { return groupNum; }
            set { groupNum = value; }
        }
        /// <summary>
        /// Filial
        /// </summary>
        public int BplId
        {
            get { return bplId; }
            set { bplId = value; }
        }
        /// <summary>
        /// Comentários
        /// </summary>
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        #endregion
    }

    public class TransEntityLine
    {
        #region Propriedades
        private int id;
        int docEntry;
        private bool readBySAP;
        private string codeBars;
        private string itemCode;
        private string u_Quality;
        private int quantity;
        private int warehouse;
        private string u_Lote;
        #endregion

        #region GET/SET
        public int DocEntry
        {
            get { return docEntry; }
            set { docEntry = value; }
        }
        public bool ReadBySAP
        {
            get { return readBySAP; }
            set { readBySAP = value; }
        }
        /// <summary>
        /// Identificador da linha
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// Número do Lote
        /// </summary>
        public string U_Lote
        {
            get { return u_Lote; }
            set { u_Lote = value; }
        }     
        /// <summary>
        /// Código do item.
        /// Para setar utilize a propriedade CodeBars
        /// </summary>
        public string ItemCode
        {
            get { return itemCode; }
        }
        /// <summary>
        /// Código de barra de itens
        /// </summary>
        public string CodeBars
        {
            get { return codeBars; }
            set
            {
                codeBars = value;

                using (var rs = new MyRecordSet())
                {
                    rs.DoQuery("SELECT ItemCode FROM OITM Where CodeBars = '{0}'", codeBars);

                    if (rs.HasNext())
                        itemCode = rs.GetFieldValue(0).ToString();
                    else
                        throw new LogException("CodeBars",System.Diagnostics.EventLogEntryType.Error, Properties.LogMessage.ERRO0001_1, codeBars);
                }

            }
        }
        /// <summary>
        /// Campo do usuário "Qualidade"
        /// </summary>
        public string U_Quality
        {
            get { return u_Quality; }
            set { u_Quality = value; }
        }
        /// <summary>
        /// Quantidade
        /// </summary>
        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
        /// <summary>
        /// Depósito
        /// </summary>
        public int Warehouse
        {
            get { return warehouse; }
            set { warehouse = value; }
        }
        #endregion
    }
    
    public class TransGenericDAO
    {
        /// <summary>
        /// Cria documento com apenas 1 linha por documento.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<ROBO.Transacoes.Entidade.Documents> GetDadosSL(TransEnum type)
        {
            var oDocEntitys = new List<Transacoes.Entidade.Documents>();
            using (var conn = new MyRecordSet())
            {
                principal.GravaAudit(Properties.Querys.Transacoes_1);
                principal.GravaAudit(type.ToString());
                conn.DoQuery(Properties.Querys.Transacoes_1, ((int)type));
                
                while (conn.HasNext())
                {
                    var oDocEntity = new Transacoes.Entidade.Documents();

                    try
                    {
                        oDocEntity = new Transacoes.Entidade.Documents();
                        oDocEntity.MyValues["Id"] = conn.GetFieldValue(0);
                        oDocEntity.BplId = conn.GetFieldValue("BplId").ToInt();
                        oDocEntity.MyValues["ReadBySAP"] = conn.GetFieldValue("ReadBySAP");
                        oDocEntity.Comments = conn.GetFieldValue("Comments").ToString();
                        oDocEntity.DocDate = conn.GetFieldValue("DocDate").ToDate();
                        oDocEntity.GroupNum = conn.GetFieldValue("GroupNum").ToInt();
                        oDocEntity.TaxDate = conn.GetFieldValue("TaxDate").ToDate();

                        // Linhas
                        var oDocentryLine = new Transacoes.Entidade.DocumentsEntityLine();
                        oDocentryLine.UserFields["U_Lot"] = conn.GetFieldValue("U_Lot").ToString();
                        oDocentryLine.UserFields["U_Quality"] = conn.GetFieldValue("U_Quality").ToString();
                        oDocentryLine.WhsCode = conn.GetFieldValue("WareHouse").ToString();
                        oDocentryLine.Quantity = conn.GetFieldValue("Quantity").ToDouble();
                        oDocentryLine.ItemCode = Functions.Find.Item(conn.GetFieldValue("OITM_CodeBars").ToString());

                        oDocEntity.Lines.Add(oDocentryLine);

                        // Adicionando na lista
                        if ((Program.contador % 2) == 0)
                        {
                            if(oDocEntity.MyValues["ReadBySAP"].ToChar() != 'E')
                                oDocEntitys.Add(oDocEntity);
                        }
                        else
                        {
                            oDocEntitys.Add(oDocEntity);
                        }

                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(oDocEntity, 'E', String.Format(Properties.LogMessage.TRANS_0030_2L
                            , oDocEntity.MyValues["Id"]
                            , ex.Message));
                    }
                }
            }

            return oDocEntitys;
        }



        /// <summary>
        /// Retorna as entidades de transações não lida pelo SAP.
        /// </summary>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        public static List<Transacoes.Entidade.Documents> GetDados(TransEnum type)
        {            
            var oDocEntitys = new List<Transacoes.Entidade.Documents>();

            var conn = new MyLibs.v2.local.DataBase.ConnSqlServer();

            conn.DoQuery(Properties.Querys.Transacoes_1, (int)type);

            var oDocEntity = new Transacoes.Entidade.Documents();
            int bplId = -1;

            try
            {
                while (conn.HasNext())
                {                    
                    var error = false;

                    try
                    {


                if (bplId != conn.GetFieldValue("BplId").ToInt())
                {
                    bplId = conn.GetFieldValue("BplId").ToInt();
                    oDocEntity.BplId = bplId;
                    oDocEntity.MyValues["Id"] = conn.GetFieldValue("Id");

                    oDocEntity.Comments = conn.GetFieldValue("Comments").ToString();
                    oDocEntity.DocDate = conn.GetFieldValue("DocDate").ToDate();
                    oDocEntity.GroupNum = conn.GetFieldValue("GroupNum").ToInt();
                    oDocEntity.TaxDate = conn.GetFieldValue("TaxDate").ToDate();

                    var oDocentryLine = new Transacoes.Entidade.DocumentsEntityLine();

                    oDocentryLine.Quantity = conn.GetFieldValue("Quantity").ToDouble();

                    var codebars = conn.GetFieldValue("OITM_CodeBars").ToString();
                    try
                    {
                        oDocentryLine.ItemCode = MyQuery.GetValue("OITM", codebars, "CodeBars", "ItemCode").ToString();
                    }
                    catch
                    {
                        error = true;
                        principal.GravaAudit("Transação " + type.ToString() + " " + Properties.LogMessage.ERRO_TRANS_28_2.ToString() + " " + codebars.ToString());

                        UpdateStatus(oDocEntity,'E',String.Format(Properties.LogMessage.ERRO_TRANS_28_2
                            , codebars
                            , oDocEntity.MyValues["Id"]));
                           
                        oDocentryLine.ItemCode = null;
                    }

                    oDocentryLine.UserFields["U_Lot"] = conn.GetFieldValue("U_Lot").ToString();
                    oDocentryLine.UserFields["U_Quality"] = conn.GetFieldValue("U_Quality").ToString();
                    oDocentryLine.WhsCode = conn.GetFieldValue("WareHouse").ToString();

                    oDocentryLine.MyValues["Id"] = conn.GetFieldValue("Id");

                    oDocEntity.Lines.Add(oDocentryLine);
                }
                else
                {
                    var oDocentryLine = new Transacoes.Entidade.DocumentsEntityLine();

                    oDocentryLine.MyValues["Id"] = conn.GetFieldValue("Id");
                    oDocentryLine.Quantity = conn.GetFieldValue("Quantity").ToDouble();
                    var codebars = conn.GetFieldValue("OITM_CodeBars").ToString();

                    try
                    {
                        oDocentryLine.ItemCode = MyQuery.GetValue("OITM", codebars, "CodeBars", "ItemCode").ToString();
                    }
                    catch
                    {
                        error = true;
                        principal.GravaAudit("Transação " + type.ToString() + " " + Properties.LogMessage.ERRO_TRANS_28_2.ToString() + " " + codebars.ToString());

                        UpdateStatus(oDocEntity, 'E', String.Format(Properties.LogMessage.ERRO_TRANS_28_2
                            , codebars
                            , oDocEntity.MyValues["Id"].ToInt()));

                        oDocentryLine.ItemCode = null;
                    }

                    oDocentryLine.UserFields["U_Lot"] = conn.GetFieldValue("U_Lot").ToString();
                    oDocentryLine.UserFields["U_Quality"] = conn.GetFieldValue("U_Quality").ToString();
                    oDocentryLine.WhsCode = conn.GetFieldValue("WareHouse").ToString();

                    oDocEntity.Lines.Add(oDocentryLine);
                    
                    }
                }catch(Exception ex)
                {
                    error = true;
                    UpdateStatus(oDocEntity, 'E', ex.Message);
                }

                    if(!error)
                        oDocEntitys.Add(oDocEntity);

                    oDocEntity = new Transacoes.Entidade.Documents();
                }
            } 
            catch(Exception ex)
            {
                principal.GravaAudit("Transação " + type.ToString() + " " + ex.Message.ToString());
            }

            return oDocEntitys;
        }

        /// <summary>
        /// Informa se há transação em aberto
        /// </summary>
        /// <param name="type">Tipo de transação</param>
        /// <returns></returns>
        public static bool TransactionOpen(TransEnum type)
        {              
            using(var rs = new MyRecordSet())
            {
                rs.DoQuery("SELECT COUNT(*) FROM sage_TRANSACOES WITH (NOLOCK) WHERE ReadBySAP <> 'Y' AND CAST(TransactionType AS INT) = '{0}'", (int)type);

                if (rs.HasNext())
                {
                    return (rs.GetFieldValue(0).ToInt() > 0);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Atualiza o Status da Transação
        /// </summary>
        /// <param name="oPedido"></param>
        public static void UpdateStatus(Transacoes.Entidade.Documents oDocEntity,char readBySAP = 'Y',string msg = "")
        {
            using(var rs = new MyRecordSet())
            {
                var sql = String.Format("UPDATE [Sage_Transacoes] SET ReadBySAP = '{0}', DocNum = '{1}', SAP_MSG = '{2}' WHERE Id = '{3}'"
                                , readBySAP, oDocEntity.DocNum, msg.Replace("'", "''"), oDocEntity.MyValues["Id"]);
                rs.DoQuery(sql);


                foreach(var linha in oDocEntity.Lines)
                {
                    sql = String.Format("UPDATE [Sage_Transacoes] SET ReadBySAP = '{0}', DocNum = '{1}', SAP_MSG = '{2}' WHERE Id = '{3}'"
                            , readBySAP, oDocEntity.DocNum, msg.Replace("'","''"), oDocEntity.MyValues["Id"]);
                    
                    rs.DoQuery(sql);
                }                
            }
        }
    }
}
