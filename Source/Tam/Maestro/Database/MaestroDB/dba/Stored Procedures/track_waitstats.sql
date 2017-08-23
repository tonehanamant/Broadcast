
create proc [dba].[track_waitstats] (@num_samples int=10,@delaynum int=1,@delaytype 
  nvarchar(10)='minutes')
AS

SET nocount ON
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'waitstats')
   CREATE table waitstats ([wait type] varchar(80),
      requests numeric(20,1),
	  [wait time] numeric (20,1),
	  [signal wait time] numeric(20,1),
	  now datetime default getdate())
ELSE    truncate table waitstats
dbcc sqlperf (waitstats,clear)                            -- Clear out waitstats.
DECLARE @i int,@delay varchar(8),@dt varchar(3),@now datetime,
   @totalwait numeric(20,1),@endtime datetime,@begintime datetime,@hr int,
   @min int,@sec int
SELECT @i = 1
SELECT @dt = case lower(@delaytype)
   WHEN 'minutes' THEN 'm'
   WHEN 'minute' THEN 'm'
   WHEN 'min' THEN 'm'
   WHEN 'mm' THEN 'm'
   WHEN 'mi' THEN 'm'
   WHEN 'm' THEN 'm'
   WHEN 'seconds' THEN 's'
   WHEN 'second' THEN 's'
   WHEN 'sec' THEN 's'
   WHEN 'ss' THEN 's'
   WHEN 's' THEN 's'
   ELSE @delaytype
END
IF @dt NOT IN ('s','m')
BEGIN
   PRINT 'please supply delay type e.g. seconds or minutes'
   RETURN
END
IF @dt = 's'
BEGIN
   SELECT @sec = @delaynum % 60
   SELECT @min = cast((@delaynum / 60) AS int)
   SELECT @hr = cast((@min / 60) AS int)
   SELECT @min = @min % 60
END
IF @dt = 'm'
BEGIN
   SELECT @sec = 0
   SELECT @min = @delaynum % 60
   SELECT @hr = cast((@delaynum / 60) AS int)
END
SELECT @delay= right('0'+ convert(varchar(2),@hr),2) + ':' +
   + right('0'+convert(varchar(2),@min),2) + ':' +
   + right('0'+convert(varchar(2),@sec),2)
IF @hr > 23 or @min > 59 or @sec > 59
BEGIN
   SELECT 'hh:mm:ss delay time cannot > 23:59:59'
   SELECT 'delay interval and type: ' + convert (varchar(10),@delaynum) + ',' + 
     @delaytype + ' converts to ' + @delay
   RETURN
END
WHILE (@i <= @num_samples)
BEGIN
             INSERT INTO waitstats ([wait type], requests, [wait time],
			   [signal wait time])
   EXEC ('dbcc sqlperf(waitstats)')
   SELECT @i = @i + 1
   waitfor delay @delay
END
-- insert report data into dba wait_stats table.
insert into dba.dbo.wait_stats(wait_type, wait_time, percentage)
exec maestro.dba.get_waitstats

--remove wait_time is 0
delete from dba.dbo.wait_stats
where wait_time=0

