
create PROCEDURE [dba].[listTableRowCounts] 
AS 
BEGIN 
    SET NOCOUNT ON 
 
    DECLARE @SQL VARCHAR(255) 
    SET @SQL = 'DBCC UPDATEUSAGE (' + DB_NAME() + ')' 
    EXEC(@SQL) 
 
    CREATE TABLE #foo 
    ( 
        tablename VARCHAR(255), 
        rc INT 
    ) 
     
    INSERT #foo 
        EXEC sp_msForEachTable 
            'SELECT PARSENAME(''?'', 1), 
            COUNT(*) FROM ?' 
 
    SELECT tablename, rc 
        FROM #foo 
        ORDER BY rc DESC 
 
    DROP TABLE #foo 
END
