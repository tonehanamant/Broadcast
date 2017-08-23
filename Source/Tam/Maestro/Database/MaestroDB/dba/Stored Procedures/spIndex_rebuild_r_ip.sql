create PROCEDURE [dba].[spIndex_rebuild_r_ip]
@DB_ID int,
@frag_in_percent int,
@Date_Diff int,
@Row_Diff int
AS

SET NOCOUNT ON;


DECLARE @DatabaseID int, @TableName varchar(50), @TableNameOld varchar(50), @IndexID int, @frag real, @StartTime datetime, 
	@EndTime datetime, @Command nvarchar(4000), @statsDate datetime, @rows int, @row_count int, @page_count int, @string varchar(2000),
	@partition_number int, @index_name varchar(100), @index_name_old varchar(100), @index_type_desc varchar(30), @newstatsDate datetime,
	@unrestored_log int, @bk_date smallint
-- check row count change for the table in the reference table dba.dbo.index_update.

--declare @DB_ID int,  @frag_in_percent int, @Date_Diff int, @Row_Diff int
--set @DB_ID=11-- cmw_analysis 6-- maestro 9, postlog 11 and external_rating 18
--set @frag_in_percent=10
--set @Date_Diff=2
--set @Row_Diff=5

--select * from sys.databases

IF OBJECT_ID('tempdb..#temp_index_data') IS NOT NULL DROP TABLE #temp_index_data;

select aa.database_id, object_name(aa.object_id) as table_name, aa.index_id, i.name as index_name,
	ISNULL(STATS_DATE(aa.object_id, aa.index_id), '1970-01-01') as Stat_Date, aa.partition_number, aa.page_count, 
	avg_fragmentation_in_percent, i.rows, isnull(iu.row_count, 0) as row_count, aa.index_type_desc
INTO #temp_index_data
	from (
			select i.database_id, object_id, index_id, index_type_desc, partition_number, page_count,
			avg_fragmentation_in_percent
			from sys.dm_db_index_physical_stats 
						(@DB_ID, NULL, NULL, NULL,  'DETAILED') as i  
			where index_type_desc in ('CLUSTERED INDEX', 'NONCLUSTERED INDEX')
			   -- and index_level=0
				--and i.page_count>300  --take out page_count to index every possible table
				and avg_fragmentation_in_percent >@frag_in_percent
			) as aa
	join sysindexes i 
		on aa.object_id=i.id 
--		and i.indid<2  --55
	left join dba.dbo.index_update as iu
		on iu.database_id=aa.database_id
		and iu.table_id=aa.object_id
		and iu.index_id=aa.index_id
		and iu.partition_number=aa.partition_number
		and iu.start_time<dateadd(dd, -1, getdate())
where i.rows>0


DECLARE Defrag CURSOR FOR 

--declare @Row_Diff smallint
--set @Row_Diff =5
	SELECT max(database_id) as [dbid], table_name, max(index_id) as [indexid], index_name, max(Stat_Date) as [st_date], 
	partition_number, max(page_count) as p_count, max(avg_fragmentation_in_percent) as f_percent, max([rows]) as rows_, 
	max(index_type_desc) as index_type, max(row_count) as row_count
	FROM #temp_index_data
	where index_name is not null
	and table_name is not null
	and partition_number>0
	--and abs([rows]-row_count)/[rows]*100>@Row_Diff 
	group by table_name, index_name, partition_number
	order by 9 asc, table_name asc, index_name asc 

--disable sql agent job to create snapshot
--	select @string='Execute tamsql02.msdb.dba.sp_update_job @job_id = ''' + CAST(job.job_id AS CHAR(36)) + ''', @enabled = 0;' 
--		from tamsql02.msdb.dbo.sysjobs job
--			where job.[name]='load snapshot data'
--			--print @string
--	exec (@string)

--remove tamsql02.maestro snapshot
--	EXECUTE tamsql02.master.dba.sp_drop_snapshot_all 'maestro'
-- Open the cursor.
--set @TableNameOld=''
--set @index_name_old=''

OPEN Defrag;

WHILE (1=1)
	BEGIN
		FETCH NEXT FROM Defrag
			INTO @Databaseid, @TableName, @Indexid, @index_name, @statsDate, @partition_number, 
				@page_count, @frag, @rows, @index_type_desc, @row_count

		IF @@FETCH_STATUS < 0 BREAK;

--	if it is too late 7:00AM during working days break 
--		declare  @bk_date smallint
		select @bk_date=status_int from  dba.dbo.status_
			where id=1
		IF @bk_date=0 BREAK; --print 'OK' 

--		check mirror log
--		truncate table #temp_monitoring
--		
--		check mirrored status
--		if (select count(dm.mirroring_state) from msdb.sys.database_mirroring dm
--			join sys.databases d
--			on dm.database_id=d.database_id
--			where mirroring_state is not null
--			and d.name='maestro')=1
--			begin
--				insert into #temp_monitoring exec msdb..sp_dbmmonitorresults 'maestro' , 0, 1
--				select @unrestored_log=unrestored_log
--					from #temp_monitoring
--			end
--		else
--			set @unrestored_log=0
--
--		while @unrestored_log<50000
--		begin
			--insert log
--			insert into dba.dbo.index_update(database_id, [schema_id], table_id, index_id,  
--				frag, sql_commond,  partition_number, page_count, 
--				tablename, indexname, row_count, index_dt)
--			values(@DatabaseID, 1, object_id(@TableName), @IndexID, @frag, 'wait for mirrored log finish', 
--				@partition_number, @page_count, @TableName, @index_name, @unrestored_log, getdate())
--
--select * from dba.dbo.index_update


			--check log
			--select top 200 * from dba.dbo.index_update where sql_commond='wait for mirrored log finish' order by id desc
			--waitfor
--			WAITFOR DELAY '10:00';

			--update data size
--			truncate table #temp_monitoring
--			if (select count(dm.mirroring_state) from msdb.sys.database_mirroring dm
--				join sys.databases d
--				on dm.database_id=d.database_id
--				where mirroring_state is not null
--				and d.name='maestro')=1
--				begin
--					insert into #temp_monitoring exec msdb..sp_dbmmonitorresults 'maestro' , 0, 1
--					select @unrestored_log=unrestored_log
--						from #temp_monitoring
--				end
--			else
--				set @unrestored_log=0
--		END
		
--			IF @TableName<>@TableNameOld and @index_name<>@index_name_old
--			begin


		if @frag >= 50 or abs(@rows-@row_count)/@rows*100>@Row_Diff  or  @statsDate<=dateadd(dd, -@Date_Diff, getdate()) 
			begin
				SET @Command = N'ALTER INDEX ' + @index_name + N' ON dbo.' + @TableName + N' REBUILD '
				if @partition_number >1
					SET @Command = @Command + N'PARTITION=' + CAST(@partition_number AS nvarchar(10)) + N' WITH (SORT_IN_TEMPDB = ON)';
				else 
					SET @Command = @Command + N'WITH (SORT_IN_TEMPDB = ON)';
				SET @newstatsDate=getdate()
			end
		ELSE
			begin
				set @Command = N'select ''No Re-Build Index ' + @index_name + ' on '+ @TableName + '''' ;
				SET @newstatsDate=@statsDate
			end

		--get start time
			set @StartTime=getdate();
		--rebuild index
			BEGIN TRY
--					print @TableName+' -- ' + cast(object_id(@TableName) as varchar)+' -- ' + @Index_name+' -- ' + cast(@statsDate as varchar)
--						+' -- ' + cast(@frag as varchar)+' -- ' + cast(@rows as varchar)+' -- ' + cast(@row_count as varchar) 
----						+' -- '+ cast(@partition_number as varchar)+ ' -- '+@Command
				EXEC (@Command);
--print @Command
				--get end time
				set @EndTime=getdate();

				insert into dba.dbo.index_update(database_id, [schema_id], table_id, index_id,  
					index_dt, start_time, end_time, frag, sql_commond, row_count, partition_number, page_count, indexType,
					tablename, indexname)
				values(@DatabaseID, 1, object_id(@TableName), @IndexID, @newstatsDate, @StartTime, @EndTime, @frag, @Command, @rows, 
					@partition_number, @page_count, @index_type_desc, @TableName, @index_name)
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
				EXECUTE [dba].[dbo].[spLog_Errors];
			END CATCH;
--				set @TableNameOld=@TableName
--				set @index_name_old=@index_name
--			End
	END

-- Close and deallocate the cursor.
CLOSE Defrag;
DEALLOCATE Defrag;

--Enable sql agent job to create snapshot
--	select @string='Execute tamsql02.msdb.dba.sp_update_job @job_id = ''' + CAST(job.job_id AS CHAR(36)) + ''', @enabled = 1;' 
--		from tamsql02.msdb.dbo.sysjobs job
--			where job.[name]='load snapshot data'
--			--print @string
--		exec (@string)

--drop temp table
IF OBJECT_ID('tempdb..#temp_index_data') IS NOT NULL DROP TABLE #temp_index_data;
--DROP TABLE #temp_monitoring;
