USE [SBODemoBR]
GO
/****** Object:  StoredProcedure [dbo].[SP_Rel_Etq]    Script Date: 28/09/2017 12:42:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SP_Rel_Etq]
	@Pick as nvarchar(20)
	,@Vol as nvarchar(10)

AS
BEGIN

	Select distinct T1.CompnyName, T1.CompnyAddr, T1.Phone1, T5.CardName, Isnull(T3.U_Sucata,'') As Endereco,
			Cast(T5.Serial As varchar(15)) + ' / ' + @Vol As Volume
		from [@RSD_REGSEPHEAD] T0
			cross join OADM T1
			inner join [@RSD_REGSEPLOTE] T2 on T2.Code = T0.Code
			inner join OITM T3 on T3.ItemCode = T2.U_ItemCode
			inner join INV1 T4 on T4.PickIdNo = T0.U_PickID
			inner join OINV T5 on T5.DocEntry = T4.DocEntry
	where T0.U_PickID = @Pick

END
