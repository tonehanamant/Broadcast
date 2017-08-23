
CREATE PROCEDURE [dbo].[usp_BS_GetMatchingBroadcastDayparts]
(	
	@daypart_id int,
	@timezone_list varchar(19),
	@mycount int
)
AS

IF(@timezone_list IS NULL)
BEGIN 
	select bdt.broadcast_daypart_id 
	from broadcast_daypart_timezones bdt 
	where bdt.daypart_id = @daypart_id and bdt.timezone_id is null;
END
ELSE
BEGIN
	create table #temp (broadcast_daypart_id int, numtimezones int);

	insert into #temp (broadcast_daypart_id, numtimezones)
	select bdt.broadcast_daypart_id, count(bdt.timezone_id) 
	from broadcast_daypart_timezones bdt left join dbo.splitintegers(@timezone_list) t on t.id = bdt.timezone_id
	where bdt.daypart_id = @daypart_id
	group by
		bdt.broadcast_daypart_id;
	
	select #temp.broadcast_daypart_id from #temp where
	#temp.numtimezones = @mycount;

	drop table #temp;

END
