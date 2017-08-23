CREATE FUNCTION dbo.sp_Tbl_RowCOUNT (

        @sTableName sysname  -- Table to retrieve Row Count
        )

    RETURNS INT -- Row count of the table, NULL if not found.

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
