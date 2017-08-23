--exec sp_reportsa_permission
--select * from dbo.account_statuses
--exec dba.usp_account_statuses_select 1

create PROCEDURE [dba].[sp_reportsa_permission]
as

declare @string_ varchar(2000), @conn_ int,  @total int
declare @textTimestamp as varchar(63)

--holder of commands
create table #TempCmd(
	id_ int identity (1,1),
	cmd_ varchar(1000))

--add permission for reportsa user

set @string_ ='insert into #TempCmd(cmd_) select ''grant execute on '' + name + '' to reportsa ''' 
		+ ' from sys.objects where type in (''P'', ''FN'',  ''FS'', ''AF'')'
print @string_
EXEC (@string_)


set @string_ ='insert into #TempCmd(cmd_) select ''grant select on '' + name + '' to reportsa '''
		+ ' from sys.objects where type in (''TF'', ''IF'')'
print @string_
EXEC (@string_)

--execute commands
select @total= count(1) from #TempCmd
print @total
set @conn_=1

While (@total>=@conn_)
	Begin
		select @string_ =cmd_
			from #TempCmd
				where id_=@conn_
		EXEC (@string_)
		SET @conn_=@conn_+ 1
	end
	
	set @textTimestamp = convert(varchar, getdate(), 121);
	raiserror('%s - Grant permission...', 
				0, 
				1, 
				@textTimestamp) with nowait;
				
drop table #TempCmd		
