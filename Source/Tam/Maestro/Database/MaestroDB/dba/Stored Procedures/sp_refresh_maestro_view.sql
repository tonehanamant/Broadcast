--exec [dbo].[sp_refresh_maestro_view] 'rs', 0

CREATE PROCEDURE [dba].[sp_refresh_maestro_view]
@view_type varchar(10),
@delete varchar(10)
as

declare @string_ varchar(2000), @conn_ int,  @total int, @textTimestamp as varchar(63)

--holder of commands
create table ##TempCmd(
	id_ int identity (1,1),
	cmd_ varchar(1000))

--drop current report view
set @string_='insert into ##TempCmd(cmd_) 
		select ''drop view ''+ s.name +''.''+ o.name 
			from sys.objects o 
			join sys.schemas s 
				on s.schema_id=o.schema_id 
				where o.type =''V'' 
					and o.name like '''+@view_type+'_%'''

--print @string_
exec(@string_)
			
--execute drop current report view commands
exec [dba].[sp_refresh_run] 'drop current view'

if @delete <> 1
begin
		--build report view
			insert into ##TempCmd(cmd_)
				select sql_text from dba.dba.maestro_report_view_text
					where view_type=@view_type

		--execute create specific report view commands
		exec [dba].[sp_refresh_run] 'create specific report view'

		--view
		set @string_='insert into ##TempCmd(cmd_)  
				select ''create view '' + s.name + ''.'+@view_type+'_''+ o.name + '' as select * from ''
					+ s.name + ''.''+o.name 
					from sys.objects o
						join sys.schemas s  
						on s.schema_id=o.schema_id
					where o.type in (''U'')
						and o.name not in (select table_name from dba.dba.maestro_report_view_text
												where view_type='''+@view_type+''')
					and s.name =''dbo'''

		print @string_
		exec(@string_)

		--execute commands to create report view
		exec [dba].[sp_refresh_run] 'create report view'
		drop table ##TempCmd

		--print 'create report view'

		--grant permission to reportsa
		exec [dba].[sp_report_permission] 'reportsa', 0
--		drop table ##TempCmd

		if @view_type='rs'
		begin
			--grant permission to report_select
			exec [dba].[sp_report_permission] 'report_select', 0
--			drop table ##TempCmd
		end

		if @view_type='qa'
		begin
			--grant permission to report_qa_test
			exec [dba].[sp_report_permission] 'report_qa_test', 0
--			drop table ##TempCmd
		end
end	