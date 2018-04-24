using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;
using SAPbobsCOM;
using System;

namespace ROBO.Transacoes
{
    class Transacao38
    {
        public enum Action { IN, OUT };

        public static void Start(Action act)
        {
            switch (act)
            {
                case Action.IN:
                    var t38list = T38DAO.GetDados();
                    if (t38list.Count > 0)
                    {
                        principal.GravaAudit("Count Maior que zero");
                        var t38trans = new Transacao38();
                        foreach (var t38 in t38list)
                        {
                            principal.GravaAudit("Inicio For");
                            //t38trans.LineJomarOWOR(t38);
                            if (t38.OIGE_DocNum < 1)
                            {
                                principal.GravaAudit("Sem OIGE, Jomar: " + t38.JOMAR_OrderNum);
                                t38trans.newOWTR(t38, act);
                            }
                            else
                            {
                                principal.GravaAudit("Com OIGE, Jomar: " + t38.JOMAR_OrderNum);
                            }
                        }

                        t38list.Clear();
                        t38list = null;
                    }
                    else
                    {
                        principal.GravaAudit("Count igual a zero");
                    }
                    break;

                case Action.OUT:
                    var t38nlist = T38NegDAO.GetDados();
                    if (t38nlist.Count > 0)
                    {
                        var t38trans = new Transacao38();
                        foreach (var t38n in t38nlist)
                        {
                            //t38trans.LineJomarOWOR(t38n);
                            t38trans.newOWTR(t38n, act); // t38trans.newOIGE(t38n); (REMOVIDO)
                        }

                        t38nlist.Clear();
                        t38nlist = null;
                    }
                    break;
            }         
            
        }

        /// <summary>
        /// Cria transferencia de estoque.
        /// </summary>
        /// <param name="t38"></param>
        private void newOWTR(T38Entity t38, Action act)
        {
            try
            {
                var oStock = (StockTransfer)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oStockTransfer);

                // HEADER
                oStock.DocDate = t38.AddDate;
                oStock.FromWarehouse = t38.From_WareHouse; // FIX_ME!
                oStock.ToWarehouse = t38.To_WareHouse; // FIX_ME!
                oStock.UserFields.Fields.Item("U_OWOR_JOMAR").Value = t38.JOMAR_OrderNum;
                // LINES
                principal.GravaAudit("ItemCode " + t38.OITM_ItemCode);
                principal.GravaAudit("FromWarehouse " + t38.From_WareHouse);
                principal.GravaAudit("ToWarehouse " + t38.To_WareHouse);
                principal.GravaAudit("Quantity " + t38.IGE1_Quantity.ToString());
                principal.GravaAudit("ID " + t38.Id.ToString());
                principal.GravaAudit("U_OWOR_JOMAR " + t38.JOMAR_OrderNum.ToString());

                oStock.Lines.ItemCode = t38.OITM_ItemCode;
                oStock.Lines.Quantity = t38.IGE1_Quantity;
                oStock.Lines.FromWarehouseCode = t38.From_WareHouse; // FIX_ME!
                oStock.Lines.WarehouseCode = t38.To_WareHouse; // FIX_ME!

                oStock.Lines.Add();

                // FOOTER
                oStock.Comments = String.Format("ID {0} - Transação {1}.", t38.Id, t38.TransactionType);

                int res = oStock.Add();

                if (res != 0)
                {
                    t38.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                    t38.ReadBySAP = 'E';
                    t38.Update_CodeBars = 'N';
                    principal.GravaAudit(t38.MSG_SAP);
                    T38DAO.UpdateStatus(t38);

                    principal.GravaAudit("Docnum: " + t38.OWOR_DocNum.ToString() + " Transtype: " + SAPConnection.DocsEnum.OWOR.ToString() + " Linha: " + t38.JOMAR_LineNum.ToString() + SAPConnection.DI.GetLastErrorDescription().ToString());
                }
                else
                {
                    t38.ReadBySAP = 'Y';
                    t38.Update_CodeBars = 'N';
                    var docnum = SAPConnection.DI.GetNewObjectKey();
                    t38.OIGE_DocNum = int.Parse(docnum);
                    t38.MSG_SAP = "SUCESSO - Transferencia de estoque inserida.";
                    principal.GravaAudit(t38.MSG_SAP);
                    T38DAO.UpdateStatus(t38);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação " + t38.TransactionType.ToString() + " " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// Cria saída dos insumos.
        /// </summary>
        /// <param name="t38"></param>
        private void newOIGE(T38Entity t38)
        {
            try
            {
               
                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGE);

                // HEADER
                // TODO - Saída do insumo, a data é a do dia.
                Inventory.DocDate = t38.AddDate;
                //Inventory.TaxDate = t08.OWOR_PostDate;
                Inventory.BPL_IDAssignedToInvoice = 1;
                //Inventory.Lines.Price = 10;
                Inventory.Lines.ItemCode = "";// t08.OITM_ItemCode;
                Inventory.Lines.Quantity = t38.IGE1_Quantity;
                //Inventory.Lines.WarehouseCode = t38.OWOR_WareHouse;
                //Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = t38.IGN1_U_Lot;
                Inventory.Lines.BaseEntry = t38.OWOR_DocNum;
                Inventory.Lines.BaseType = (int) SAPConnection.DocsEnum.OWOR;
                Inventory.Lines.BaseLine = linenum; //t38.JOMAR_LineNum;

                principal.GravaAudit("Docnum " + t38.OWOR_DocNum.ToString());
                principal.GravaAudit("Linha" + linenum.ToString());

                Inventory.Lines.Add();

                //FOOTER
                Inventory.Comments = String.Format("ID {0} - Transação {1}.", t38.Id, t38.TransactionType);

                int res = Inventory.Add();

                if (res != 0)
                {
                    t38.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                    t38.ReadBySAP = 'E';
                    T38DAO.UpdateStatus(t38);

                    principal.GravaAudit("Docnum: " + t38.OWOR_DocNum.ToString() + " Transtype: " + SAPConnection.DocsEnum.OWOR.ToString() + " Linha: " + t38.JOMAR_LineNum.ToString() + SAPConnection.DI.GetLastErrorDescription().ToString());
                }
                else
                {
                    t38.ReadBySAP = 'Y';
                    var docnum = SAPConnection.DI.GetNewObjectKey();
                    t38.OIGE_DocNum = int.Parse(docnum);
                    t38.MSG_SAP = "SUCESSO - Entrada de produto acabado inserido.";
                    T38DAO.UpdateStatus(t38);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação " + t38.TransactionType.ToString() + " " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// Desmontagem - Entrada dos insumos.
        /// </summary>
        /// <param name="t38"></param>
        private void NewOIGN(T38Entity t38)
        {
            try
            {

                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGN);

                // HEADER
                // TODO - Entrada do insumo, a data é a do dia.
                Inventory.DocDate = t38.AddDate;
                //Inventory.TaxDate = t08.OWOR_PostDate;
                Inventory.BPL_IDAssignedToInvoice = 1;
                //Inventory.Lines.Price = 10;
                Inventory.Lines.ItemCode = "";// t08.OITM_ItemCode;
                Inventory.Lines.Quantity = t38.IGE1_Quantity;
                //Inventory.Lines.WarehouseCode = t38.OWOR_WareHouse;
                //Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = t38.IGN1_U_Lot;
                Inventory.Lines.BaseEntry = t38.OWOR_DocNum;
                Inventory.Lines.BaseType = (int)SAPConnection.DocsEnum.OWOR;
                Inventory.Lines.BaseLine = t38.WOR1_LineNum;
                Inventory.Lines.Add();

                //FOOTER
                Inventory.Comments = String.Format("ID {0} - Transação {1}.", t38.Id, t38.TransactionType);

                int res = Inventory.Add();

                if (res != 0)
                {
                    t38.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                    t38.ReadBySAP = 'E';
                    T38DAO.UpdateStatus(t38);

                    principal.GravaAudit(this.GetType().ToString() + " " + ROBO.Properties.LogMessage.ERRO_003_4.ToString() + " " + t38.TransactionType.ToString() + " " + t38.Id.ToString() + " " + SAPConnection.DI.GetLastErrorDescription().ToString());
                }
                else
                {
                    var docnum = SAPConnection.DI.GetNewObjectKey();
                    t38.OIGE_DocNum = int.Parse(docnum);
                    t38.MSG_SAP = "SUCESSO - Desmontagem e extorno do insumo.";
                    T38DAO.UpdateStatus(t38);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação " + t38.TransactionType.ToString() + " " + ex.Message.ToString());
            }
        }

        public static int linenum;

        /// <summary>
        /// Atualiza a linha OWOR.U_line_Jomar com ITT1.
        /// </summary>
        /// <param name="t08"></param>
        private void LineJomarOWOR(T38Entity t38)
        {
            try
            {
                bool lineexist = false;
                bool itemexist = false;
                string itemcode = null;

                using (var rs = new MyRecordSet())
                {
                    // Pesquisa a Linha Jomar.
                    rs.DoQuery("SELECT ItemCode,LineNum FROM WOR1 WHERE DocEntry = '{0}' AND U_Line_Jomar = '{1}'", t38.OWOR_DocNum, t38.JOMAR_LineNum);
                    
                    if (rs.HasNext())
                    {
                        lineexist = true;
                        linenum = rs.GetFieldValue(1).ToInt();
                        itemcode = rs.GetFieldValue(0).ToString();

                        if (t38.OITM_ItemCode == itemcode)
                            return;

                    }
                    else
                    {
                        // Pesquisa por item.
                        rs.DoQuery("SELECT LineNum FROM WOR1 WHERE DocEntry = '{0}' AND ItemCode = '{1}'", t38.OWOR_DocNum, t38.OITM_ItemCode);

                        if (rs.HasNext())
                        {
                            itemexist = true;
                            linenum = rs.GetFieldValue(0).ToInt();
                        }
                        else 
                        {
                            principal.GravaAudit(t38.Id.ToString() + " LineJomar OWOR Não existe a linha Jomar " + t38.JOMAR_LineNum.ToString() + "  nem o itemCode " + t38.OITM_ItemCode.ToString() + " na OWOR " + t38.OWOR_DocNum.ToString());
                            return;
                        }
                    }
                }


                bool newitem = false;

                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);

                if (oOWOR.GetByKey(t38.OWOR_DocNum))
                {
                    if (lineexist)
                    {
                        oOWOR.Lines.SetCurrentLine(linenum);

                        if (itemcode != t38.OITM_ItemCode)
                        {
                            oOWOR.Lines.ItemNo = t38.OITM_ItemCode;
                            newitem = true;
                        }
                    }
                    else if (itemexist)
                    {
                        oOWOR.Lines.SetCurrentLine(linenum);
                        oOWOR.Lines.UserFields.Fields.Item("U_Line_Jomar").Value = t38.JOMAR_LineNum;
                        t38.Update_CodeBars = 'N';
                    }

                    if (oOWOR.Update() != 0)
                    {
                        t38.ReadBySAP = 'E';
                        t38.Update_CodeBars = 'N';
                        t38.MSG_SAP = String.Format(Properties.LogMessage.Trans38_0037_2, t38.Id, SAPConnection.DI.GetLastErrorDescription());
                        T38DAO.UpdateStatus(t38);

                        principal.GravaAudit("LineJomarOWOR " + t38.MSG_SAP.ToString());
                    } 
                    else
                    {
                        t38.Update_CodeBars = newitem ? 'Y' : 'N';
                        t38.ReadBySAP = 'Y';
                        t38.MSG_SAP = "Linha/Item Jomar atualizado na tela de Ordem de Produção.";
                        T38DAO.UpdateStatus(t38);
                    }
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("LineJomarOWOR " + ex.Message.ToString());
            }                
        }
    }
}
