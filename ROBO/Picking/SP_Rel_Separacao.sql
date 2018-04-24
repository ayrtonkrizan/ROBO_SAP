SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[Sp_Rel_Separacao]
@Picking int

AS
BEGIN
		Select
			T0.CardCode As Cliente
			,T0.CardName As NomeC
			,T3.Address As Endereco
			,Isnull(T4.Carrier,'') As Transp
			, T1.itemcode As item
			, T5.Descr As Utilizacao
			,T2.U_Sucata As Endereco
			,T1.Dscription As DescricaoPrd
			,T1.Quantity As Qtde
			,T1.PickStatus As Status
			,T0.DocNum As Pedido
			,T1.pickidno As Picking 
		From ORDR T0
			inner join RDR1 T1 on T1.DocEntry = T0.DocEntry
			inner join OITM T2 on T2.ItemCode = T1.ItemCode
			inner join OCRD T3 on T3.CardCode = T0.CardCode
			left join RDR12 T4 on T4.DocEntry = T0.DocEntry
			left join OUSG T5 on T5.ID = T1.Usage
		Where T1.pickidno = @Picking
END








