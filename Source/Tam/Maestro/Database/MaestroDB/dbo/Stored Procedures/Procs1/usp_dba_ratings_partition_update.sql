
--exec [dbo].[usp_dba_ratings_partition_update]
CREATE PROCEDURE [dbo].[usp_dba_ratings_partition_update]
AS

SET NOCOUNT ON

DECLARE @max_base_media_month_id int, @max_value int,  @max_value1 int, @Message nvarchar(500), @count int, @string varchar(2000), @start_dt datetime

--create max of partition key based on current media month + 1
select @max_base_media_month_id=ID+1 from media_months
where [start_date]=(
select end_date + 1 from media_months
where GETDATE() between [start_date] and end_date)

select MAX(media_month_id) from postlog_staging.dbo.post_logs  --380

--	MediaMonthIDRangePFN()	
	select @max_value=cast(value as int) from sys.partition_range_values 
	where boundary_id=(
	select max(boundary_id) 
	from sys.partition_range_values 
	where function_id=65536)

	set @count=@max_value

	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		set @start_dt=getdate()		

		set @string='ALTER PARTITION FUNCTION MediaMonthIDRangePFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'ratings', 'MediaMonthIDRangePFN', @count, 'extend ratings range',  @string, getdate(), @start_dt, getdate()-@start_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthIDPScheme NEXT USED [PARTITION_FILES]'
		exec (@string)
	end 
	
if @max_value-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro Database max_base_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value as nvarchar(5)) +N'.'; 
else 
begin
		SET @Message=N'Maestro Database Partition Key MediaMonthIDRangePFN Auto Extend. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 

--constraint	
--affidavit_deliveries
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_media_month_range]') 
		AND parent_object_id = OBJECT_ID(N'[dbo].[affidavit_deliveries]'))
	ALTER TABLE [dbo].[affidavit_deliveries] DROP CONSTRAINT [p_media_month_range]

	set @string='ALTER TABLE [dbo].[affidavit_deliveries]  
		WITH CHECK ADD  CONSTRAINT [p_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)
	
	ALTER TABLE [dbo].[affidavit_deliveries] CHECK CONSTRAINT [p_media_month_range]

--affidavit_deliveries_out	
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[media_month_range_ad_out]') 
		AND parent_object_id = OBJECT_ID(N'[dbo].[affidavit_deliveries_out]'))
	ALTER TABLE [dbo].[affidavit_deliveries_out] DROP CONSTRAINT [media_month_range_ad_out]

	set @string='ALTER TABLE [dbo].[affidavit_deliveries_out]  
		WITH CHECK ADD  CONSTRAINT [media_month_range_ad_out] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)

	ALTER TABLE [dbo].[affidavit_deliveries_out] CHECK CONSTRAINT [media_month_range_ad_out]

--p_posted_affidavits
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_media_month_range]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits]'))
	ALTER TABLE [dbo].[p_posted_affidavits] DROP CONSTRAINT [p_posted_affidavits_media_month_range]
	
	set @string='ALTER TABLE [dbo].[p_posted_affidavits]  
		WITH CHECK ADD  CONSTRAINT [p_posted_affidavits_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)
	
	ALTER TABLE [dbo].[p_posted_affidavits] CHECK CONSTRAINT [p_posted_affidavits_media_month_range]

--p_posted_affidavits_in
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_in_media_month_range]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_in]'))
	ALTER TABLE [dbo].[p_posted_affidavits_in] DROP CONSTRAINT [p_posted_affidavits_in_media_month_range]

	set @string='ALTER TABLE [dbo].[p_posted_affidavits_in]  
		WITH CHECK ADD  CONSTRAINT [p_posted_affidavits_in_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)
	
	ALTER TABLE [dbo].[p_posted_affidavits_in] CHECK CONSTRAINT [p_posted_affidavits_in_media_month_range]

--p_posted_affidavits_out_of_spec
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_out_of_spec_media_month_range]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_out_of_spec]'))
			
	ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec] DROP CONSTRAINT [p_posted_affidavits_out_of_spec_media_month_range]

	set @string='ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec]  
		WITH CHECK ADD  CONSTRAINT [p_posted_affidavits_out_of_spec_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)
	
	ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec] CHECK CONSTRAINT [p_posted_affidavits_out_of_spec_media_month_range]

--p_posted_affidavits_out_of_spec_in
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_out_of_spec_in_media_month_range]') 
		AND parent_object_id = OBJECT_ID(N'[dbo].[p_posted_affidavits_out_of_spec_in]'))
	ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec_in] DROP CONSTRAINT [p_posted_affidavits_out_of_spec_in_media_month_range]

	set @string='ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec_in]  
		WITH CHECK ADD  CONSTRAINT [p_posted_affidavits_out_of_spec_in_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)
	
	ALTER TABLE [dbo].[p_posted_affidavits_out_of_spec_in] CHECK CONSTRAINT [p_posted_affidavits_out_of_spec_in_media_month_range]
	
end

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthIDRangePFN Auto Extend',
	@body=@Message;
	
--MediaMonthPFN()
	select @max_value1=cast(value as int) from sys.partition_range_values 
		where boundary_id=(
		select max(boundary_id) 
		from sys.partition_range_values 
		where function_id=65537)

	set @count=@max_value1

	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		set @start_dt=getdate()				
	
		set @string='ALTER PARTITION FUNCTION MediaMonthPFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'affidavits', 'MMediaMonthPFN', @count, 'extend affidavits range',  @string, getdate(), @start_dt, getdate()-@start_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value1-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro Database max_base_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value1 as nvarchar(5)) +N'.'; 
else 
begin
		SET @Message=N'Maestro Database Partition Key MediaMonthPFN Auto Extend. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 
		
--constraint
--affidavits
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[media_month_range]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[affidavits]'))
	ALTER TABLE [dbo].[affidavits] DROP CONSTRAINT [media_month_range]

	set @string='ALTER TABLE [dbo].[affidavits]  WITH NOCHECK ADD  CONSTRAINT [media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)

	ALTER TABLE [dbo].[affidavits] CHECK CONSTRAINT [media_month_range]
	
--affidavits_out
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[p_media_month_range_out]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[affidavits_out]'))
	ALTER TABLE [dbo].[affidavits_out] DROP CONSTRAINT [p_media_month_range_out]

	set @string='ALTER TABLE [dbo].[affidavits_out]  WITH CHECK ADD  CONSTRAINT [p_media_month_range_out] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)

	ALTER TABLE [dbo].[affidavits_out] CHECK CONSTRAINT [p_media_month_range_out]	
end	

EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthPFN Auto Extend',
	@body=@Message;
	
--MediaMonthSmallintPFN
	select @max_value1=cast(value as int) from sys.partition_range_values 
		where boundary_id=(
		select max(boundary_id) 
		from sys.partition_range_values 
		where function_id=65539)

	set @count=@max_value1

	while @count-@max_base_media_month_id<1
	begin
		--extend range, split range by "add" the new boundary point 
		set @count=@count+1
		set @start_dt=getdate()				
	
		set @string='ALTER PARTITION FUNCTION MediaMonthSmallintPFN() SPLIT RANGE ('+ cast(@count as varchar(4)) + ')'
		exec (@string)

		--log the events
		insert into dba.dbo.partition_key_logs(database_name, table_name, partition_function, 
			count_, events, sql_string, create_dt, min_date, max_date)
		values('maestro', 'affidavits', 'MediaMonthSmallintPFN', @count, 'extend affidavits range',  @string, getdate(), @start_dt, getdate()-@start_dt )

		--next partition to use 
		set @string='ALTER PARTITION SCHEME MediaMonthSmallintScheme NEXT USED [PARTITION_FILES]'
		exec (@string)	
	end 

if @max_value1-@max_base_media_month_id>=1
		SET @Message=N'No action required. Maestro Database max_base_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
			N' and partition value is ' + cast(@max_value1 as nvarchar(5)) +N'.'; 
else 
begin
		SET @Message=N'Maestro Database Partition Key MediaMonthSmallintPFN Auto Extend. Maestro Database max_media_month_id is ' + cast(@max_base_media_month_id as nvarchar(5)) +
		N' and new partition value is ' + cast(@count as nvarchar(5)) +N'.'; 
		
--constraint
--posted_affidavits_out
	IF  EXISTS (SELECT * FROM sys.check_constraints 
		WHERE object_id = OBJECT_ID(N'[dbo].[posted_affidavits_out_media_month_range]') 
			AND parent_object_id = OBJECT_ID(N'[dbo].[posted_affidavits_out]'))
	ALTER TABLE [dbo].[posted_affidavits_out] DROP CONSTRAINT [posted_affidavits_out_media_month_range]

	set @string='ALTER TABLE [dbo].[posted_affidavits_out]  
		WITH CHECK ADD  CONSTRAINT [posted_affidavits_out_media_month_range] CHECK  (([media_month_id]>=(360) AND [media_month_id]<=('+@max_base_media_month_id+')))'
	exec (@string)	

	ALTER TABLE [dbo].[posted_affidavits_out] CHECK CONSTRAINT [posted_affidavits_out_media_month_range]
end


EXECUTE msdb.dbo.sp_notify_operator @name=N'William Feng',
	@subject=N'Maestro Database Partition Key MediaMonthSmallintPFN Auto Extend',
	@body=@Message;





