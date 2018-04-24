using System;
using System.Collections.Generic;
using System.Text;
using MyLibs.v2.sbo;
using MyLibs.v2.local.Register;
using MyLibs.v2.local.Converters;

namespace ROBO.Transacoes
{
    public class Orders
    {
        private SAPConnection.DocsEnum Table = SAPConnection.DocsEnum.ORDR;

        /// <summary>
        /// Registra o documento no SBO.
        /// </summary>
        /// <param name="oPedido">Entidade</param>
        /// <exception cref="SAPException">Retorno o erro do SAP em log</exception>
        public void Add(MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntity oPedido)
        {
            var oDocument = SAPConnection.GetDocument(Table);

            // Header
            oDocument.CardCode = oPedido.CardCode;
            oDocument.CardName = oPedido.CardName;
            oDocument.NumAtCard = oPedido.NumAtCard;
            oDocument.DocDate = oPedido.DocDate;
            oDocument.DocDueDate = oPedido.DocDueDate;
            oDocument.TaxDate = oPedido.TaxDate;
            oDocument.Series = oPedido.Series;

            if (SAPConnection.IsMultiBranchs())
                oDocument.BPL_IDAssignedToInvoice = oPedido.BplId;

            // Lines
            foreach (var line in oPedido.Lines)
            {
                oDocument.Lines.ItemCode = line.ItemCode;
                oDocument.Lines.Quantity = line.Quantity;
                oDocument.Lines.DiscountPercent = line.DiscPrcnt;

                if (line.LineNum >= 0)
                {
                    oDocument.Lines.BaseLine = line.LineNum;
                    oDocument.Lines.BaseType = (int)oPedido.ObjType;
                    oDocument.Lines.BaseEntry = oPedido.DocNum;
                }

                oDocument.Lines.Add();
            }

            // Footer
            oDocument.DiscountPercent = oPedido.DiscPrcnt;
            oDocument.Comments = oPedido.Comments;

            if (oDocument.Add() != 0)
                throw new MyLibs.v2.sbo.Register.SAPException(this.GetType(),"0 - Erro ao adicionar um PV");
            else
                oPedido.DocNum = SAPConnection.GetNewKey();

        }

        public void Delete(MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Atualiza o pedido de venda.
        /// </summary>
        /// <param name="oPedido">DocEntry</param>
        /// <exception cref="SAPException">Retorno o erro do SAP em log</exception>
        public void Update(MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntity oDocEntity)
        {
            var oDocument = SAPConnection.GetDocument(Table);

            if (!oDocument.GetByKey(oDocEntity.DocNum))
                throw new LogException(this.GetType(), System.Diagnostics.EventLogEntryType.Error, Properties.LogMessage.Erro_0033_2, oDocEntity.DocNum, oDocEntity.MyValues["Id"].ToString());


            oDocument.DocDate = oDocEntity.DocDate;
            oDocument.TaxDate = oDocEntity.TaxDate;
            oDocument.SalesPersonCode = oDocEntity.Slpcode;
            oDocument.NumAtCard = oDocEntity.NumAtCard;
            oDocument.Comments = oDocEntity.Comments;
            //oDocument.ObjType = ""

            foreach (var line in oDocEntity.Lines)
            {
                oDocument.Lines.Add();
                // @BFGUNDES - Atualiza somente linha aberta.
                if (oDocument.Lines.LineStatus == SAPbobsCOM.BoStatus.bost_Open)
                {
                    oDocument.Lines.SetCurrentLine(line.LineNum);
                    oDocument.Lines.ItemCode = line.ItemCode;
                    oDocument.Lines.Quantity = line.Quantity;
                    oDocument.Lines.Price = line.Price;
                }                
            }

            if (oDocument.Update() != 0)
            {
                oDocEntity.MyValues["MSG_SAP"] = new ConvertTo(SAPConnection.DI.GetLastErrorDescription());
                oDocEntity.MyValues["ReadBySAP"] = new ConvertTo("E");

                foreach (var line in oDocEntity.Lines)
                {
                    line.MyValues["ReadBySAP"] = new ConvertTo("E");
                }

                principal.GravaAudit(this.GetType().Name.ToString() + " 0 - Não foi possivel atualizar pedido de venda nº " + oDocEntity.DocNum.ToString());
            }
        }

        /// <summary>
        /// Obter os dados do documento.
        /// </summary>
        /// <param name="docNum">Número do documento</param>
        /// <returns></returns>
        public MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntity getByKey(int docNum)
        {
            var oDocEntity = new MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntity(Table);

            var oDocument = SAPConnection.GetDocument(Table);

            //if (!oDocument.GetByKey(docNum))
            //    throw new LogException(this.GetType(), System.Diagnostics.EventLogEntryType.Error, Properties.Message_v2.i900014_iDocuments_2, Table);


            oDocEntity.DocNum = oDocument.DocNum;
            oDocEntity.CardCode = oDocument.CardCode;
            oDocEntity.DocDate = oDocument.DocDate;
            oDocEntity.TaxDate = oDocument.TaxDate;
            oDocEntity.BplId = oDocument.BPL_IDAssignedToInvoice;
            oDocEntity.Slpcode = oDocument.SalesPersonCode;
            oDocEntity.NumAtCard = oDocument.NumAtCard;
            oDocEntity.ObjType = (SAPConnection.DocsEnum)(int)oDocument.DocObjectCode;

            for (int i = 0; i < oDocument.Lines.Count; i++)
            {
                var oDocEntityLine = new MyLibs.v2.sbo.DI.Documents.Entity.DocumentsEntityLine(oDocument.CardCode);

                oDocument.Lines.SetCurrentLine(i);
                oDocEntityLine.ItemCode = oDocument.Lines.ItemCode;
                oDocEntityLine.Quantity = oDocument.Lines.Quantity;
                oDocEntityLine.Price = oDocument.Lines.Price;
                oDocEntityLine.LineNum = oDocument.Lines.LineNum;
                oDocEntityLine.LineStatus = oDocument.Lines.LineStatus;

                oDocEntity.Lines.Add(oDocEntityLine);
            }

            oDocument = null;

            return oDocEntity;
        }
    }
}
