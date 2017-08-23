CREATE FUNCTION dbo.udf_Tbl_RowCOUNT (

        @sTableName sysname  -- Table to retrieve Row Count
        )

    RETURNS INT -- Row count of the table, NULL if not found.

/*
* Returns the row count for a table by examining sysindexes.
* This function must be run in the same database as the table.
*
* Common Usage:   
SELECT dbo.udf_Tbl_RowCOUNT ('')

* Test   
 PRINT 'Test 1 Bad table ' + CASE WHEN SELECT 
       dbo.udf_Tbl_RowCOUNT ('foobar') is NULL
        THEN 'Worked' ELSE 'Error' END
        
* © Copyright 2002 Andrew Novick http://www.NovickSoftware.com
* You may use this function in any of your SQL Server databases
* including databases that you sell, so long as they contain 
* other unrelated database objects. You may not publish this 
* UDF either in print or electronically.
***************************************************************/

AS BEGIN
    
    DECLARE @nRowCount INT -- the rows
    DECLARE @nObjectID int -- Object ID

    SET @nObjectID = OBJECT_ID(@sTableName)

    -- Object might not be found
    IF @nObjectID is null RETURN NULL

    SELECT TOP 1 @nRowCount = rows 
        FROM sysindexes 
        WHERE id = @nObjectID AND indid < 2

    RETURN @nRowCount
END 
