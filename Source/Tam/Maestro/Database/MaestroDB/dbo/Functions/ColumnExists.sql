CREATE FUNCTION [dbo].[ColumnExists] (@_table AS VARCHAR(128), @_column AS VARCHAR(128))  
RETURNS BIT AS  
BEGIN 

DECLARE @table_id INT
DECLARE @return_val BIT

IF EXISTS (SELECT id FROM dbo.sysobjects where id = object_id(@_table) AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	IF EXISTS (SELECT name FROM syscolumns WHERE id=object_id(@_table) AND name=@_column)
	BEGIN
		SET @return_val = 1
	END
	ELSE
	BEGIN
		SET @return_val = 0
	END
END
ELSE
BEGIN
	SET @return_val = 0
END

RETURN @return_val
END
