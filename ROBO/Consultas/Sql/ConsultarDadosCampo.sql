Select 
	T0."TableID"
	,T0."FieldID"
	,ISNULL(T1."FldValue" ,'') as 'Valor'
	,ISNULL(T1."Descr" ,'') as 'Descrição Valid Value'
	,ISNULL(T0."SizeId" ,'') as 'Tamanho'
	,ISNULL(T0."Dflt" ,'') as 'Valor Padrão'
	,ISNULL(T0."NotNull" ,'N') as 'Obrigatorio?'
	,ISNULL(T0."FieldID" ,'') as 'FieldID'
	,ISNULL(T0."Descr" ,'') as 'Descrição'
From 
	"CUFD" T0
	LEFT JOIN "UFD1" T1 ON T0."TableID"=T1."TableID" AND T0."FieldID"=T1."FieldID"
Where 
	T0."TableID"='{0}' 
	and  T0."AliasID"='{1}'