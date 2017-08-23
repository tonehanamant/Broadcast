
create PROCEDURE [dba].[sp_refresh_run]
@run_name varchar(5)
as

declare @string_ varchar(2000), @conn_ int,  @total int, @sn_all int, @textTimestamp as varchar(63)

select @total= count(1) from ##TempCmd
select @sn_all=count(1) from sys.objects where type ='SN' 
set @conn_=0

While (@total>@conn_)
	Begin
		select @string_ =cmd_
			from ##TempCmd
				where id_=@conn_+1
		EXEC (@string_)
		SET @conn_=@conn_+ 1
	end
		
select  @conn_=	count(1) from sys.objects where type ='SN'

	truncate table ##TempCmd

If  @conn_= @total+@sn_all
	truncate table ##TempCmd
else 
	begin
		print @conn_
		print @run_name
		set @textTimestamp = convert(varchar, getdate(), 121);
		raiserror('%s - Load  SN...', 
			0, 
			1, 
			@textTimestamp) with nowait
	end
