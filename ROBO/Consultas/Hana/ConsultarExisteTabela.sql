SELECT
	Count(1) AS "Count" 
FROM "{1}"."OUTB"  T10
WHERE UPPER(T10."TableName") = UPPER('{0}')