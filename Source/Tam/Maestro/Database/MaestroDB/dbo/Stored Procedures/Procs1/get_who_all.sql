CREATE PROCEDURE [dbo].[get_who_all] 
AS
Begin
	EXEC msdb.dbo.sp_start_job 'get_who_all'
	select * from master.dbo.temp_process_info with (nolock)
	order by 1 asc
end