--exec [dba].[sp_report_select_permission]
--select * from dbo.account_statuses
--exec dba.usp_account_statuses_select 1

create PROCEDURE [dba].[sp_report_select_permission]
as

declare @string_ varchar(2000), @conn_ int,  @total int
declare @textTimestamp as varchar(63)

--holder of commands
create table ##TempCmd(
	id_ int identity (1,1),
	cmd_ varchar(1000))

--add permission for report_select user

set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + s.name +''.'' + o.name + '' to report_select ''' 
		+ ' from sys.objects o join sys.schemas s on s.schema_id=o.schema_id
			where o.type in (''TF'', ''IF'') and o.name like ''%Get%'''
--print @string_
EXEC (@string_)

set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + s.name +''.'' + o.name + '' to report_select '''
		+ ' from sys.objects o join sys.schemas s on s.schema_id=o.schema_id
		where o.type in (''V'') and (o.name like ''rs_%'' or o.name like ''uvw_%'' or o.name like ''vw_%'')'
--print @string_
EXEC (@string_)

--execute commands
exec [dba].[sp_refresh_run] 'Grant report)select select permission'
				
drop table ##TempCmd		
