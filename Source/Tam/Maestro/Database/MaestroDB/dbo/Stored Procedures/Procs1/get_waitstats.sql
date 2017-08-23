
--exec [dbo].[get_waitstats]
--truncate table  dba.dbo.wait_stats
--select * from dba.dbo.wait_stats

CREATE proc [dbo].[get_waitstats]
AS

SET nocount ON

DECLARE @now datetime,@totalwait numeric(20,1)
   ,@endtime datetime,@begintime datetime
   ,@hr int,@min int,@sec int

SELECT  @now=max(now),@begintime=min(now),@endtime=max(now)
FROM waitstats WHERE [wait type] = 'Total'

-- Subtract waitfor, sleep, and resource_queue from total.
SELECT @totalwait = sum([wait time]) + 1 FROM waitstats
WHERE [wait type] NOT IN ('WAITFOR','SLEEP','RESOURCE_QUEUE', 'Total', 
  '***total***') AND now = @now

-- Insert adjusted totals and rank by percentage in descending order.
DELETE waitstats WHERE [wait type] = '***total***' AND now = @now
INSERT INTO waitstats SELECT '***total***',0,@totalwait,@totalwait,@now

insert into dba.dbo.wait_stats(date_, server_name, wait_type, wait_time, percentage)
SELECT GETDATE(), 'tamsql04', [wait type],[wait time],percentage=cast (100*[wait time]/@totalwait 
  AS numeric(20,1))
FROM waitstats
WHERE [wait type] NOT IN ('WAITFOR','SLEEP','RESOURCE_QUEUE','Total')
AND now = @now
ORDER BY percentage desc
