using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using SAPbobsCOM;
using System;

namespace ROBO.Transacoes
{
    class Transacao41
    {
        public static void Start(BoProductionOrderTypeEnum type)
        {
            switch (type)
            {
                case BoProductionOrderTypeEnum.bopotStandard:
                    try
                    {
                            //Procura Ordem de produção
                            var twolista = T41DAO.GetDados(true);

                            principal.GravaAudit("Alterando Linha Ordens de Produção.");

                            foreach (var two in twolista)
                            {
                                var prd = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                                T41DAO.UpdateLineOP(two, prd);
                            }

                            twolista.Clear();
                            twolista = null;
                        //}
                    }
                    catch (Exception ex)
                    {
                        principal.GravaAudit("Transação 41 " + ex.Message.ToString());
                    }
                    break;
            }
        }

        /// <summary>
        /// Criação de uma nova Ordem de Produção com os
        /// componentes da estrutura do produto.
        /// </summary>
        /// <param name="tnwo"></param>
        private void OWORWithProductTreeDefault(TWOEntity two)
        {
            try
            {
                principal.GravaAudit("Inicio AddOP");
                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                oOWOR.ItemNo = two.OITM_ItemCode;
                oOWOR.ProductionOrderType = two.OWOR_TypeBo;
                oOWOR.Warehouse = two.OWOR_WareHouse;
                oOWOR.PostingDate = two.OWOR_PostDate;
                oOWOR.DueDate = two.OWOR_DueDate;
                if (two.OWOR_OriginNum > 0)
                    oOWOR.ProductionOrderOriginEntry = two.OWOR_OriginNum;
                oOWOR.ProductionOrderStatus = BoProductionOrderStatusEnum.boposPlanned;
                oOWOR.Project = two.OWOR_Project;
                oOWOR.PlannedQuantity = two.OWOR_PlannedQty < 0 ? -two.OWOR_PlannedQty : two.OWOR_PlannedQty;
                oOWOR.UserFields.Fields.Item("U_ORDER_JOMAR").Value = two.OWOR_U_ORDER_JOMAR;
                oOWOR.Remarks = two.OWOR_Remarks;

                // Desmontagem - Os itens deverão ser informado manualmente.
                if (oOWOR.ProductionOrderType == BoProductionOrderTypeEnum.bopotDisassembly)
                {
                    var oOITT = (SAPbobsCOM.ProductTrees)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductTrees);

                    if (oOITT.GetByKey(two.OITM_ItemCode))
                    {
                        for (int i = 0; i < (oOITT.Items.Count - 1); i++)
                        {
                            oOITT.Items.SetCurrentLine(i);

                            // Desmontagem - Não é permitido inserir recurso.
                            if (oOITT.Items.ItemType == ProductionItemType.pit_Item)
                            {
                                oOWOR.Lines.ItemNo = oOITT.Items.ItemCode;
                                oOWOR.Lines.ItemType = oOITT.Items.ItemType;
                                oOWOR.Lines.PlannedQuantity = oOITT.Items.Quantity * (two.OWOR_PlannedQty < 0 ? -two.OWOR_PlannedQty : two.OWOR_PlannedQty);
                                oOWOR.Lines.BaseQuantity = oOITT.Items.Quantity;
                                oOWOR.Lines.Warehouse = "PP";//oOITT.Items.Warehouse;
                                oOWOR.Lines.UserFields.Fields.Item("U_Line_Jomar").Value = two.OWOR_U_ORDER_JOMAR;
                                oOWOR.Lines.Add();
                            }
                        }
                    }
                }

                if (oOWOR.Add() != 0)
                {
                    two.ReadBySAP = 'E';
                    two.MSG_SAP = SAPConnection.DI.GetLastErrorDescription().Replace("'", "\"");
                    principal.GravaAudit(two.MSG_SAP);
                    TWODAO.UpdateStatus(two);

                    if (SAPConnection.DI.InTransaction)
                        SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_RollBack);
                }
                else
                {
                    two.OWOR_DocNum = SAPConnection.GetNewKey();
                    two.ReadBySAP = 'Y';
                    two.MSG_SAP = "SUCESSO - Ordem de Produção criado. Número da Ordem: " + SAPConnection.GetNewKey();
                    principal.GravaAudit(two.MSG_SAP);
                    TWODAO.UpdateStatusReleased(two, oOWOR);
                    TWODAO.UpdateLineOP(two, oOWOR);
                    TWODAO.UpdateStatus(two);

                    // @BFAGUNDES - alterando o Status como 'Liberado'
                    two.OWOR_Status = 'R'; // Release
                    AlterOWORStatus(two);
                }
                principal.GravaAudit("Termino AddOP");
            }
            catch (Exception ex)
            {
                principal.GravaAudit(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Alterar o Status do Pedido.
        /// </summary>
        /// <param name="two"></param>
        private void AlterOWORStatus(TWOEntity two)
        {
            try
            {
                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                if (!oOWOR.GetByKey(two.OWOR_DocNum))
                {
                    two.ReadBySAP = 'E';
                    two.MSG_SAP = String.Format(Properties.LogMessage.TransWO_0038_2, two.Id, two.OWOR_DocNum);
                    two.OWOR_Status = 'P';
                    TWODAO.UpdateStatus(two);

                    return;
                }

                oOWOR.ProductionOrderStatus = two.OWOR_StatusBo;
                
                if (oOWOR.Update() != 0)
                {
                    two.ReadBySAP = 'E';
                    two.MSG_SAP = String.Format(Properties.LogMessage.TransWO_0039_3, two.Id, two.OWOR_DocNum, SAPConnection.DI.GetLastErrorDescription());
                    two.OWOR_Status = 'P';
                    TWODAO.UpdateStatus(two);
                }
                else
                {
                    two.ReadBySAP = 'Y';
                    two.MSG_SAP = "SUCESSO - Ordem de Produção liberada";
                    TWODAO.UpdateStatus(two);
                }
            }
            catch (Exception ex)
            {
                principal.GravaAudit(ex.Message.ToString());
            }
        }
    }
}
