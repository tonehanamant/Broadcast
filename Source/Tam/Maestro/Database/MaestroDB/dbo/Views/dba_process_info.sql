create view dbo.dba_process_info as 
select spid, blocked, waittime, lastwaittype, waitresource, db_name(dbid) as DatabaseName, user_name(uid) as ProcessUserName, 
cpu, physical_io, login_time, last_batch, open_tran, sp.[status],  hostname, [program_name], 
 cmd, nt_domain, nt_username, loginame
from [$(master)].sys.syslogins sl
join [$(master)].sys.sysprocesses sp
on sp.sid=sl.sid
where spid>50