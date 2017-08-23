--exec  [dbo].[sp_report_qa_test_permission]

CREATE PROCEDURE [dbo].[sp_report_qa_test_permission]
as

declare @string_ varchar(2000), @conn_ int,  @total int
declare @textTimestamp as varchar(63)

--holder of commands
create table ##TempCmd(
	id_ int identity (1,1),
	cmd_ varchar(1000))

--add permission for reportuser and Richard user

set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + name + '' to report_qa_test ''' 
		+ ' from sys.objects where type in (''U'', ''V'')
			and (name not like ''rs_%'' or name not like ''dev_%'') 
			and schema_id=1'
--print @string_
EXEC (@string_)

--execute 
exec dbo.sp_refresh_run 'grant U and V'

set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + name + '' to report_qa_test '''
		+ ' from sys.objects where type in (''TF'', ''IF'')'
--print @string_
EXEC (@string_)

--execute 
exec dbo.sp_refresh_run 'grant TF and IF'

--sp and fn
set @string_ ='insert into ##TempCmd(cmd_) select ''grant execute on '' + name + '' to report_qa_test '''
		+ ' from sys.objects where type in (''P'', ''FN'', ''FS'', ''AF'')
			and (name like ''%get%'' or name like ''%select%'')
			and schema_id=1'
--print @string_
EXEC (@string_)

--execute 
exec dbo.sp_refresh_run 'grant P, FN, FS, AF'


--deny

set @string_ ='insert into ##TempCmd(cmd_) select ''DENY all on '' + name + '' to report_qa_test '''
		+ ' from sys.objects where type in (''P'', ''FN'', ''FS'', ''AF'')
			and (name like ''sp_refresh%'' 
			or name like ''%_permission%'' 
			or name in (''get_running_sql'', ''get_who_all'', ''usp_table_index_size''))
			and schema_id=1'
--print @string_
EXEC (@string_)

--execute 
exec dbo.sp_refresh_run 'Deny P, FN, FS, AF'


drop table ##TempCmd		

