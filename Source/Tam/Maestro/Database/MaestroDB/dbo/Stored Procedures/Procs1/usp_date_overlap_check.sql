
Create PROCEDURE [dbo].[usp_date_overlap_check]
AS

SET NOCOUNT ON

CREATE TABLE #temp (
id int IDENTITY(1,1) NOT NULL,
zoneid int,
networkid int, 
startdate datetime, 
enddate datetime)

DECLARE @id int, @zoneid int, @networkid int, @startdate datetime, @enddate datetime, @count int, @i smallint

DECLARE sub_cursor CURSOR FOR 
	select count(1) as count_, znh.zone_id, znh.network_id
		from dbo.zone_network_histories znh
		join dbo.zone_network_histories ch
		on znh.zone_id = ch.zone_id 
		and znh.network_id = ch.network_id
	where ((ch.start_date between znh.start_date and znh.end_date
			or ch.end_date between znh.start_date and znh.end_date))
		and (ch.start_date != znh.start_date or ch.end_date != znh.end_date)
	group by znh.zone_id, znh.network_id
	order by znh.zone_id, znh.network_id

OPEN sub_cursor
FETCH NEXT FROM sub_cursor
INTO @count, @zoneid, @networkid

--print cast(@count as varchar)+'; '+cast(@zoneid as varchar) +'; '+cast(@networkid as varchar)

WHILE @@FETCH_STATUS = 0
	BEGIN
	--get data set for zone and network
		insert into #temp(zoneid, networkid, startdate, enddate)
		select zone_id, network_id, [start_date], end_date
			from dbo.zone_network_histories
			where zone_id=@zoneid
			and network_id=@networkid
			order by [start_date] asc
	
		select @count=count(1) from #temp
--		print @count
--		select * from #temp
		set @i=1

	--check over-lap date
		WHILE @i<=@count-1
			begin
				select @enddate=enddate from #temp
					where id=@i
				select @startdate=startdate from #temp
					where id=@i+1
--		print @startdate
				if @startdate < @enddate+1
				begin
					insert into date_overlap_log(zoneid, networkid, startdate, enddate)
						select zhn.zone_id, zhn.network_id, zhn.[start_date], zhn.end_date
							from dbo.zone_network_histories zhn
							join #temp
							on zoneid=zhn.zone_id
							and networkid=zhn.network_id
							and startdate=zhn.[start_date]
							and id=@i
					update zhn set [end_date]=@startdate-1
						from dbo.zone_network_histories zhn
							join #temp
							on zoneid=zhn.zone_id
							and networkid=zhn.network_id
							and startdate=zhn.[start_date]
							and id=@i
--		print 'here'
				end;
			--check next
				set @i=@i+1
			end;
	--reset #temp table and re-insert data
	truncate table #temp;			
	FETCH NEXT FROM sub_cursor
	INTO @count, @zoneid, @networkid
--		print cast(@count as varchar)+'; '+cast(@zoneid as varchar) +'; '+cast(@networkid as varchar)
  END;

CLOSE sub_cursor
DEALLOCATE sub_cursor

IF OBJECT_ID(N'tempdb..#temp', N'U') IS NOT NULL 
	DROP TABLE #temp
