--exec [dbo].[sp_report_permission] 'reportsa', 0
--exec [dbo].[sp_report_permission] 'report_select', 0
--exec [dbo].[sp_report_permission] 'report_qa_test', 0

CREATE PROCEDURE [dbo].[sp_report_permission]
@role varchar(100),
@delete int 
as

declare @string_ varchar(2000), @conn_ int,  @total int
declare @textTimestamp as varchar(63)

--holder of commands
create table ##TempCmd(
	id_ int identity (1,1),
	cmd_ varchar(1000))

--add permission for reportsa user

if @role='reportsa'
begin
	set @string_ ='insert into ##TempCmd(cmd_) select ''grant execute on '' + s.name +''.'' + o.name + '' to reportsa '''
			+ ' from sys.objects o join sys.schemas s on s.schema_id=o.schema_id
				where o.type in (''P'', ''FN'',  ''FS'', ''AF'')'
	--print @string_
	EXEC (@string_)

	set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + s.name +''.'' + o.name + '' to reportsa '''
			+ ' from sys.objects o join sys.schemas s on s.schema_id=o.schema_id
				where o.type in  (''TF'', ''IF'', ''U'', ''V'')'
	--print @string_
	EXEC (@string_)

	--execute commands
	exec [dbo].[sp_refresh_run] 'grant reportsa'
end

--add permission for report_select user

if @role='report_select'
begin
	--REVOKE permission
	insert into ##TempCmd(cmd_)
		select 'REVOKE '+ dp.permission_name COLLATE DATABASE_DEFAULT +  ' ON ' 
			+  o.name COLLATE DATABASE_DEFAULT + ' TO ' + sp.name COLLATE DATABASE_DEFAULT
			from sys.database_permissions dp
			join sys.database_principals sp
				on dp.grantee_principal_id=sp.principal_id
				and sp.name='report_select'
			join sys.objects o
				on o.object_id=dp.major_id
	EXEC (@string_)

	--execute commands
	exec [dbo].[sp_refresh_run] 'revoke report_select'

	set @string_ ='insert into ##TempCmd(cmd_) select ''grant select on '' + s.name +''.'' + o.name + '' to report_select '''
			+ ' from sys.objects o join sys.schemas s on s.schema_id=o.schema_id
			where o.type in (''V'') and (o.name like ''rs_%'' or o.name like ''uvw_%'' or o.name like ''vw_%'')'
	--print @string_
	EXEC (@string_)

	--execute commands
	exec [dbo].[sp_refresh_run] 'grant report_select'
end

if @role='report_qa_test'
begin
	--REVOKE permission
	insert into ##TempCmd(cmd_)
		select 'REVOKE '+ dp.permission_name COLLATE DATABASE_DEFAULT +  ' ON ' 
			+  o.name COLLATE DATABASE_DEFAULT + ' TO ' + sp.name COLLATE DATABASE_DEFAULT
			from sys.database_permissions dp
			join sys.database_principals sp
				on dp.grantee_principal_id=sp.principal_id
				and sp.name='report_qa_test'
			join sys.objects o
				on o.object_id=dp.major_id
	EXEC (@string_)

	--execute commands
	exec [dbo].[sp_refresh_run] 'revoke report_qa_test'

	--Grant permission
	set @string_ ='insert into ##TempCmd(cmd_) select o.ob_permission +  '' on dbo.'' + o.ob_name + '' to report_qa_test '''
				+ ' from dba.dbo.maestro_report_permission o'
--	print @string_
	EXEC (@string_)

	--execute commands
	exec [dbo].[sp_refresh_run] 'grant report_qa_test'
end
			
drop table ##TempCmd		

--select o.ob_permission +  ' on dbo.' + o.ob_name + ' to report_qa_test '
--			 from dba.dbo.maestro_report_permission o