-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/2/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_spwho
	@database VARCHAR(63)
AS
BEGIN
	SET NOCOUNT ON;

    create table #spwho ([spid] [smallint] NOT NULL,	[blocked] [smallint] NOT NULL,	[waittime] [bigint] NOT NULL,	[lastwaittype] [nchar](32) NOT NULL,	[waitresource] [nchar](256) NOT NULL,	[DatabaseName] [nvarchar](128) NULL,	[ProcessUserName] [nvarchar](128) NULL,	[cpu] [int] NOT NULL,	[physical_io] [bigint] NOT NULL,	[login_time] [datetime] NOT NULL,	[last_batch] [datetime] NOT NULL,	[open_tran] [smallint] NOT NULL,	[status] [nchar](30) NOT NULL,	[hostname] [nchar](128) NOT NULL,	[program_name] [nchar](128) NOT NULL,	[cmd] [nchar](16) NOT NULL,	[nt_domain] [nchar](128) NOT NULL,	[nt_username] [nchar](128) NOT NULL,	[loginame] [nchar](128) NOT NULL)
	insert #spwho exec get_who_all
	-- filter/sort query here
	SELECT 
		spid,DatabaseName,hostname,loginame,status,program_name,cmd,blocked,cpu,physical_io,open_tran,waittime,lastwaittype,waitresource
	FROM
		#spwho
	WHERE
		(@database IS NULL OR DatabaseName=@database)
		--and hostname in ('tamservices','tamdev2','tam-pa-sdefusco','tamdev4')
	ORDER BY
		hostname,databasename,program_name

	drop table #spwho;
END
