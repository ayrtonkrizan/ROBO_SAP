GO
/****** Object:  StoredProcedure [dbo].[SP_Rel_Etq]    Script Date: 29/09/2017 15:31:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Alter PROCEDURE [dbo].[SP_Rel_Manifesto]
	@DEntry as varchar(20)

AS
BEGIN
	Select distinct
		T1.CompnyName As End1
		,T1.CompnyAddr As End2
		,T6.CardName As Transp
		,T0.[U_ManifDt] As Data
		, 'Tipo/Modelo/Texto' As 'TPModText'
		,'Assinado' As Assinado
		,'Placa' As Placa 
		,'RG' As RG
		,T5.CardName As Cliente
		,(Select Count(A0.ItemCode) From INV1 A0 Where A0.DocEntry = T5.DocEntry) As QTdeNF
		, (Select Max(A0.U_Volume) From [@RSD_CNFEMBITEM] A0 Where A0.U_NumNF = T5.Serial) As Volume
		from [@RSD_CNFEMBHEAD] T0
			cross join OADM T1
			inner join [@RSD_CNFEMBITEM] T2 on T2.[DocEntry] = T0.[DocEntry]
			inner join OINV T5 on T5.Serial = T2.U_NumNF
			inner join INV1 T4 on T4.DocEntry = T5.DocEntry
			inner join OCRD T6 on T6.CardCode = T0.[U_ShipCode]
	where T0.Docentry = @DEntry

END