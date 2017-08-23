CREATE proc [dbo].[get_running_sql] 
as
begin
	set nocount on
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	--WAITFOR TIME '19:00:00'
	--GO
	--WhatSQLIsRunning 
	--forever loop
	declare @i int;
	set @i=0

	while @i<1439
	begin
		insert into dba.dbo.running_sql(monitor_time, start_time, [status], command, dbname, username, 
			wait_type, wait_time, cpu_time, total_elapsed_time, reads, writes, logical_reads, row_count, 
			granted_query_memory, sql_text, num_reads, num_writes, client_net_address)
		select GETDATE() AS monitor_time, s1.start_time, s1.status, s1.command, 
			DB_NAME(s1.database_id) as dbname, user_name(user_id) as username, s1.wait_type, s1.wait_time, s1.cpu_time, s1.total_elapsed_time,
			s1.reads, s1.writes, s1.logical_reads, s1.row_count, s1.granted_query_memory, s2.text as sql_text,
			s3.num_reads, s3.num_writes, s3.client_net_address --into dba.dbo.running_sql
		from sys.dm_exec_requests as s1
		CROSS APPLY sys.dm_exec_sql_text(sql_handle) as s2
		join sys.dm_exec_connections as s3
		on s3.connection_id=s1.connection_id
		where s3.client_net_address!='<local machine>'

		WAITFOR DELAY '00:01:00' 
		
		--delete old record (5 days)
		if datediff(day,(select min(monitor_time) from dba.dbo.running_sql), getdate())>30
			delete from dba.dbo.running_sql
				where monitor_time <getdate()-30
		set @i=@i+1
	end
end