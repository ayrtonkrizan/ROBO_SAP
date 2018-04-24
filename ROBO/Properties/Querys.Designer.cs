﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ROBO.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Querys {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Querys() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ROBO.Properties.Querys", typeof(Querys).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE	 @id as int
        ///		,@transType as int
        ///
        ///set	@id = {0};
        ///set @transType = {1};
        ///
        ///DELETE [dbo].[TRANSACOES_LOG] WHERE [Id] = @id AND [TransactionType] = @transType.
        /// </summary>
        internal static string ApagaLog_2 {
            get {
                return ResourceManager.GetString("ApagaLog_2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE  @DocNum as int
        ///		,@ReadBySAP as char(1)
        ///		,@Id as int;
        ///
        ///set @DocNum = &apos;{0}&apos;
        ///set @ReadBySAP = &apos;{1}&apos;
        ///set @Id = &apos;{2}&apos;
        ///
        ///
        ///UPDATE	 [Sage_Transacoes]
        ///SET		 DocNum = @DocNum
        ///		,ReadBySAP = @ReadBySAP
        ///WHERE	 Id = @Id.
        /// </summary>
        internal static string AtualizaStatusTransacao_3 {
            get {
                return ResourceManager.GetString("AtualizaStatusTransacao_3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE OCRD SET U_Processado = &apos;Y&apos;
        ///		WHERE U_Processado &lt;&gt; &apos;Y&apos;
        ///			AND CardType = &apos;C&apos;
        ///			AND CardCode IN (	SELECT DISTINCT [CardCode_SAP] 
        ///								FROM	[SAGE_CLI] 
        ///								WHERE	CreateDate = OCRD.CreateDate 
        ///									AND	UpdateDate = OCRD.UpdateDate).
        /// </summary>
        internal static string CLIENTE_AtualizaStatus_0 {
            get {
                return ResourceManager.GetString("CLIENTE_AtualizaStatus_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE	[SAGE_CLI]
        ///WHERE	[CardCode_SAP] IN ( SELECT	 OCRD.CardCode 
        ///							FROM	 OCRD
        ///							INNER JOIN [SAGE_CLI]
        ///								ON	 OCRD.CardCode = [SAGE_CLI].[CardCode_SAP]
        ///							WHERE	(	OCRD.CreateDate &lt;&gt; [SAGE_CLI].CreateDate OR
        ///										OCRD.UpdateDate &lt;&gt; [SAGE_CLI].UpdateDate)
        ///								OR	 OCRD.U_Processado &lt;&gt; &apos;Y&apos;).
        /// </summary>
        internal static string CLIENTE_RemoveDesatualizado_0 {
            get {
                return ResourceManager.GetString("CLIENTE_RemoveDesatualizado_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO	 [SAGE_CLI]			
        ///		(	 [CardCode_SAP]		,[CardCode_Jomar]
        ///			,[CardType]			,[CardName]
        ///			,[CardFName]		,[LineNum]
        ///			,[TypeofAddress]	,[AddressName]
        ///			,[Address]			,[AddrType]
        ///			,[Street]			,[StreetNo]
        ///			,[Building]			,[Block]
        ///			,[ZipCode]			,[State]
        ///			,[County]			,[City]
        ///			,[Country]			,[FirstName]
        ///			,[E_MailL]			,[Tel]
        ///			,[TaxId0]			,[TaxId1]
        ///			,[TaxId3]			,[TaxId4]
        ///			,[CONdP]			,[CreateDate]
        ///			,[UpdateDate]					) 
        ///
        ///		SELECT	
        ///				 ISNULL(ocrd_1.CardCode, &apos;&apos;)			As Card [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CLIENTES_Integrar_0 {
            get {
                return ResourceManager.GetString("CLIENTES_Integrar_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE @ItemCode as nvarchar(100);
        ///DECLARE @WhsCode as  nvarchar(100);
        ///
        ///set @ItemCode = &apos;{0}&apos;
        ///set @WhsCode = &apos;{1}&apos;
        ///
        ///SELECT	 CASE	WHEN AvgPrice = 0 
        ///				THEN (SELECT U_Custo_Std FROM OITM WHERE ItemCode = @ItemCode)
        ///				ELSE AvgPrice END Custo
        ///FROM OITW
        ///WHERE	 ItemCode = @ItemCode
        ///	AND	 WhsCode = @WhsCode.
        /// </summary>
        internal static string CustoItem_2 {
            get {
                return ResourceManager.GetString("CustoItem_2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE OCRD SET U_Processado = &apos;Y&apos;
        ///		WHERE U_Processado &lt;&gt; &apos;Y&apos;
        ///			AND CardType = &apos;S&apos;
        ///			AND CardCode IN (	SELECT DISTINCT [CardCode_SAP] 
        ///								FROM	[SAGE_FOR] 
        ///								WHERE	CreateDate = OCRD.CreateDate 
        ///									AND	UpdateDate = OCRD.UpdateDate).
        /// </summary>
        internal static string FORNECEDOR_AtualizaStatus_0 {
            get {
                return ResourceManager.GetString("FORNECEDOR_AtualizaStatus_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO	 [SAGE_FOR]			
        ///		(	 [CardCode_SAP]		,[CardCode_Jomar]
        ///			,[CardType]			,[CardName]
        ///			,[CardFName]		,[LineNum]
        ///			,[TypeofAddress]	,[AddressName]
        ///			,[Address]			,[AddrType]
        ///			,[Street]			,[StreetNo]
        ///			,[Building]			,[Block]
        ///			,[ZipCode]			,[State]
        ///			,[County]			,[City]
        ///			,[Country]			,[FirstName]
        ///			,[E_MailL]			,[Tel]
        ///			,[TaxId0]			,[TaxId1]
        ///			,[TaxId3]			,[TaxId4]
        ///			,[CONdP]			,[CreateDate]
        ///			,[UpdateDate]					) 
        ///
        ///		SELECT	
        ///				 ISNULL(ocrd_1.CardCode, &apos;&apos;)			As Card [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string FORNECEDOR_Integrar_1 {
            get {
                return ResourceManager.GetString("FORNECEDOR_Integrar_1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE	[SAGE_FOR]
        ///WHERE	[CardCode_SAP] IN ( SELECT	 OCRD.CardCode 
        ///							FROM	 OCRD
        ///							INNER JOIN [SAGE_FOR]
        ///								ON	 OCRD.CardCode = [SAGE_FOR].[CardCode_SAP]
        ///							WHERE	(	OCRD.CreateDate &lt;&gt; [SAGE_FOR].CreateDate OR
        ///										OCRD.UpdateDate &lt;&gt; [SAGE_FOR].UpdateDate)
        ///								OR	 OCRD.U_Processado &lt;&gt; &apos;Y&apos;).
        /// </summary>
        internal static string FORNECEDOR_RemoveDesatualizado_0 {
            get {
                return ResourceManager.GetString("FORNECEDOR_RemoveDesatualizado_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE	 @id as int
        ///		,@transType as int
        ///		,@msg as nvarchar(300)
        ///
        ///
        ///set	@id = {0};
        ///set @transType = {1};
        ///set @msg = &apos;{2}&apos;;
        ///
        ///INSERT INTO [dbo].[TRANSACOES_LOG] ([Id],[TransactionType],[Message]) 
        ///	VALUES (@id, @transType, @msg).
        /// </summary>
        internal static string InsereLog_3 {
            get {
                return ResourceManager.GetString("InsereLog_3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT	 RDR1.LineNum
        ///		,RDR1.ItemCode
        ///		,RDR1.Quantity
        ///		,RDR1.Price
        ///		,RDR1.WhsCode
        ///		,ORDR.ObjType 
        ///		,ORDR.DocNum
        ///		,(SELECT RDR1.Quantity - ISNULL(SUM(Quantity),0) FROM INV1 WHERE BaseType = ORDR.ObjType AND BaseEntry = ORDR.DocEntry AND LineNum = RDR1.LineNum) Dif
        ///FROM	 RDR1 
        ///INNER JOIN ORDR
        ///	ON	 ORDR.DocEntry = RDR1.DocEntry
        ///WHERE	 ORDR.DocEntry = &apos;{0}&apos;
        ///	AND	 RDR1.LineStatus = &apos;O&apos;.
        /// </summary>
        internal static string ItensAberto_RDR1_1 {
            get {
                return ResourceManager.GetString("ItensAberto_RDR1_1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO	 [SAGE_CLI]			
        ///		(	 [CardCode_SAP]		,[CardCode_Jomar]
        ///			,[CardType]			,[CardName]
        ///			,[CardFName]		,[LineNum]
        ///			,[TypeofAddress]	,[AddressName]
        ///			,[Address]			,[AddrType]
        ///			,[Street]			,[StreetNo]
        ///			,[Building]			,[Block]
        ///			,[ZipCode]			,[State]
        ///			,[County]			,[City]
        ///			,[Country]			,[FirstName]
        ///			,[E_MailL]			,[Tel]
        ///			,[TaxId0]			,[TaxId1]
        ///			,[TaxId3]			,[TaxId4]
        ///			,[CONdP]			,[CreateDate]
        ///			,[UpdateDate]					) 
        ///
        ///		SELECT	
        ///				 ISNULL(ocrd_1.CardCode, &apos;&apos;)			As Card [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PopulaCliente_0_DEPRECIADO {
            get {
                return ResourceManager.GetString("PopulaCliente_0_DEPRECIADO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT	 
        ///		 [GroupID]
        ///		,TransactionType
        ///		,OWOR_ORDER_JOMAR
        ///		,OWOR_DocNum
        ///		,OITM_CodeBars
        ///		,OWOR_WareHouse
        ///		,-SUM(IGN1_Quantity) IGN1_Quantity 
        ///		,	CAST(TransactionType as nvarchar(100)) 
        ///		+	CAST(OWOR_ORDER_JOMAR as nvarchar(100)) 
        ///		+	CAST(OITM_CodeBars AS nvarchar(20))
        ///		+	CAST(OWOR_DocNum AS nvarchar(20))
        ///		+	OWOR_WareHouse as GroupUnique
        ///		,OWOR_DocNum
        ///FROM	 [dbo].[Sage_Transaction08] WITH (NOLOCK) 
        ///WHERE	 [TransactionType] = -8
        ///	AND  OWOR_DocNum &gt; 1
        ///	AND	 OIGE_DocNum = 0
        ///	AND (O [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Transacao_n08OIGE_0 {
            get {
                return ResourceManager.GetString("Transacao_n08OIGE_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT	 CAST(RAND() * 1000 AS INT) + OWOR_ORDER_JOMAR [GroupID]
        ///		,TransactionType
        ///		,OWOR_ORDER_JOMAR,OITM_CodeBars
        ///		,OWOR_WareHouse
        ///		,SUM(OWOR_PlannedQty) OWOR_PlannedQty 
        ///		,	CAST(TransactionType as nvarchar(100)) 
        ///		+	CAST(OWOR_ORDER_JOMAR as nvarchar(100)) 
        ///		+	CAST(OITM_CodeBars AS nvarchar(20))
        ///		+	CAST(OWOR_DocNum AS nvarchar(20))
        ///		+	OWOR_WareHouse as GroupUnique
        ///FROM	 [dbo].[Sage_Transaction08] WITH (NOLOCK) 
        ///WHERE	ReadBySAP &lt;&gt; &apos;Y&apos; 
        ///	AND [TransactionType] = -8
        ///	AND (OWOR_DocNum IS N [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Transacao_n08OWOR_0 {
            get {
                return ResourceManager.GetString("Transacao_n08OWOR_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///		 [GroupID]
        ///		,TransactionType
        ///		,OWOR_ORDER_JOMAR
        ///		,OWOR_DocNum
        ///		,OITM_CodeBars
        ///		,OWOR_WareHouse
        ///		,-SUM(IGN1_Quantity) IGN1_Quantity
        ///		,	CAST(TransactionType as nvarchar(100))
        ///		+	CAST(OWOR_ORDER_JOMAR as nvarchar(100))
        ///		+	CAST(OITM_CodeBars AS nvarchar(20))
        ///		+	CAST(OWOR_DocNum AS nvarchar(20))
        ///		+	OWOR_WareHouse as GroupUnique
        ///		,OWOR_DocNum
        ///FROM	 [dbo].[Sage_Transaction_WO] WITH (NOLOCK)
        ///WHERE	 [TransactionType] = -100
        ///	AND  OWOR_DocNum &gt; 1
        ///	AND	 OIGE_DocNum = 0
        ///	AND (OWOR [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Transacao_nW0OIGE_0 {
            get {
                return ResourceManager.GetString("Transacao_nW0OIGE_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT	 CAST(RAND() * 1000 AS INT) + OWOR_ORDER_JOMAR [GroupID]
        ///		,TransactionType
        ///		,OWOR_ORDER_JOMAR,OITM_CodeBars
        ///		,OWOR_WareHouse
        ///		,SUM(OWOR_PlannedQty) OWOR_PlannedQty 
        ///		,	CAST(TransactionType as nvarchar(100)) 
        ///		+	CAST(OWOR_ORDER_JOMAR as nvarchar(100)) 
        ///		+	CAST(OITM_CodeBars AS nvarchar(20))
        ///		+	CAST(OWOR_DocNum AS nvarchar(20))
        ///		+	OWOR_WareHouse as GroupUnique
        ///FROM	 [dbo].[Sage_Transaction_WO] WITH (NOLOCK) 
        ///WHERE	ReadBySAP &lt;&gt; &apos;Y&apos; 
        ///	AND [TransactionType] = -100
        ///	AND (OWOR_DocNum I [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Transacao_nWOOWOR_0 {
            get {
                return ResourceManager.GetString("Transacao_nWOOWOR_0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///		 Id
        ///		,AddDate
        ///		,TransactionType
        ///		,ReadBySAP
        ///		,DocNum
        ///		,DocDate
        ///		,TaxDate
        ///		,ISNULL(GroupNum,0) as GroupNum
        ///		,BplId
        ///		,OITM_CodeBars
        ///		,U_Lot
        ///		,U_Quality
        ///		,Quantity
        ///		,WareHouse
        ///		,Comments
        ///FROM	 dbo.[Sage_Transacoes] WITH (NOLOCK) 
        ///WHERE	 ReadBySAP &lt;&gt; &apos;Y&apos;
        ///	AND	 (CAST(TransactionType AS int) = {0} OR CAST(TransactionType AS int) = -{0})
        ///ORDER BY ReadBySAP DESC, BplId ASC.
        /// </summary>
        internal static string Transacoes_1 {
            get {
                return ResourceManager.GetString("Transacoes_1", resourceCulture);
            }
        }
    }
}