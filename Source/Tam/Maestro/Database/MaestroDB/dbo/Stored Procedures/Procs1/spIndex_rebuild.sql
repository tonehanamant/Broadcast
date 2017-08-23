CREATE PROCEDURE [dbo].[spIndex_rebuild]
@frag_set int,
@Date_Diff int,
@Row_Diff int
AS

SET NOCOUNT ON;
DECLARE @objectid int;
DECLARE @indexid int;
DECLARE @partitioncount int;
DECLARE @schemaname nvarchar(130); 
DECLARE @objectname nvarchar(130); 
DECLARE @indexname nvarchar(130); 
DECLARE @partitionnum int;
DECLARE @partitions int;
DECLARE @frag float;
DECLARE @command nvarchar(4000); 
declare @RowCnt bigint;
declare @start_time datetime;
declare @page_count int; 
declare @index_type_desc varchar(100);
declare @update_type tinyint;
declare @last_run_time datetime

set  @last_run_time=GETDATE()-@Date_Diff 

--get row count

--delete temp table
IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_row_count') IS NOT NULL 
	DROP TABLE temp_db_backup.dbo.temp_reindex_row_count;

select aa.table_id, i.name as index_name, aa.RowCnt
into temp_db_backup.dbo.temp_reindex_row_count
from (SELECT ta.OBJECT_ID as table_id ,SUM(pa.rows) RowCnt
FROM sys.tables ta
INNER JOIN sys.partitions pa
	ON pa.OBJECT_ID = ta.OBJECT_ID
	INNER JOIN sys.schemas sc
	ON ta.schema_id = sc.schema_id
WHERE ta.is_ms_shipped = 0 AND pa.index_id IN (1,0)
and ta.OBJECT_ID not in (select table_id from dba.dbo.index_update
						where start_time >@last_run_time)
GROUP BY sc.name, ta.OBJECT_ID, ta.name) as aa
left join sys.indexes i
on i.object_id=aa.table_id
where i.name is not null
and i.type in (1, 2)
and aa.RowCnt>@Row_Diff
order by 1, 2 --66 rows

--drop temp table
IF OBJECT_ID('temp_db_backup.dbo.temp_reindex') IS NOT NULL 
	DROP TABLE temp_db_backup.dbo.temp_reindex;
	
-- Conditionally select tables and indexes from the sys.dm_db_index_physical_stats function 
-- and convert object and index IDs to names.
SELECT
    object_id AS objectid,
    index_id AS indexid,
    partition_number AS partitionnum,
    avg_fragmentation_in_percent AS frag, 
    page_count, 
    index_type_desc
INTO temp_db_backup.dbo.temp_reindex
FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL , NULL, 'LIMITED')
join temp_db_backup.dbo.temp_reindex_row_count r
on r.table_id=object_id 
WHERE index_type_desc in ('CLUSTERED INDEX', 'NONCLUSTERED INDEX')
and index_id > 0
and object_name(object_id) is not null
and avg_fragmentation_in_percent > @frag_set;

-- Declare the cursor for the list of partitions to be processed.
DECLARE rebuild_index CURSOR 
FOR 
SELECT * FROM temp_db_backup.dbo.temp_reindex;

-- Open the cursor.
OPEN rebuild_index;

-- Loop through the rebuild_index.
WHILE (1=1)
    BEGIN;
        FETCH NEXT
           FROM rebuild_index
           INTO @objectid, @indexid, @partitionnum, @frag, @page_count, @index_type_desc;
           
        IF @@FETCH_STATUS < 0 BREAK;
        
        SELECT @objectname = QUOTENAME(o.name), @schemaname = QUOTENAME(s.name), @RowCnt=t.RowCnt
        FROM sys.objects AS o
        JOIN sys.schemas as s ON s.schema_id = o.schema_id
        join temp_db_backup.dbo.temp_reindex_row_count t
        on t.table_id=@objectid
        WHERE o.object_id = @objectid;
               
        SELECT @indexname = QUOTENAME(name)
        FROM sys.indexes
        WHERE  object_id = @objectid AND index_id = @indexid;
        
        SELECT @partitioncount = count (*)
        FROM sys.partitions
        WHERE object_id = @objectid AND index_id = @indexid;

--	set start time
		set @start_time=GETDATE()
 --30 is an arbitrary decision point at which to switch between reorganizing and rebuilding.
        IF @frag < 30.0
		begin
            SET @command = N'ALTER INDEX ' + @indexname + N' ON ' + @schemaname + N'.' + @objectname + N' REORGANIZE';
            set @update_type=0;
        end
        IF @frag >= 30.0
        begin
            SET @command = N'ALTER INDEX ' + @indexname + N' ON ' + @schemaname + N'.' + @objectname + N' REBUILD';
            set @update_type=1;
        end
        IF @partitioncount > 1
            SET @command = @command + N' PARTITION=' + CAST(@partitionnum AS nvarchar(10));
       EXEC (@command);
 --      PRINT N'Executed: ' + @command;
	--	insert record
		insert into dba.dbo.index_update(database_id, [schema_id], table_id, index_id,  tablename, indexname, 
			row_count, partition_number, index_date, index_dt, update_type, start_time, end_time, frag, 
			sql_commond, allocUnitType, indexType, page_count)
		values(DB_ID(), schema_id(), @objectid, @indexid, @objectname, @indexname, 
			@RowCnt, @partitionnum, null, GETDATE(), @update_type, @start_time, GETDATE(), @frag,  --1 for  REBUILD; 0 for REORGANIZE
			@command, 'null', @index_type_desc, @page_count)
    END;

-- Close and deallocate the cursor.
CLOSE  rebuild_index;
DEALLOCATE  rebuild_index;

-- Drop the temporary table.
IF OBJECT_ID('temp_db_backup.dbo.temp_reindex') IS NOT NULL 
	DROP TABLE temp_db_backup.dbo.temp_reindex;

IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_row_count') IS NOT NULL 
	DROP TABLE temp_db_backup.dbo.temp_reindex_row_count;	

