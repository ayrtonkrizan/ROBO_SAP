using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;
using MyLibs.v2.sbo.Register;
using SAPbobsCOM;
using System;
using System.Collections.Generic;

namespace ROBO.Transacoes
{
    class Transaction32
    {
        public static void Start()
        {
            var sapTrans = new Transaction32();

            #region Pedido de Venda
            var t32list = T32DAO.GetNewORDR();
            

            try
            {
                foreach (var t32 in t32list)
                {
                    // Criando o pedido de venda.
                    if (t32.ORDR_DocEntry < 1)
                        sapTrans.NewORDR(t32);

                    if(t32.ORDR_DocEntry > 0)
                        sapTrans.NewLineORDR(t32);
                }


            }
            finally
            {
                t32list.Clear();
                t32list = null;
            }
            #endregion

            #region Nota Fiscal
            t32list = T32DAO.GetNewOINV();


            try
            {
                foreach (var t32 in t32list)
                {
                    sapTrans.NewOINV(t32);
                }
            }
            finally
            {
                t32list.Clear();
                t32list = null;
            }
            #endregion
        }


        /// <summary>
        /// Cria Pedido de Vendo com item genérico.
        /// </summary>
        /// <param name="t32"></param>
        private void NewORDR(T32Entity t32)
        {
            if(!MyQuery.Exist("OITM",Properties.Settings.Default.ItemCodeDefault,"ItemCode"))
            {
                return;
            }

            var oORDR = SAPConnection.GetDocument(SAPConnection.DocsEnum.ORDR);

            oORDR.CardCode = t32.JOMAR_CardCode;
            oORDR.NumAtCard = t32.ORDR_NumAtCard;
            oORDR.BPL_IDAssignedToInvoice = t32.ORDR_BplId;

            oORDR.Series = -1;
            oORDR.HandWritten = BoYesNoEnum.tYES;
            oORDR.DocNum = t32.JOMAR_OrderNum;
            oORDR.Reference1 = t32.JOMAR_OrderNum.ToString();
            oORDR.DocDate = t32.ORDR_DocDate < (new DateTime(2016, 1, 1)) ? (new DateTime(2016, 1, 1)) : t32.ORDR_DocDate;
            oORDR.DocDueDate = DateTime.Now;
            oORDR.TaxDate = t32.ORDR_TaxDate < (new DateTime(2016, 1, 1)) ? (new DateTime(2016, 1, 1)) : t32.ORDR_TaxDate;

            oORDR.Lines.ItemCode = Properties.Settings.Default.ItemCodeDefault;
            oORDR.Lines.Quantity = 1;
            oORDR.Lines.Price = 0;
            oORDR.Lines.WarehouseCode = "EM";
            oORDR.Lines.Add();
            
            oORDR.SalesPersonCode = t32.JOMAR_OIM;           


            if (oORDR.Add() != 0)
            {                
                t32.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                t32.ReadBySAP = 'E';
                t32.ORDR_DocEntry = MyQuery.GetValue("ORDR", t32.JOMAR_OrderNum, "DocNum", "DocEntry").ToInt();
                t32.ReadBySAP = t32.ORDR_DocEntry > 0 ? 'Y' : 'E';
                T32DAO.UpdateStatus(t32);

                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
            else
            {
                t32.ORDR_DocEntry = SAPConnection.GetNewKey();
                t32.ReadBySAP = 'Y';
                t32.MSG_SAP = "SUCESSO - Pedido de Venda  criado.";
                T32DAO.UpdateStatus(t32);
            }
        }
        /// <summary>
        /// Atualiza com novas linhas o pedido de venda
        /// </summary>
        /// <param name="t32"></param>
        private void NewLineORDR(T32Entity t32)
        {
            
            foreach (var line in t32.Lines)
            {
                var oORDR = SAPConnection.GetDocument(SAPConnection.DocsEnum.ORDR);

                if (oORDR.GetByKey(t32.ORDR_DocEntry))
                {
                    oORDR.Lines.Add();
                    line.RDR1_LineNum = oORDR.Lines.Count - 1;
                    oORDR.Lines.SetCurrentLine(line.RDR1_LineNum);
                    ////////////////////////////////////
                    oORDR.Lines.ItemCode = line.OITM_ItemCode;
                    oORDR.Lines.Quantity = line.RDR1_Quantity;
                    //oODRF.Lines.Price = ;
                    oORDR.Lines.WarehouseCode = line.RDR1_WhsCode;
                    oORDR.Lines.UserFields.Fields.Item("U_PesoPeca").Value = line.JOMAR_Peso;
                    oORDR.Lines.Add();               
 
                    if(oORDR.Update() != 0)
                    {
                        line.ReadBySAP = 'E';                        
                        line.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                        T32DAO.UpdateStatus(t32);

                        principal.GravaAudit("Transação 32 " + line.MSG_SAP.ToString());
                    }
                    else
                    {
                        line.ReadBySAP = 'Y';
                        line.ORDR_DocEntry = t32.ORDR_DocEntry;
                        line.MSG_SAP = "SUCESSO - Linha cadastrada";
                        T32DAO.UpdateStatus(t32);
                    }

                }
                else
                {
                    line.ReadBySAP = 'E';
                    line.MSG_SAP = "Não foi localizado nenhum pedido de venda.";
                    T32DAO.UpdateStatus(t32);
                }

            }
        }

        /// <summary>
        /// Cria um esboço de Nota Fiscal.
        /// </summary>
        /// <param name="t32"></param>
        private void NewOINV(T32Entity t32)
        {       

            var oODRF = SAPConnection.GetDocument(SAPConnection.DocsEnum.ODRF);

            oODRF.DocObjectCode = BoObjectTypes.oInvoices;
            oODRF.CardCode = t32.JOMAR_CardCode;
            //oODRF.NumAtCard = t32.ORDR_NumAtCard;
            oODRF.HandWritten = BoYesNoEnum.tNO;
            oODRF.BPL_IDAssignedToInvoice = t32.ORDR_BplId;
            
            //oODRF.Reference1 = t32.JOMAR_OrderNum.ToString();
            oODRF.DocDate = DateTime.Now;
            oODRF.DocDueDate = DateTime.Now;
            oODRF.TaxDate = DateTime.Now;

            foreach(var line in t32.Lines)
            {
                oODRF.Lines.ItemCode = line.OITM_ItemCode;
                oODRF.Lines.Quantity = line.RDR1_Quantity;
                oODRF.Lines.WarehouseCode = line.RDR1_WhsCode;
                oODRF.Lines.BaseType = 17;
                oODRF.Lines.BaseEntry = t32.ORDR_DocEntry;//rs.GetFieldValue("DocNum").ToInt();
                oODRF.Lines.BaseLine = line.RDR1_LineNum;
                oODRF.Lines.Add();
            }
            //using (var rs = new MyRecordSet())
            //{
            //    rs.DoQuery(Properties.Querys.ItensAberto_RDR1_1, t32.ORDR_DocEntry);
            //    while (rs.HasNext())
            //    {
            //        ////////////////////////////////////
            //        oODRF.Lines.ItemCode = rs.GetFieldValue("ItemCode").ToString();
            //        oODRF.Lines.Quantity = rs.GetFieldValue("diff").ToDouble() < rs.GetFieldValue("Quantity").ToDouble() ;
            //        //oODRF.Lines.Price = rs.GetFieldValue("Price").ToDouble();
            //        //oODRF.Lines.WarehouseCode = rs.GetFieldValue("WhsCode").ToString();
            //        oODRF.Lines.BaseType = rs.GetFieldValue("ObjType").ToInt();
            //        oODRF.Lines.BaseEntry = t32.ORDR_DocEntry;//rs.GetFieldValue("DocNum").ToInt();
            //        oODRF.Lines.BaseLine = rs.GetFieldValue("LineNum").ToInt();
            //        oODRF.Lines.Add();
            //    }
            //}

            oODRF.SalesPersonCode = t32.JOMAR_OIM;            

            if (oODRF.Add() != 0)
            {
                t32.ReadBySAP = 'E';
                t32.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();

                foreach (var line in t32.Lines)
                {
                    line.ReadBySAP = 'E';

                    using(var rs = new MyRecordSet())
                    {
                        rs.DoQuery("SELECT TrgetEntry FROM RDR1 WHERE TargetType = '13' AND LineNum = '{0}' AND DocEntry = '{1}'", line.Jomar_LineNum, t32.ORDR_DocEntry);
                            if(rs.HasNext())
                            {
                                line.OINV_DocEntry = rs.GetFieldValue("TrgetEntry").ToInt();
                                line.ReadBySAP = 'Y';
                            }
                            else
                                line.OINV_DocEntry = 0;
                    }

                    line.OINV_DocEntry = 0;
                    line.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                }

                T32DAO.UpdateStatus(t32, true);

                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
            else
            {
                var oinv = SAPConnection.GetNewKey();
                var msg = "SUCESSO - Nota Fiscal Criada com sucesso.";
                foreach(var line in t32.Lines)
                {
                    line.ReadBySAP = 'Y';
                    line.OINV_DocEntry = oinv;
                    line.MSG_SAP = msg;
                }
                
                T32DAO.UpdateStatus(t32 , true);
            }
        }
    }


}
