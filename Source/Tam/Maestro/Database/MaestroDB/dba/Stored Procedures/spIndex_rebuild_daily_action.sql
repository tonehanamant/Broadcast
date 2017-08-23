
--exec [dba].[spIndex_rebuild_daily_action] 'maestro', 1, 20, 0, 0, 40
--select * from dbo.daily_reindex_list
--select * from dbo.reindex_daily_row_count_daily
CREATE PROCEDURE [dba].[spIndex_rebuild_daily_action]
@dbname_ varchar(50),
@priority_type int,
@frag_set int,
@Date_Diff int,
@Row_Diff INT,
@index_no int
AS

SET NOCOUNT ON;
DECLARE @pid int;
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
--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily_row_count_daily') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily_row_count_daily;

--select aa.table_id, i.name as index_name, aa.RowCnt
--into temp_db_backup.dbo.temp_reindex_daily_row_count_daily
--from (SELECT ta.OBJECT_ID as table_id ,SUM(pa.rows) RowCnt
--FROM sys.tables ta
--INNER JOIN sys.partitions pa
--	ON pa.OBJECT_ID = ta.OBJECT_ID
--	INNER JOIN sys.schemas sc
--	ON ta.schema_id = sc.schema_id
--WHERE ta.is_ms_shipped = 0 AND pa.index_id IN (1,0)
--and ta.OBJECT_ID not in (select table_id from dba.dbo.index_update
--						where start_time >@last_run_time)
--GROUP BY sc.name, ta.OBJECT_ID, ta.name) as aa
--left join sys.indexes i
--on i.object_id=aa.table_id
--where i.name is not null
--and i.type in (1, 2)
--and aa.RowCnt>@Row_Diff
--order by 1, 2 --66 rows

--drop temp table
--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily;
	
-- Conditionally select tables and indexes from the sys.dm_db_index_physical_stats function 
-- and convert object and index IDs to names.
--SELECT
--    object_id AS objectid,
--    index_id AS indexid,
--    partition_number AS partitionnum,
--    avg_fragmentation_in_percent AS frag, 
--    page_count, 
--    index_type_desc
--INTO temp_db_backup.dbo.temp_reindex_daily
--FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL , NULL, 'LIMITED')
--join temp_db_backup.dbo.temp_reindex_daily_row_count_daily r
--on r.table_id=object_id 
--WHERE index_type_desc in ('CLUSTERED INDEX', 'NONCLUSTERED INDEX')
--and index_id > 0
--and object_name(object_id) is not null
--and avg_fragmentation_in_percent > @frag_set
--ORDER BY 4;

--get top @index_no
--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily1') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily1;
	
--SET @command='select top '+ CAST(@index_no AS varchar(3)) + ' * into temp_db_backup.dbo.temp_reindex_daily1 from temp_db_backup.dbo.temp_reindex_daily'
----PRINT @command
--EXEC(@command)

--insert into dba.dbo.daily_reindex_list([db_name], objectid, indexid, partitionnum, frag, page_count, 
--index_type_desc, priority_type, processed)
--SELECT @dbname_, objectid,	indexid,	partitionnum,	frag,	page_count,	index_type_desc, 
--@priority_type, 0 as processed
--FROM temp_db_backup.dbo.temp_reindex_daily1
--order by 6 desc, 5 desc

-- Declare the cursor for the list of partitions to be processed.
DECLARE rebuild_index CURSOR 
FOR 
SELECT id, objectid, indexid, partitionnum, frag, page_count, index_type_desc
FROM dba.dbo.daily_reindex_list
where [db_name]=@dbname_ 
and processed<>1
order by id asc, priority_type asc;


-- Open the cursor.
OPEN rebuild_index;

-- Loop through the rebuild_index.
WHILE (1=1)
    BEGIN;
        FETCH NEXT
           FROM rebuild_index
           INTO @pid, @objectid, @indexid, @partitionnum, @frag, @page_count, @index_type_desc;
           
        IF @@FETCH_STATUS < 0 BREAK;
        
        SELECT @objectname = QUOTENAME(o.name), @schemaname = QUOTENAME(s.name), @RowCnt=t.RowCnt
        FROM sys.objects AS o
        JOIN sys.schemas as s ON s.schema_id = o.schema_id
        join dba.dbo.reindex_daily_row_count_daily t
        on t.table_id=@objectid
        and [db_name]=@dbname_
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
 --       PRINT N'Executed: ' + @command;
		--insert record
		insert into dba.dbo.index_update(database_id, [schema_id], table_id, index_id,  tablename, indexname, 
			row_count, partition_number, index_date, index_dt, update_type, start_time, end_time, frag, 
			sql_commond, allocUnitType, indexType, page_count)
		values(DB_ID(), schema_id(), @objectid, @indexid, @objectname, @indexname, 
			@RowCnt, @partitionnum, null, GETDATE(), @update_type, @start_time, GETDATE(), @frag,  --1 for  REBUILD; 0 for REORGANIZE
			@command, 'null', @index_type_desc, @page_count)
		update dba.dbo.daily_reindex_list set processed=1
			where id=@pid
    END;

-- Close and deallocate the cursor.
CLOSE  rebuild_index;
DEALLOCATE  rebuild_index;

-- Drop the temporary table.
--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily;

--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily_row_count_daily') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily_row_count_daily;	
	
--IF OBJECT_ID('temp_db_backup.dbo.temp_reindex_daily1') IS NOT NULL 
--	DROP TABLE temp_db_backup.dbo.temp_reindex_daily1;	

