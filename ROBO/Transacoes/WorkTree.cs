using MyLibs.v2.local.Register;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;
using MyLibs.v2.sbo.Register;
using ROBO.Transacoes.Entidade;
using System;
using System.Collections.Generic;
using System.Text;

namespace ROBO.Transacoes
{
    public class WorkTree
    {
        /// <summary>
        /// Verifica se há Estrutura de produto não cadastrada
        /// </summary>
        /// <returns>Boolean</returns>
        public static bool HaveNewWorkTree()
        {
            using (var rs = new MyRecordSet())
            {
                rs.DoQuery("SELECT COUNT(*) FROM [Sage_WorkTree] WITH (NOLOCK) WHERE ReadBySAP <> 'Y' ");
                if (rs.HasNext())
                {
                    if (rs.GetFieldValue(0).ToInt() > 0)
                    {
                        principal.GravaAudit("T08 WorkTree Encontrado {0} transações não processadas "  + rs.GetFieldValue(0).ToString());
                    }
                    else
                    {
                        principal.GravaAudit("T08 WorkTree - Nenhuma transação em aberto.");
                    }

                    return (rs.GetFieldValue(0).ToInt() > 0);
                }
                else
                {
                    principal.GravaAudit("WorkTree - Erro a consultar a tabela  Sage_Transacoes.");

                    return false;
                }
            }
        }

        /// <summary>
        /// Registra as novas Estruturas no SBO.
        /// </summary>
        public static void AddNewWorkTree()
        {
            var wteList = Entidade.WorkTreeDAO.GetDados();

            foreach(var wte in wteList)
            {
                try
                {
                    var oOITT = (SAPbobsCOM.ProductTrees)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductTrees);
                    var exist = oOITT.GetByKey(wte.OITT_Code);

                    if (!exist)
                    {
                        oOITT = (SAPbobsCOM.ProductTrees)SAPConnection.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductTrees);
                        oOITT.TreeCode = wte.OITT_Code;
                        oOITT.Quantity = wte.OITT_Quantity;
                        oOITT.Warehouse = wte.OITT_ToWH;
                        oOITT.PriceList = wte.OITT_PriceList;
                        oOITT.TreeType = SAPbobsCOM.BoItemTreeTypes.iProductionTree;

                    }
                    else
                    {
                        for (int i = oOITT.Items.Count; i > 0; )
                        {
                            oOITT.Items.SetCurrentLine(--i);
                            oOITT.Items.Delete();
                        }
                    }

                    foreach (var line in wte.Lines)
                    {
                        oOITT.Items.ItemType = line.ITT1_TypeBo;
                        oOITT.Items.ItemCode = line.ITT1_ItemCode;
                        oOITT.Items.Quantity = line.ITT1_Quantity;
                        oOITT.Items.PriceList = wte.OITT_PriceList;
                        if(line.ITT1_WareHouse == "0")
                            oOITT.Items.Warehouse = wte.OITT_ToWH;
                        else
                            oOITT.Items.Warehouse = line.ITT1_WareHouse;

                        oOITT.Items.IssueMethod = line.ITT1_IssueMthdBo;

                        oOITT.Items.Add();
                    }
                   
                    int ret;

                    if (exist)
                        ret = oOITT.Update();
                    else
                        ret = oOITT.Add();

                    if (ret != 0)
                    {
                        wte.ReadBySAP = 'E';
                        wte.MSG_SAP = SAPConnection.DI.GetLastErrorDescription();
                        WorkTreeDAO.UpdateStatus(wte);
                    }
                    else
                    {
                        wte.ReadBySAP = 'Y';
                        WorkTreeDAO.UpdateStatus(wte);
                    }
                }
                catch (Exception ex)
                {
                    wte.ReadBySAP = 'E';
                    wte.MSG_SAP = ex.Message;
                    WorkTreeDAO.UpdateStatus(wte);
                }
            }
        }
    }
}
