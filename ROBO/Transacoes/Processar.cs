using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;
using MyLibs.v2.sbo.Register;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ROBO.Transacoes
{
    public class Processar
    {
        private TransEnum transType;
        /// <summary>
        /// Tipo de transação
        /// </summary>
        public TransEnum TransType
        {
            get { return transType; }
        }

        /// <summary>
        /// Informe o número da transação.
        /// </summary>
        /// <param name="transtype">Transação ID</param>
        public Processar(TransEnum type)
        {
            this.transType = type;
        }

        /// <summary>
        /// Faz o cadastro das transações no SAP e Registra na tabela como status de OK
        /// </summary>
        public void Registrar()
        {
            #region DEBUG
            #if DEBUG
            //if (Properties.Debug.Default.ExecOnlyTrans != 0)
            //{
            //    transType = (TransEnum)Enum.Parse(typeof(TransEnum), Properties.Debug.Default.ExecOnlyTrans.ToString());
            //    MyLibs.v2.local.Register.Log.Register("ATENÇÃO"
            //            , System.Diagnostics.EventLogEntryType.Warning
            //            , "Serviço está limitado para executar apenas a transação nº {0}"
            //            , Properties.Debug.Default.ExecOnlyTrans);
            //}
            #endif
            #endregion

            principal.GravaAudit("Iniciando o processo. " + transType.ToString());

            MessageBox.Show(transType.ToString());
            switch (transType)
            {
                //case TransEnum.T05AumentoValorItem:
                //    this.Trans05(); break;

                case TransEnum.T06EntradaLocalizacaoPeca:
                    this.Trans06(); break;

                //case TransEnum.T08CriacaoNovaPeca:
                //    this.Trans08(); break;

                //case TransEnum.T08CancelamentoNovaPeca:
                //    this.TransNeg08(); break;

                //case TransEnum.T32PedidoDeVenda:
                //    this.Trans32(); break;

                //case TransEnum.T35ReducaoValorItem:
                //    this.Trans35(); break;

                //case TransEnum.T36SaidaLocalizacaoPeca:
                //    this.Trans36(); break;

                //case TransEnum.T38ComsumirMateiral:
                //    this.Trans38(); break;

                //case TransEnum.T38DevolucaoSobra:
                //    this.TransNeg38(); break;

                //case TransEnum.T98AjustFiesicoEstoque:
                //    this.Trans98(); break;

                //case TransEnum.TWOCriacaoOrdemProdMont:
                //    this.TransWO(); break;
                
                //case TransEnum.T41AlteraEstruturaOP:
                //    this.Trans41(); break;

                //case TransEnum.TWOCriacaoOrdemProdDesm:
                //    this.TransNegWO(); break;
            }
        }

        /// <summary>
        /// Transação 05 - Aumenta do valor de um item
        /// </summary>
        private void Trans05()
        {
            var transacoes = TransGenericDAO.GetDadosSL(transType);
            foreach (var transacao in transacoes)
            {

                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGN);      
                // HEADER
                Inventory.DocDate = transacao.DocDate;
                Inventory.TaxDate = transacao.TaxDate;
                Inventory.BPL_IDAssignedToInvoice = transacao.BplId;                

                // LINES
                foreach (var line in transacao.Lines)
                {                    
                    Inventory.Lines.ItemCode = line.ItemCode;
                    Inventory.Lines.Quantity = line.Quantity;
                    Inventory.Lines.WarehouseCode = line.WhsCode;
                    Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = line.UserFields["U_Lot"];
                    Inventory.Lines.UserFields.Fields.Item("U_Quality").Value = line.UserFields["U_Quality"];

                    using(var rs = new MyRecordSet())
                    {
                        rs.DoQuery(Properties.Querys.CustoItem_2, line.ItemCode, line.WhsCode);
                        if(rs.HasNext())
                            Inventory.Lines.Price = rs.GetFieldValue(0).ToDouble();
                        else
                            Inventory.Lines.Price = 0;
                    }

                    Inventory.Lines.Add();
                }

                //FOOTER
                Inventory.Comments = String.Format("ID {0} - Transação {1}. {2}", transacao.MyValues["Id"].ToInt(), (int)transType, transacao.Comments);
                int res = Inventory.Add();

                if (res != 0)
                {
                    principal.GravaAudit(this.GetType().Name.ToString() + SAPConnection.DI.GetLastErrorDescription().ToString());
                    TransGenericDAO.UpdateStatus(transacao, 'E', SAPConnection.DI.GetLastErrorDescription());
                }
                else
                {
                    transacao.DocNum = int.Parse(MyLibs.v2.sbo.SAPConnection.DI.GetNewObjectKey());
                    TransGenericDAO.UpdateStatus(transacao);
                }
            }

            transacoes.Clear();
            transacoes = null;
        }

        /// <summary>
        /// Transação 06 - Transferência de localização de Peça (Table OIGN in SBO).
        /// </summary>
        private void Trans06()
        {
            MessageBox.Show("TRans06");
            var transacoes = TransGenericDAO.GetDadosSL(transType);
            foreach (var transacao in transacoes)
            {
                MessageBox.Show("Achou GetDados");
                var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGN);
                // HEADER
                Inventory.DocDate = transacao.DocDate;
                Inventory.TaxDate = transacao.TaxDate;
                Inventory.BPL_IDAssignedToInvoice = transacao.BplId;

                MessageBox.Show("GroupNUM " + Inventory.GroupNumber.ToString());

                // LINES
                foreach (var line in transacao.Lines)
                {
                    Inventory.Lines.ItemCode = line.ItemCode;
                    Inventory.Lines.Quantity = line.Quantity;
                    Inventory.Lines.WarehouseCode = line.WhsCode;
                    Inventory.Lines.UserFields.Fields.Item("U_Lot").Value = line.UserFields["U_Lot"];
                    Inventory.Lines.UserFields.Fields.Item("U_Quality").Value = line.UserFields["U_Quality"];

                    MessageBox.Show("ItemCode " + line.ItemCode.ToString());
                    MessageBox.Show("Quantity " + line.Quantity.ToString());
                    MessageBox.Show("WarehouseCode " + line.WhsCode.ToString());
                    
                    using (var rs = new MyRecordSet())
                    {
                        rs.DoQuery(Properties.Querys.CustoItem_2, line.ItemCode, line.WhsCode);
                        if (rs.HasNext())
                        {
                            MessageBox.Show("Price " + rs.GetFieldValue(0).ToDouble());
                            Inventory.Lines.UnitPrice = rs.GetFieldValue(0).ToDouble();
                        }
                        else
                        {
                            Inventory.Lines.UnitPrice = 0;
                            MessageBox.Show("Price 0");
                        }
                    }

                    Inventory.Lines.Add();
                }

                //FOOTER
                Inventory.Comments = String.Format("ID {0} - Transação {1}. {2}", transacao.MyValues["Id"].ToInt(), (int)transType, transacao.Comments);
                int res = Inventory.Add();

                MessageBox.Show("Add " + res.ToString());
                MessageBox.Show("MSG " + this.GetType().Name.ToString() + SAPConnection.DI.GetLastErrorDescription().ToString());

                if (res != 0)
                {
                    principal.GravaAudit(this.GetType().Name.ToString() + SAPConnection.DI.GetLastErrorDescription().ToString());
                    TransGenericDAO.UpdateStatus(transacao, 'E', SAPConnection.DI.GetLastErrorDescription());
                }
                else
                {
                    transacao.DocNum = int.Parse(MyLibs.v2.sbo.SAPConnection.DI.GetNewObjectKey());
                    TransGenericDAO.UpdateStatus(transacao);
                }
            }

            transacoes.Clear();
            transacoes = null;
        }

        /// <summary>
        /// Transação 08 - Criação de uma nova Peça (OWOR).
        /// </summary>
        private void Trans08()
        {            
            try
            {
                #region Estrutura do Produto
                if (WorkTree.HaveNewWorkTree())
                    WorkTree.AddNewWorkTree();
                #endregion
                // Tipo Padrão
                Transaction08.Start(BoProductionOrderTypeEnum.bopotStandard);
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }

        /// <summary>
        /// Transação -8 Desmontagem de item.
        /// </summary>
        private void TransNeg08()
        {
            try
            {
                // Tipo Desmontagem
                Transaction08.Start(BoProductionOrderTypeEnum.bopotDisassembly);
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }   

        /// <summary>
        /// Transacao 32 - Pedido de Venda (Table ORDR in SBO)
        /// </summary>
        private void Trans32()
        {
            try
            {
                Transaction32.Start();
            }
            catch(Exception ex)
            {
                principal.GravaAudit("Transação " + transType.ToString() + " " + ex.Message.ToString());
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }

        /// <summary>
        /// Transação 35 - Redução do valor de um item (Table OIGE in SBO).
        /// </summary>
        private void Trans35()
        {
            var transacoes = TransGenericDAO.GetDadosSL(transType);

            if (transacoes.Count == 0)
            {
                principal.GravaAudit(transType.ToString() + this.GetType().ToString() + ROBO.Properties.LogMessage.TRANS_i0002_G1.ToString());
                return;
            }

            var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGE);

            foreach (var transacao in transacoes)
            {
                try
                {
                    // HEADER
                    Inventory.DocDate = transacao.DocDate;
                    Inventory.TaxDate = transacao.TaxDate;
                    Inventory.BPL_IDAssignedToInvoice = transacao.BplId;

                    // LINES
                    foreach (var line in transacao.Lines)
                    {
                        Inventory.Lines.ItemCode = line.ItemCode;
                        Inventory.Lines.Quantity = line.Quantity;
                        Inventory.Lines.WarehouseCode = line.WhsCode;
                        Inventory.Lines.Add();
                    }

                    //FOOTER
                    Inventory.Comments = String.Format("ID {0} - Transação {1}. {2}", transacao.MyValues["Id"].ToInt(), (int)transType, transacao.Comments);

                    int res = Inventory.Add();

                    if (res != 0)
                    {
                        TransGenericDAO.UpdateStatus(transacao, 'E', SAPConnection.DI.GetLastErrorDescription());

                        var array = new List<string>();                        
                        foreach(var line in transacao.Lines)
                        {
                            array.Add(line.ItemCode);
                        }

                        throw new LogException(this.GetType()
                                , System.Diagnostics.EventLogEntryType.Error
                                , ROBO.Properties.LogMessage.ERRO_003_4
                                , (int)transType
                                , transacao.MyValues["Id"]
                                , SAPConnection.DI.GetLastErrorDescription()
                                , String.Join(", ",array.ToArray()));
                    }
                    else
                    {
                        var docnum = SAPConnection.DI.GetNewObjectKey();
                        transacao.DocNum = int.Parse(docnum);                        
                        TransGenericDAO.UpdateStatus(transacao);
                    }
                }
                catch (Exception ex)
                {
                    principal.GravaAudit("Transação " + transType.ToString() + " " + ex.Message.ToString());
                }
            }

            transacoes.Clear();
            transacoes = null;
        }

        /// <summary>
        /// Transação 36 - Transferencia de localização de pecas  (Table OIGE in SBO).
        /// </summary>
        private void Trans36()
        {
            var transacoes = TransGenericDAO.GetDadosSL(transType);

            if (transacoes.Count == 0)
            {
                principal.GravaAudit(transType.ToString() + this.GetType().ToString() + ROBO.Properties.LogMessage.TRANS_i0002_G1.ToString());
                return;
            }

            foreach (var transacao in transacoes)
            {
                try
                {
                    var Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGE);

                    // HEADER
                    Inventory.DocDate = transacao.DocDate;
                    Inventory.TaxDate = transacao.TaxDate;
                    Inventory.BPL_IDAssignedToInvoice = transacao.BplId;

                    // LINES
                    foreach (var line in transacao.Lines)
                    {
                        //Inventory.Lines.Price = 10;
                        Inventory.Lines.ItemCode = line.ItemCode;
                        Inventory.Lines.Quantity = line.Quantity;
                        Inventory.Lines.WarehouseCode = line.WhsCode;
                        Inventory.Lines.Add();
                    }

                    //FOOTER
                    Inventory.Comments = String.Format("ID {0} - Transação {1}. {2}", transacao.MyValues["Id"].ToInt(), (int)transType, transacao.Comments);

                    int res = Inventory.Add();

                    if (res != 0)
                    {
                        var error = SAPConnection.DI.GetLastErrorDescription();
                        TransGenericDAO.UpdateStatus(transacao, 'E', SAPConnection.DI.GetLastErrorDescription());

                        var array = new List<string>();
                        foreach (var line in transacao.Lines)
                        {
                            array.Add(line.ItemCode);
                        }

                        throw new LogException(this.GetType()
                                , System.Diagnostics.EventLogEntryType.Error
                                , ROBO.Properties.LogMessage.ERRO_003_4
                                , (int)transType
                                , transacao.MyValues["Id"]
                                , SAPConnection.DI.GetLastErrorDescription()
                                , String.Join(", ", array.ToArray()));
                    }
                    else
                    {
                        var docnum = SAPConnection.DI.GetNewObjectKey();
                        transacao.DocNum = int.Parse(docnum);
                        TransGenericDAO.UpdateStatus(transacao);
                    }
                }
                catch (Exception ex)
                {
                    principal.GravaAudit("Transação " + transType.ToString() + " " + ex.Message.ToString());
                }
            }

            transacoes.Clear();
            transacoes = null;
        }

        /// <summary>
        /// Transação 38 - Ordem de Produção (Table OWOR and OITT in SBO).
        /// </summary>
        private void Trans38()
        {
            Transacao38.Start(Transacao38.Action.IN);
        }

        /// <summary>
        /// Transação -38 Devolução de sobra.
        /// </summary>
        private void TransNeg38()
        {
            Transacao38.Start(Transacao38.Action.OUT);
        }

        /// <summary>
        /// Transação WO - Criação de Ordem de Produção (Montagem)
        /// </summary>
        private void TransWO()
        {
            try
            {
                // Tipo Padrão
                principal.GravaAudit("Start");
                TransactionWO.Start(BoProductionOrderTypeEnum.bopotStandard);
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }

        private void Trans41()
        {
            try
            {
                // Tipo Padrão
                principal.GravaAudit("Start");
                Transacao41.Start(BoProductionOrderTypeEnum.bopotStandard);
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }

        /// <summary>
        /// Transação -WO Criação de Ordem de Produção (Desmontagem)
        /// </summary>
        private void TransNegWO()
        {
            try
            {
                // Tipo Desmontagem
                TransactionWO.Start(BoProductionOrderTypeEnum.bopotDisassembly);
            }
            finally
            {
                if (SAPConnection.DI.InTransaction)
                    SAPConnection.DI.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }   

        /// <summary>
        /// Transação 98 - Ajuste de Estoque.
        /// Se a coluna quantidade for negativa, trata-se de uma saída de mercadoria,
        /// caso contrário será considerada uma entrada.
        /// </summary>
        private void Trans98()
        {
            var transacoes = TransGenericDAO.GetDadosSL(transType);

            if (transacoes.Count == 0)
            {
                principal.GravaAudit("Transação " + transType.ToString() + " " + ROBO.Properties.LogMessage.TRANS_i0002_G1.ToString());
                return;
            }

            foreach (var transacao in transacoes)
            {
                try
                {
                    Documents Inventory = null;

                    if (transacao.Lines[0].Quantity > 0)
                        Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGN); //Entrada
                    else if (transacao.Lines[0].Quantity < 0)
                        Inventory = SAPConnection.GetDocument(SAPConnection.DocsEnum.OIGE); // saida
                    else
                        TransGenericDAO.UpdateStatus(transacao, 'E', Properties.LogMessage.Trans_0031_0);

                    if (Inventory != null)
                    {
                        // HEADER
                        Inventory.DocDate = transacao.DocDate;
                        Inventory.TaxDate = transacao.TaxDate;
                        Inventory.BPL_IDAssignedToInvoice = transacao.BplId;

                        // LINES
                        foreach (var line in transacao.Lines)
                        {
                            Inventory.Lines.ItemCode = line.ItemCode;
                            Inventory.Lines.Quantity = line.Quantity < 0 ? -line.Quantity : line.Quantity;
                            Inventory.Lines.WarehouseCode = line.WhsCode;
                            Inventory.Lines.Add();
                        }
                        //FOOTER
                        Inventory.Comments = String.Format("ID {0} - Transação {1}. {2}", transacao.MyValues["Id"].ToInt(), (int)transType, transacao.Comments);

                        int res = Inventory.Add();

                        if (res != 0)
                        {
                            var erro = SAPConnection.DI.GetLastErrorDescription();
                            TransGenericDAO.UpdateStatus(transacao, 'E', SAPConnection.DI.GetLastErrorDescription());

                            var array = new List<string>();
                            foreach (var line in transacao.Lines)
                            {
                                array.Add(line.ItemCode);
                            }

                            throw new LogException(this.GetType()
                                    , System.Diagnostics.EventLogEntryType.Error
                                    , ROBO.Properties.LogMessage.ERRO_003_4
                                    , (int)transType
                                    , transacao.MyValues["Id"]
                                    , SAPConnection.DI.GetLastErrorDescription()
                                    , String.Join(", ", array.ToArray()));
                        }
                        else
                        {
                            var docnum = SAPConnection.DI.GetNewObjectKey();
                            transacao.DocNum = int.Parse(docnum);
                            TransGenericDAO.UpdateStatus(transacao);
                        }
                    }
                }
                catch (Exception ex)
                {
                    principal.GravaAudit("Transação " + transType.ToString() + " " + ex.Message.ToString());
                }
            }

            transacoes.Clear();
            transacoes = null;
        }        

        /// <summary>
        /// Cria ou atualiza a estrutura de produto, baseada na tabela ProductTree.
        /// </summary>
        private bool ProductTree(string itemCode)
        {
            
            var prodTree = ProductTreeDAO.GetDados(itemCode);
            if (prodTree == null)
            {
                principal.GravaAudit(this.GetType().ToString() + " Não existe nenhuma estrutura para ItemCode {0} na tabela [ProductTree] " + itemCode.ToString());
                return true;
            }


            var oOITT = (SAPbobsCOM.ProductTrees)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductTrees);
            bool is_update;

            if (MyQuery.Exist("OITT", itemCode, "Code"))
            {
                is_update = true;
                if(!oOITT.GetByKey(itemCode))
                {
                    throw new LogException(this.GetType()
                        ,System.Diagnostics.EventLogEntryType.Error
                        , Properties.LogMessage.ProductTree_e0021_1
                        , itemCode);
                }
            } 
            else
            {
                is_update = false;
                oOITT.TreeCode = prodTree.OITT_Code;
            }
                
            oOITT.PriceList = prodTree.OITT_PriceList;
            oOITT.Project = prodTree.OITT_Project;
            oOITT.Quantity = prodTree.OITT_Quantity;
            oOITT.TreeType = prodTree.OITT_TreeTypeBo;
            oOITT.Warehouse = prodTree.OITT_ToWH;

            foreach (var line in prodTree.Lines)
            {
                oOITT.Items.ItemCode = line.ITT1_Code;
                oOITT.Items.ItemType = line.ITT1_TypeBo;
                oOITT.Items.Quantity = line.ITT1_Quantity;
                oOITT.Items.Warehouse = line.ITT1_WareHouse;
                oOITT.Items.IssueMethod = line.ITT1_IssuedMthdBo;
                oOITT.Items.PriceList = prodTree.OITT_PriceList;
                oOITT.Items.Price = line.ITT1_Price;
                oOITT.Items.Comment = line.ITT1_Comment;
                oOITT.Items.Add();
            }

            if(is_update)
            {
                if(oOITT.Update() != 0)
                {
                    prodTree.ReadBySAP = 'E';
                    ProductTreeDAO.UpdateStatus(prodTree);

                    throw new SAPException("OITT"
                        , Properties.LogMessage.ProductTree_e0022_2, "atualização", itemCode);

                    return false;
                }
            }
            else
            {
                if (oOITT.Add() != 0)
                {
                    prodTree.ReadBySAP = 'E';
                    ProductTreeDAO.UpdateStatus(prodTree);

                    throw new SAPException("OITT"
                        , Properties.LogMessage.ProductTree_e0022_2, "cadastro", itemCode);

                    return false;
                }
            }

            prodTree.ReadBySAP = 'Y';
            ProductTreeDAO.UpdateStatus(prodTree);

            prodTree = null;

            return true;
        }
        
    }
}