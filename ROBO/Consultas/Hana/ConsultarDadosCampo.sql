Select 
	T0."TableID"
	,T0."FieldID"
	,IFNULL(T1."FldValue" ,'') as "Valor"
	,IFNULL(T1."Descr" ,'') as "Descrição Valid Value"
	,IFNULL(T0."SizeID" ,0) as "Tamanho"
	,IFNULL(T0."Dflt" ,'') as "Valor Padrão"
	,IFNULL(T0."NotNull" ,'N') as "Obrigatorio?"
	,IFNULL(T0."FieldID" ,0) as "FieldID"
	,IFNULL(T0."Descr" ,'') as "Descrição"
From 
	"{2}"."CUFD" T0
	LEFT JOIN "{2}"."UFD1" T1 ON T0."TableID"=T1."TableID" AND T0."FieldID"=T1."FieldID"
Where 
	T0."TableID"='{0}' 
	and  T0."AliasID"='{1}'