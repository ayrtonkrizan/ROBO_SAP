Select 
	Count(1) As "Count"
From 
	"ORTT" T0
	INNER JOIN "OADM" T1 ON T0."Currency" = T1."SysCurrncy"
Where 
	T0."RateDate" = CAST('[DATA]' AS DATE)