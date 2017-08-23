
--exec [dba].[usp_dba_partition_update]
CREATE PROCEDURE [dba].[usp_dba_partition_update]
AS

SET NOCOUNT ON

DECLARE @max_base_media_month_id int, @max_value int, @Message nvarchar(500), 
	@count int, @string varchar(2000), @start_dt datetime, @end_dt DATETIME, @max_media_week_id int

--create max of partition key based on current media month + 2
select @max_base_media_month_id=ID+2 from media_months
where [start_date]=(
select end_date + 1 from media_months
where GETDATE() between [start_date] and end_date)

--create max of partition key based on current media week + 8
select @max_media_week_id=ID+8 from dbo.media_weeks
where [start_date]=(
select end_date + 1 from media_months
where GETDATE() between [start_date] and end_date)

--print @max_base_media_month_id

--loop through all partition functions
/*name	function_id
MediaMonthIDRangePFN	65536  done
MediaMonthPFN	65537  done
MediaMonthSmallintPFN	65539  done
MediaMonthIDRatingFN	65541  done
MediaWeekPFN	65542*/

--select * from sys.partition_schemes 
/*name	data_space_id	type	type_desc	is_default	function_id
MediaMonthIDPScheme	65601	PS	PARTITION_SCHEME	0	65536
MediaMonthScheme	65602	PS	PARTITION_SCHEME	0	65537
MediaMonthSmallintScheme	65603	PS	PARTITION_SCHEME	0	65539
MediaMonthIDRatingScheme	65605	PS	PARTITION_SCHEME	0	65541
MediaWeekScheme	65606	PS	PARTITION_SCHEME	0	65542*/

--	MediaMonthIDRangePFN()	
	select @max_value=cast(Max(value) as int) 
		from sys.partition_range_values 
	where function_id=65536
	
	set @count=@max_value
--	print @count
	
	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		select  @start_dt=[start_date], @end_dt=end_date
			from media_months where id= @count

		set @string='ALTER PARTITION FUNCTION MediaMonthIDRangePFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, [events], sql_string, create_dt, min_date, max_date)
		values('maestro', 'partition tables', 'MediaMonthIDRangePFN', @count, 'extend partition range',  @string, getdate(), @start_dt, @end_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthIDPScheme NEXT USED [PARTITION_FILES]'
		exec (@string)
	end 
	
--select * from dba.dbo.partition_key_logs
--order by id desc

if @max_value-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro Database max_base_media_month_id for posted_affidavits is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
		SET @Message=N'Maestro database partition key MediaMonthIDRangePFN auto extend for posted_affidavits. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthIDRangePFN Auto Extend',
	@body=@Message;
	
--MediaMonthPFN()
	select @max_value=cast(Max(value) as int) 
		from sys.partition_range_values 
	where function_id=65537


	set @count=@max_value
--print @max_value
	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		select  @start_dt=[start_date], @end_dt=end_date
			from media_months where id= @count			
	
		set @string='ALTER PARTITION FUNCTION MediaMonthPFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, [events], sql_string, create_dt, min_date, max_date)
		values('maestro', 'affidavits', 'MMediaMonthPFN', @count, 'extend affidavits range',  @string, getdate(), @start_dt,  @end_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro database max_base_media_month_id for affidavits is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
		SET @Message=N'Maestro database partition key MediaMonthPFN for affidavits auto extend. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthPFN Auto Extend',
	@body=@Message;
	
--MediaMonthSmallintPFN
	select @max_value=cast(max(value) as int) 
		from sys.partition_range_values 
	where function_id=65539

	set @count=@max_value

	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		select  @start_dt=[start_date], @end_dt=end_date
			from media_months where id= @count				
	
		set @string='ALTER PARTITION FUNCTION MediaMonthSmallintPFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'small int', 'MediaMonthSmallintPFN', @count, 'extend small int range',  @string, getdate(), @start_dt, @end_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthSmallintScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro database max_base_media_month_id for affidavit_delivery is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
		SET @Message=N'Maestro database partition key MediaMonthSmallintPFN for affidavit_delivery auto extend. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 


EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthSmallintPFN Auto Extend',
	@body=@Message;

--MediaMonthIDRatingFN
	select @max_value=cast(max(value) as int) 
		from sys.partition_range_values 
	where function_id=65541

	set @count=@max_value

	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		select  @start_dt=[start_date], @end_dt=end_date
			from media_months where id= @count				
	
		set @string='ALTER PARTITION FUNCTION MediaMonthIDRatingFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'int', 'MediaMonthIDRatingFN', @count, 'extend int range',  @string, getdate(), @start_dt, @end_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthIDRatingScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro Database max_base_media_month_id for rating is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
		SET @Message=N'Maestro dDatabase partition key MediaMonthIDRatingFN auto extend for rating. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthSmallintPFN Auto Extend',
	@body=@Message;

--MediaWeekPFN	
	select @max_value=cast(max(value) as int) 
		from sys.partition_range_values 
	where function_id=65542

	set @count=@max_value

	while @count-@max_media_week_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		select  @start_dt=[start_date], @end_dt=end_date
			from media_weeks where id= @count				
	
		set @string='ALTER PARTITION FUNCTION MediaWeekPFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'int', 'MediaWeekPFN', @count, 'extend int range',  @string, getdate(), @start_dt, @end_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaWeekScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value-@max_media_week_id>=1
		SET @Message=N'No action required. Maestro Database max_media_week_id for rating is ' + cast(@max_media_week_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
		SET @Message=N'Maestro dDatabase partition key MediaWeekPFN auto extend for rating. Maestro Database max_media_week_id is ' + cast(@max_media_week_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaWeekPFN Auto Extend',
	@body=@Message;


