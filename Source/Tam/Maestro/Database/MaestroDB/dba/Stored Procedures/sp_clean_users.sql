
--exec [dbo].[sp_clean_users] 'cmw_reporting_dev'

create PROCEDURE [dba].[sp_clean_users] 
@db_name varchar(50)
as

declare @string_ varchar(2000)

--holder of commands
	IF OBJECT_ID('tempdb..##TempCmd') IS NOT NULL DROP TABLE ##TempCmd;
	create table ##TempCmd(
		id_ int identity (1,1),
		cmd_ varchar(1000))

--drop current user
	insert into ##TempCmd(cmd_)
		select distinct 'DROP USER ['+ p.name +']'
			from sys.database_principals p
		where p.type  in ('U', 'S')
			and p.name not in ('dbo', 'guest', 'INFORMATION_SCHEMA', 
								'sys')

--execute 
	exec dba.sp_refresh_run 'drop users'
--select * from  ##TempCmd

--add user
	set @string_='insert into ##TempCmd(cmd_) '+
		'select distinct ''CREATE USER [''+ p.name +''] FOR LOGIN ['' + p.name + '']  WITH DEFAULT_SCHEMA=[dbo]''' +
			' from ' + @db_name + '.sys.database_principals p ' +
		'where p.type  in (''U'', ''S'') '+
			' and p.name not in (select [name] from sys.database_principals)'

--execute commands
	exec (@string_)
--print  @string_
	exec dba.sp_refresh_run 'add users'

--assign access rights based on reference database
if @db_name like 'cmw_reporting_%'
	begin
		insert into ##TempCmd(cmd_)
			select 'EXEC sp_addrolemember N''report_dev'', N''' + p.name +''''
				from sys.database_principals p
			where p.name not like '%sqlreport%'
				and p.type  in ('U', 'S')
				and p.name not in ('dbo', 'guest', 'INFORMATION_SCHEMA', 'sys')

	--execute commands
--	select * from ##TempCmd
		exec dba.sp_refresh_run 'grant access'
	end

IF OBJECT_ID('tempdb..##TempCmd') IS NOT NULL DROP TABLE ##TempCmd;