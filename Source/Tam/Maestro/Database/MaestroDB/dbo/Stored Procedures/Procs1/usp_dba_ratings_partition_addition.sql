
CREATE PROCEDURE [dbo].[usp_dba_ratings_partition_addition]
AS

SET NOCOUNT ON

DECLARE @base_media_month_id int, @row_count int, @Message nvarchar(500), @string varchar(2000), @start_dt datetime

--cursor
DECLARE b_cursor CURSOR FOR 
	select * from (
	select base_media_month_id, count(1) as c_ 
	FROM [dbo].[ratings] with (nolock)
	group by base_media_month_id) as aa
	where base_media_month_id>48
	and base_media_month_id not in 
		(select cast(value as int)
			from sys.partition_range_values 
				where function_id=65536)

OPEN b_cursor
FETCH NEXT FROM b_cursor
	INTO @base_media_month_id, @row_count

WHILE @@FETCH_STATUS = 0
	BEGIN

		--extend range, split range by "add" the new boundary point 
		set @start_dt=getdate()
		set @string='ALTER PARTITION FUNCTION MediaMonthIDRangePFN() SPLIT RANGE ('+ cast(@base_media_month_id as varchar(4)) + ')'

		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'ratings', 'MediaMonthIDRangePFN', @base_media_month_id, 'extend ratings range with row count: ' + cast(@row_count as varchar), 
			 @string, getdate(), @start_dt, getdate()-@start_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthIDPScheme NEXT USED [PRIMARY]'
		exec (@string)

		--sent message
		SET @Message=N'Maestro Database Partition Key Addition. Add new partition value ' + cast(@base_media_month_id as nvarchar(5)) +N'.'; 

		EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
			@subject=N'Maestro Database Partition Key Addition',
			@body=@Message;

		FETCH NEXT FROM b_cursor
		INTO @base_media_month_id, @row_count;
	END;

CLOSE b_cursor;
DEALLOCATE b_cursor;

