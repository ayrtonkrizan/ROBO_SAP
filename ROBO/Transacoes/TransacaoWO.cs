using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using SAPbobsCOM;
using System;

namespace ROBO.Transacoes
{
    class TransactionWO
    {
        public static void Start(BoProductionOrderTypeEnum type)
        {
            switch (type)
            {
                case BoProductionOrderTypeEnum.bopotStandard:
                    #region Montagem Padrão
                    try
                    {
                        //while (TWODAO.TransactionOpen())
                        //{
                            // 1º Procura as novas ordem de produção
                            var twolista = TWODAO.GetDados(true);
                            var sapTrans = new TransactionWO();

                            MyLibs.v2.local.Register.Log.Register("Transação WO"
                                        , System.Diagnostics.EventLogEntryType.Information
                                        , "0 - Criando as Ordens de Produção.");

                            foreach (var two in twolista)
                            {
                                if (!two.Exist_OWOR)
                                    sapTrans.OWORWithProductTreeDefault(two);
                                else
                                {
                                    two.MSG_SAP = "Ordem de Produção já criada. Número da Ordem " + two.OWOR_DocNum;
                                    TWODAO.UpdateStatus(two);
                                }
                            }

                            twolista.Clear();
                            twolista = null;
                        //}
                    }
                    catch (Exception ex)
                    {
                        MyLibs.v2.local.Register.Log.Register("Transação WO"
                                , System.Diagnostics.EventLogEntryType.Error
                                , ex.Message);
                    }
                    #endregion
                    break;

                case BoProductionOrderTypeEnum.bopotDisassembly:
                    // 
                    #region Desmontagem
                    try
                    {
                        // 1º Procura as ordens a desmontar
                        var tnwolista = TWONegDAO.GetForOWOR();
                        var sapTrans = new TransactionWO();

                        MyLibs.v2.local.Register.Log.Register("Transação -WO"
                                    , System.Diagnostics.EventLogEntryType.Information
                                    , "0 - Criando as Ordens de Produção (Desmontagem).");

                        foreach (var tnwo in tnwolista)
                        {
                            var valido = false;
                            //if (Properties.Settings.Default.ValidarDesmontagem)
                            //    valido = tnwo.Exist_OWOR;
                            //else
                            //{
                            //    valido = true;
                            //    MyLibs.v2.local.Register.Log.Register("Transação -WO"
                            //        , System.Diagnostics.EventLogEntryType.Warning
                            //        , "A desmontagem não está validando as montagens criadas.");
                            //}

                            if (valido)
                                sapTrans.OWORWithProductTreeDefault(tnwo);
                            else
                            {
                                tnwo.ReadBySAP = 'E';
                                tnwo.MSG_SAP = "Não foi localizada nenhuma ordem de produção no SAP vinculada a ordem de produção Jomar nº " + tnwo.OWOR_U_ORDER_JOMAR + ".";
                                TWODAO.UpdateStatus(tnwo);

                                MyLibs.v2.local.Register.Log.Register("Transação -WO"
                                    , System.Diagnostics.EventLogEntryType.Error
                                    , tnwo.MSG_SAP);
                            }
                        }

                        tnwolista.Clear();
                        tnwolista = null;
                    }
                    catch (Exception ex)
                    {
                        MyLibs.v2.local.Register.Log.Register("Transação WO"
                                , System.Diagnostics.EventLogEntryType.Error
                                , ex.Message);
                    }
                    #endregion
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
                var oOWOR = (SAPbobsCOM.ProductionOrders)SAPConnection.DI.GetBusinessObject(BoObjectTypes.oProductionOrders);
                oOWOR.ItemNo = two.OITM_ItemCode;
                oOWOR.ProductionOrderType = two.OWOR_TypeBo;
                oOWOR.Warehouse = two.OWOR_WareHouse;
                oOWOR.PostingDate = two.OWOR_PostDate;
                oOWOR.DueDate = two.OWOR_DueDate;
                if (two.OWOR_OriginNum > 0)
                    oOWOR.ProductionOrderOriginEntry = two.OWOR_OriginNum;
                oOWOR.ProductionOrderStatus = two.OWOR_StatusBo;
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
                            else
                            {
                                Log.Register("Transação - WO"
                                    , System.Diagnostics.EventLogEntryType.Information
                                    , "Estrutura de Produto {0} : Recurso '{1}' removido da desmontagem."
                                    , two.OITM_ItemCode
                                    , oOITT.Items.ItemCode);
                            }
                        }
                    }
                    else
                    {
                        Log.Register("Transação " + (int)two.TransType
                                    , System.Diagnostics.EventLogEntryType.Information
                                    , "Estrutura de Produto {0} não localizado."
                                    , two.OITM_ItemCode);
                    }
                }

                #region Itens - Não será mais utilizado
                //var t38lista = T38DAO.GetForOWOR(int.Parse(tnwo.OWOR_U_ORDER_JOMAR));
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
                    two.ReadBySAP = 'E';
                    two.MSG_SAP = SAPConnection.DI.GetLastErrorDescription().Replace("'", "\"");
                    TWODAO.UpdateStatus(two);

                    Log.Register("Transação " + (int)two.TransType, System.Diagnostics.EventLogEntryType.Error
                        , "ID {0} - {1}", two.Id, two.MSG_SAP);
                    if (SAPConnection.DI.InTransaction)
                        SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_RollBack);
                }
                else
                {
                    two.OWOR_DocNum = SAPConnection.GetNewKey();
                    two.ReadBySAP = 'Y';
                    two.MSG_SAP = "SUCESSO - Ordem de Produção criado. Número da Ordem: " + SAPConnection.GetNewKey();
                    TWODAO.UpdateStatus(two);

                    // @BFAGUNDES - alterando o Status como 'Liberado'
                    two.OWOR_Status = 'R'; // Release
                    AlterOWORStatus(two);
                }
            }
            catch (Exception ex)
            {
                Log.Register("Transação WO-WO"
                    , System.Diagnostics.EventLogEntryType.Error
                    , "Erro Adicionar OP na ID {0}. {1}"
                    , two.Id
                    , ex.Message);
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

                    Log.Register("AlterOWORStatus"
                        , System.Diagnostics.EventLogEntryType.Error
                        , Properties.LogMessage.TransWO_0038_2
                        , two.Id
                        , two.OWOR_DocNum);

                    return;
                }

                oOWOR.ProductionOrderStatus = two.OWOR_StatusBo;
                
                if (oOWOR.Update() != 0)
                {
                    two.ReadBySAP = 'E';
                    two.MSG_SAP = String.Format(Properties.LogMessage.TransWO_0039_3, two.Id, two.OWOR_DocNum, SAPConnection.DI.GetLastErrorDescription());
                    two.OWOR_Status = 'P';
                    TWODAO.UpdateStatus(two);

                    Log.Register("AlterOWORStatus"
                        , System.Diagnostics.EventLogEntryType.Error
                        , two.MSG_SAP);
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
                Log.Register("Transação WO"
                    , System.Diagnostics.EventLogEntryType.Error
                    , "Erro Atualizar Status OP na ID {0}. {1}"
                    , two.Id
                    , ex.Message);
            }
        }

    }
}
