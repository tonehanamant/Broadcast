--exec [dbo].[usp_table_index_size]

CREATE PROCEDURE [dbo].[usp_table_index_size]
AS

SET NOCOUNT ON

DECLARE @cmdstr varchar(200), @dup_name varchar(200), @dup_count bigint
declare @TempTable table
 ([Table_Name] varchar(100),
	Row_Count  varchar(50),
	Table_Size varchar(50),
	Data_Space_Used varchar(50),
	Index_Space_Used varchar(50),
	Unused_Space varchar(50)
 )
--Create Stored Procedure String
SELECT @cmdstr = 'sp_msforeachtable ''sp_spaceused "?"'''
--Populate Table variable
INSERT INTO @TempTable  EXEC(@cmdstr);

--select * from @TempTable 

select @dup_name=Table_Name, @dup_count=row_count
from (
select Table_Name, count(1) as c_, min(cast(row_count as bigint)) as row_count
from @TempTable
group by Table_Name
having count(1)>1) as aa

delete from @TempTable 
where Table_Name=@dup_name
and cast(row_count as bigint)=@dup_count

--select * from @TempTable 

--notify 100% table size change
--sent email 
DECLARE @DatabaseName varchar(50), @TableName varchar(200), @TableKBUsed bigint 
DECLARE @TableKBLast bigint, @IndexKBUsed bigint, @IndexKBLast bigint
DECLARE @Message nvarchar(2000)

DECLARE kb_cursor CURSOR FOR 
 select Table_Name, db_name(), cast(replace(Data_Space_Used, 'KB', '') as bigint), 
	cast(replace(Index_Space_Used,  'KB', '') as bigint)
 from @TempTable;


OPEN kb_cursor
FETCH NEXT FROM kb_cursor
INTO @TableName, @DatabaseName, @TableKBUsed, @IndexKBUsed

WHILE @@FETCH_STATUS = 0
BEGIN
--table and Index changes
	SELECT @TableKBLast =aa.Data_Space_Used,
		@IndexKBLast =aa.Index_Space_Used from (
			select top 1 Index_Space_Used, Data_Space_Used  
				FROM dba.dbo.table_index_size
				where Server_Name='TAMSQL04'
				AND database_name=@DatabaseName
				AND table_name=@TableName
				AND table_name<>'Total'
				order by date_id desc) as aa;

--email for table change
	SET @Message=N'TAMSQL04 Server Database ' + cast(@DatabaseName as nvarchar(20)) +
		N' Table ' + cast(@TableName as nvarchar(100)) + N' current size is '+ 
		cast(@TableKBUsed as nvarchar(20))+ N' (KB) and last size is '+ 
		cast(@TableKBLast as nvarchar(20)) + N' (KB).'; 

--print @TableKBLast
--print ABS(@TableKBLast-@TableKBUsed)*100/(@TableKBLast+0.001)
	IF @TableKBLast>0 and ABS(@TableKBLast-@TableKBUsed)*100/(@TableKBLast+0.001)>100
				EXECUTE msdb.dbo.sp_notify_operator @name=N'cloud_TAMSQL04',
					@subject=N'TAMSQL04 Table 100% OR More Changes',
					@body=@Message;

--email for index change
	SET @Message=N'TAMSQL04 Server Database ' + cast(@DatabaseName as nvarchar(20)) +
		N' Table Index ' + cast(@TableName as nvarchar(100)) + N' current size is '+ 
		cast(@IndexKBUsed as nvarchar(20))+ N' (KB) and last size is '+ 
		cast(@IndexKBLast as nvarchar(20)) + N' (KB).'; 

	IF @IndexKBLast>0 and ABS(@IndexKBLast-@IndexKBUsed)*100/(@IndexKBLast+0.001)>100
				EXECUTE msdb.dbo.sp_notify_operator @name=N'cloud_TAMSQL04',
					@subject=N'TAMSQL04 Index 100% OR More Changes',
					@body=@Message;

	FETCH NEXT FROM kb_cursor
		INTO @TableName, @DatabaseName, @TableKBUsed, @IndexKBUsed;
END;

CLOSE kb_cursor;
DEALLOCATE kb_cursor;

--insert data to table_index_size
 INSERT INTO dba.dbo.table_index_size(date_id, Table_Name,database_name, Row_Count, server_name,
	Table_Size, Data_Space_Used, Index_Space_Used, Unused_Space)
 select cast(getdate() as smalldatetime), Table_Name, db_name(), cast(Row_Count as bigint), 'TAMSQL04',
	cast(replace(Table_Size, 'KB', '') as bigint), 
	cast(replace(Data_Space_Used, 'KB', '') as bigint), 
	cast(replace(Index_Space_Used,  'KB', '') as bigint), 
	cast(replace(Unused_Space, 'KB', '') as bigint)
 from @TempTable;

-- Total size change notification
select @DatabaseName=db_name(),  @TableKBUsed=sum(cast(replace(Data_Space_Used, 'KB', '') as bigint)), 
	@IndexKBUsed=sum(cast(replace(Index_Space_Used,  'KB', '') as bigint))
from @TempTable;

SELECT @TableKBLast =aa.Data_Space_Used,
		@IndexKBLast =aa.Index_Space_Used from (
			select top 1 Index_Space_Used, Data_Space_Used  
				FROM dba.dbo.table_index_size
				where Server_Name='TAMSQL04'
				AND database_name=@DatabaseName
				AND table_name='Total'
				order by date_id desc) as aa;

--email for databsae 30% change
	SET @Message=N'TAMSQL04 Server Database ' + cast(@DatabaseName as nvarchar(20)) +
		N' Total current size is '+ cast(@TableKBUsed as nvarchar(20))+ N' (KB) and last size is '+ 
		cast(@TableKBLast as nvarchar(20)) + N' (KB).'; 

	IF @TableKBLast>0 and ABS(@TableKBLast-@TableKBUsed)*100/(@TableKBLast+0.001)>100
				EXECUTE msdb.dbo.sp_notify_operator @name=N'cloud_TAMSQL04',
					@subject=N'TAMSQL04 Server Total 100% More Changes',
					@body=@Message;

--email for total index change
	SET @Message=N'TAMSQL04 Server Database ' + cast(@DatabaseName as nvarchar(20)) +
		N' Total Index current size is '+ cast(@IndexKBUsed as nvarchar(20))+ N' (KB) and last size is '+ 
		cast(@IndexKBLast as nvarchar(20)) + N' (KB).'; 

	IF @IndexKBLast>0 and ABS(@IndexKBLast-@IndexKBUsed)*100/(@IndexKBLast+0.001)>100
				EXECUTE msdb.dbo.sp_notify_operator @name=N'cloud_TAMSQL04',
					@subject=N'TAMSQL04 Total Index 100% More Changes',
					@body=@Message;

--add total
 INSERT INTO dba.dbo.table_index_size(date_id, Table_Name, database_name, Row_Count, server_name,
	Table_Size, Data_Space_Used, Index_Space_Used, Unused_Space)
 select cast(getdate() as smalldatetime), 'Total', db_name(), sum(cast(Row_Count as bigint)), 'TAMSQL04',
	sum(cast(replace(Table_Size, 'KB', '') as bigint)), 
	sum(cast(replace(Data_Space_Used, 'KB', '') as bigint)), 
	sum(cast(replace(Index_Space_Used,  'KB', '') as bigint)), 
	sum(cast(replace(Unused_Space, 'KB', '') as bigint))
 from @TempTable;