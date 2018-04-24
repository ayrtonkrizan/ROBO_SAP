using System;
using System.Collections.Generic;
using System.Text;
using MyLibs.v2.local.Register;
using SAPbobsCOM;
using MyLibs.v2.local.Converters;
using MyLibs.v2.sbo;
using MyLibs.v2.sbo.DI;

namespace ROBO.Transacoes.Entidade
{
    /// <summary>
    /// Entidade para documentos de marketing.
    /// </summary>
    public class Documents
    {
        // Header
        public int DocNum = 0;
        public SAPConnection.DocsEnum ObjType;
        private string cardCode;
        public string CardCode
        {
            get { return cardCode; }

            set 
            {
                if (MyQuery.Exist("OCRD", value, "CardCode"))
                    cardCode = value;
                else
                    cardCode = null;
            }
        }
        private string cardName;
        public string CardName
        {
            get 
            {
                if (String.IsNullOrEmpty(cardName))
                    return MyQuery.GetValue("OCRD",cardCode,"CardCode","CardName").ToString();
                else
                    return cardName; 
            }
            set { cardName = value; }
        }
        public string NumAtCard;
        private int bplId = 0;
        public int BplId
        {
            get 
            {                
                return bplId; 
            }
            set 
            {                

                bplId = value; 
            }
        }
        public DateTime DocDate = DateTime.Now;      
        public DateTime DocDueDate = DateTime.Now;
        public DateTime TaxDate  = DateTime.Now;
        public int SlpCode = -1;
        public int GroupNum;

        // Lines
        public List<DocumentsEntityLine> Lines = new List<DocumentsEntityLine>();

        // Footer
        public int Slpcode;
        public string Comments;
        public double DiscPrcnt = 0;

        // Users Fileds - Campos de usuários
        public Dictionary<string, string> UserFields = new Dictionary<string, string>();
        // My Values - Meus parâmetros (não será adicionado no SBO).
        public Dictionary<string, ConvertTo> MyValues = new Dictionary<string, ConvertTo>();

    }
    
    /// <summary>
    /// Linha do documentos de marketing.
    /// </summary>
    public class DocumentsEntityLine
    {
        private string cardCode;
        private int priceList = -1;
        public int LineNum = -1;
        private string itemCode;
        public string ItemCode
        {
            get { return itemCode; }
            set 
            {              

                itemCode = value; }
        }
        public double Quantity;
        private double price = 0;
        public double Price
        {
            get 
            {
                if (price == 0)
                {
                    if (priceList >= 0)
                        return MyQuery.GetValue("OITM", priceList, "PriceList", "Price").ToDouble();
                    else
                        return 0;
                }
                else
                    return price; 
            }
            set { price = value; }
        }
        public double DiscPrcnt;
        public int SlpCode = -1;
        public string WhsCode;


        // Users Fileds - Campos de usuários
        public Dictionary<string, string> UserFields = new Dictionary<string, string>();
        // My Values - Meus parâmetros (não será adicionado no SBO).
        public Dictionary<string, ConvertTo> MyValues = new Dictionary<string, ConvertTo>();


        public SAPbobsCOM.BoStatus LineStatus;


        public DocumentsEntityLine(){}

        /// <summary>
        /// Construtor. Informe a lista de preço padrão
        /// </summary>
        /// <param name="priceList">Lista de Preço</param>
        public DocumentsEntityLine(int priceList)
        {
            
        }

        /// <summary>
        /// Construtor. Informe o Código do parceiro padrão
        /// </summary>
        /// <param name="cardCode">Código do Parceiro</param>
        public DocumentsEntityLine(string cardCode)
        {
            this.cardCode = cardCode;
        }
    }
}
