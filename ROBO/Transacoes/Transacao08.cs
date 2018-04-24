using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using SAPbobsCOM;
using System;

namespace ROBO.Transacoes
{
    class Transaction08
    {
        public static void Start(BoProductionOrderTypeEnum type)
        {
            switch (type)
            {
                case BoProductionOrderTypeEnum.bopotStandard:
                    #region Montagem Padrão
                    try
                    {
                        //while (T08DAO.TransactionOpen())
                        //{
                            // 1º Procura as novas ordem de produção (REMOVIDO)
                            //var t08lista = T08DAO.GetDados(true);
                            var sapTrans = new Transaction08();

                            // 2º Registrando a entrada no produto.
                            //t08lista.Clear();
                            //t08lista = null;
                            principal.GravaAudit("Inicio Getdados");
                            var t08lista = T08DAO.GetDados(false);


                            foreach (var t08 in t08lista)
                            {
                                sapTrans.NewOIGN(t08);
                            }

                            t08lista.Clear();
                            t08lista = null;
                        //}
                    }
                    catch (Exception ex)
                    {
                        principal.GravaAudit("Transação 8 " + ex.Message);
                    }
                    #endregion
                    break;

                case BoProductionOrderTypeEnum.bopotDisassembly:
                    // 
                    #region Desmontagem
                    try
                    {
                        principal.GravaAudit("Jomar -8 desmontagem ");

                        var tn08lista = T08NegDAO.GetForOWOR();
                        var sapTrans = new Transaction08();

                        

                        //// 2º Registrando a saída do insumo.
                        //tn08lista.Clear();
                        //tn08lista = null;
                        //tn08lista = T08NegDAO.GetTransForOIGE();


                        foreach (var tn08 in tn08lista)
                        {
                            principal.GravaAudit("new oige");
                            sapTrans.NewOIGE(tn08);
                        }

                        tn08lista.Clear();
                        tn08lista = null;
                    }
                    catch (Exception ex)
                    {
                        principal.GravaAudit("Transação 8 " + ex.Message.ToString());
                    }
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// Criação de uma nova Orderm de Produção com os
        /// componente da estrutura do produto.
        /// </summary>
        /// <param name="tn08"></param>
        private void OWORWithProductTreeDefault(T08Entity t08)
        {
            try
            {
                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                oOWOR.ItemNo = t08.OITM_ItemCode;
                oOWOR.ProductionOrderType = t08.OWOR_TypeBo;
                oOWOR.Warehouse = t08.OWOR_WareHouse;
                oOWOR.PostingDate = t08.OWOR_PostDate;
                oOWOR.DueDate = t08.OWOR_DueDate;
                if (t08.OWOR_OriginNum > 0)
                    oOWOR.ProductionOrderOriginEntry = t08.OWOR_OriginNum;
                oOWOR.ProductionOrderStatus = t08.OWOR_StatusBo;
                oOWOR.Project = t08.OWOR_Project;
                oOWOR.PlannedQuantity = t08.OWOR_PlannedQty < 0 ? -t08.OWOR_PlannedQty : t08.OWOR_PlannedQty;
                oOWOR.UserFields.Fields.Item("U_ORDER_JOMAR").Value = t08.OWOR_U_ORDER_JOMAR;
                oOWOR.Remarks = t08.OWOR_Remarks;

                // Desmontagem - Os itens deverão ser informado manualmente.
                if (oOWOR.ProductionOrderType == BoProductionOrderTypeEnum.bopotDisassembly)
                {
                    var oOITT = (SAPbobsCOM.ProductTrees)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductTrees);

                    if (oOITT.GetByKey(t08.OITM_ItemCode))
                    {
                        for (int i = 0; i < (oOITT.Items.Count - 1); i++)
                        {
                            oOITT.Items.SetCurrentLine(i);

                            // Desmontagem - Não é permitido inserir recurso.
                            if (oOITT.Items.ItemType == ProductionItemType.pit_Item)
                            {
                                oOWOR.Lines.ItemNo = oOITT.Items.ItemCode;
                                oOWOR.Lines.ItemType = oOITT.Items.ItemType;
                                oOWOR.Lines.PlannedQuantity = oOITT.Items.Quantity * (t08.OWOR_PlannedQty < 0 ? -t08.OWOR_PlannedQty : t08.OWOR_PlannedQty);
                                oOWOR.Lines.BaseQuantity = oOITT.Items.Quantity;
                                oOWOR.Lines.Warehouse = oOITT.Items.Warehouse;
                                oOWOR.Lines.UserFields.Fields.Item("U_Line_Jomar").Value = t08.OWOR_U_ORDER_JOMAR;
                                oOWOR.Lines.Add();
                            }
                            else
                            {
                                principal.GravaAudit("Transação -8 - Estrutura de Produto {0} : Recurso '{1}' removido da desmontagem." + t08.OITM_ItemCode.ToString() + oOITT.Items.ItemCode.ToString());
                            }
                        }
                    }
                    else
                    {
                        principal.GravaAudit("Transação " + t08.TransType.ToString() +  " Estrutura de Produto {0} não localizado." + t08.OITM_ItemCode.ToString());
                    }
                }

                #region Itens - Não será mais utilizado
                //var t38lista = T38DAO.GetForOWOR(int.Parse(tn08.OWOR_U_ORDER_JOMAR));
                //int pos = 0;
                //foreach (var t38 in t38lista)
                //{
                //    if (oODRF.ProductionOrderType != BoProductionOrderTypeEnum.bopotSpecial)
                //        oODRF.ProductionOrderType = BoProductionOrderTypeEnum.bopotSpecial;
                //    var a = oODRF.Lines.Count;
                //    oODRF.Lines.SetCurrentLine(pos++);
                //    oODRF.Lines.ItemNo = t38.OITM_ItemCode;
                //    oODRF.Lines.ItemType = t38.ItemTypeBo;
                //    oODRF.Lines.PlannedQuantity = t38.WOR1_PlannedQty;
                //    oODRF.Lines.BaseQuantity = t38.WOR1_BaseQty;
                //    oODRF.Lines.Warehouse = t38.WOR1_WareHouse;
                //    oODRF.Lines.UserFields.Fields.Item("U_Line_Jomar").Value = t38.JOMAR_LineNum;
                //    oODRF.Lines.Add();
                //}
                #endregion

                if (oOWOR.Add() != 0)
                {
                    t08.ReadBySAP = 'E';
                    t08.GroupID = 0;
                    t08.MSG_SAP = SAPConnection.DI.GetLastErrorDescription().Replace("'","\"");
                    T08DAO.UpdateStatus(t08);

                    principal.GravaAudit("Transação " + t08.TransType.ToString() + " " + t08.Id.ToString(), t08.MSG_SAP.ToString());
                    if (SAPConnection.DI.InTransaction)
                        SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_RollBack);
                }
                else
                {
                    t08.OWOR_DocNum = SAPConnection.GetNewKey();
                    t08.ReadBySAP = 'Y';
                    t08.MSG_SAP = "SUCESSO - Ordem de Produção criado com o grupo " + t08.GroupID;
                    T08DAO.UpdateStatus(t08);

                    // @BFAGUNDES - alterando o Status como 'Liberado'
                    t08.OWOR_Status = 'R'; // Release
                    AlterOWORStatus(t08);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação 8 - Erro Adicionar OP na ID " +  t08.Id.ToString() + " " + ex.Message,ToString());
            }
        }

        /// <summary>
        /// Alterar o Status do Pedido.
        /// </summary>
        /// <param name="tn08"></param>
        private void AlterOWORStatus(T08Entity t08)
        {
            try
            {
                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                if(!oOWOR.GetByKey(t08.OWOR_DocNum))
                {
                    t08.ReadBySAP = 'E';
                    t08.MSG_SAP = String.Format(Properties.LogMessage.Trans08_0034_2,t08.Id,t08.OWOR_DocNum);
                    t08.OWOR_Status = 'P';
                    T08DAO.UpdateStatus(t08);

                    principal.GravaAudit("AlterOWORStatus " + Properties.LogMessage.Trans08_0034_2.ToString() + " " + t08.Id.ToString() + t08.OWOR_DocNum.ToString());

                    return;
                }

                oOWOR.ProductionOrderStatus = t08.OWOR_StatusBo;
                
                if (oOWOR.Update() != 0)
                {
                    t08.ReadBySAP = 'E';
                    t08.MSG_SAP = String.Format(Properties.LogMessage.Trans08_0035_3, t08.Id, t08.OWOR_DocNum, SAPConnection.DI.GetLastErrorDescription());
                    t08.OWOR_Status = 'P';
                    T08DAO.UpdateStatus(t08);


                    principal.GravaAudit("AlterOWORStatus " + t08.MSG_SAP.ToString());
                }
                else
                {
                    t08.ReadBySAP = 'Y';
                    t08.MSG_SAP = "SUCESSO - Ordem de Produção liberada";
                    T08DAO.UpdateStatus(t08);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação 8 - Erro Atualizar Status OP na ID " + t08.Id.ToString() + " " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// Cria nova Entrada de Produtos acabados.
        /// </summary>
        /// <param name="t08"></param>
        private void NewOIGN(T08Entity t08)
        {
             try
            {
               
                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGN);

                // HEADER
                Inventory.DocDate = t08.OWOR_PostDate;
                //Inventory.TaxDate = tn08.OWOR_PostDate;
                Inventory.BPL_IDAssignedToInvoice = 1;
                //Inventory.Lines.Price = 10;
                //Inventory.Lines.ItemCode = tn08.OITM_ItemCode;
                Inventory.Lines.Quantity = t08.IGN1_Quantity;
                principal.GravaAudit(t08.IGN1_WhsCode);
                principal.GravaAudit(t08.OWOR_DocNum.ToString());
                Inventory.Lines.WarehouseCode = t08.IGN1_WhsCode;
                Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = t08.IGN1_U_Lot;
                Inventory.Lines.BaseEntry = t08.OWOR_DocNum;
                Inventory.Lines.BaseType = (int) SAPConnection.DocsEnum.OWOR;
                Inventory.Lines.Add();

                //FOOTER
                Inventory.Comments = t08.OIGN_Comments;

                int res = Inventory.Add();

                if (res != 0)
                {
                    t08.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                    t08.ReadBySAP = 'E';
                    principal.GravaAudit(t08.MSG_SAP);
                    T08DAO.UpdateStatus(t08);
                    
                    principal.GravaAudit(SAPConnection.DI.GetLastErrorDescription().ToString());
                }
                else
                {
                    var docnum = SAPConnection.DI.GetNewObjectKey();
                    t08.OIGN_DocNum = int.Parse(docnum);
                    t08.ReadBySAP = 'Y';
                    t08.MSG_SAP = "SUCESSO - Entrada de produto acabado inserido.";
                    principal.GravaAudit(t08.MSG_SAP);
                    T08DAO.UpdateStatus(t08);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação " + t08.TransType.ToString() + " " + ex.Message);
            }
        }

        /// <summary>
        /// Cria saída de Produtos acabados.
        /// </summary>
        /// <param name="t08"></param>
        private void NewOIGE(T08Entity t08)
        {
            try
            {

                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGE);

                // HEADER
                Inventory.DocDate = t08.OWOR_PostDate;
                //Inventory.TaxDate = tn08.OWOR_PostDate;
                Inventory.BPL_IDAssignedToInvoice = 1;
                //Inventory.Lines.Price = 10;
                Inventory.Lines.ItemCode = "";// tn08.OITM_ItemCode;
                Inventory.Lines.Quantity = t08.IGN1_Quantity;
                Inventory.Lines.WarehouseCode = t08.OWOR_WareHouse;
                //Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = t08.IGN1_U_Lot;
                Inventory.Lines.BaseEntry = t08.OWOR_DocNum;
                Inventory.Lines.BaseType = (int)SAPConnection.DocsEnum.OWOR;
                Inventory.Lines.Add();

                //FOOTER
                Inventory.Comments = t08.OWOR_Remarks;

                int res = Inventory.Add();

                if (res != 0)
                {
                    t08.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                    t08.ReadBySAP = 'E';
                    T08DAO.UpdateStatus(t08);

                    principal.GravaAudit(SAPConnection.DI.GetLastErrorDescription().ToString());
                }
                else
                {
                    var docnum = SAPConnection.DI.GetNewObjectKey();

                    if (int.Parse(docnum) == 0) // Estava dando erro ao recuperar o DocEntry
                        t08.OIGE_DocNum = int.Parse(docnum);
                    else
                        t08.OIGE_DocNum = 999999; // Somente para verificar se houver falha. 

                    t08.ReadBySAP = 'Y';
                    t08.MSG_SAP = "SUCESSO - Saída do produto efetuada.";
                    T08NegDAO.UpdateStatus(t08);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit("Transação " + t08.TransType.ToString() + " " + ex.Message.ToString());
            }
        }
        
    }
}
