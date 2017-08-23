CREATE PROCEDURE [dbo].[usp_REL_UpdateSpotsByTrafficNetworkNode]
(
      @trafficNetworkNodes TrafficNetworkNode READONLY
)
AS
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
begin
	declare @distinct_dates table (asOfDate datetime)
	declare @distinct_topographies table (topography_id int)
	
	insert into @distinct_dates
		Select distinct start_date
		from @trafficNetworkNodes
	
	insert into @distinct_topographies
		Select distinct topography_id
		from @trafficNetworkNodes
		
	Select 
		d.asOfDate
		system_id, 
		zone_id, 
		traffic_factor
	into #custom_traffic
	from @distinct_dates d
	CROSS APPLY dbo.udf_GetCustomTrafficAsOf(d.asOfDate) ct 
	
	Select  dd.asOfDate, dt.topography_id, system_id
	into #systems
	from @distinct_dates dd
	CROSS APPLY @distinct_topographies dt
	CROSS APPLY dbo.GetSystemsByTopographyAndDate(dt.topography_id, dd.asOfDate)

	create nonclustered index IX_SYSTEM_ID on #systems(system_id)
	
	select tord.id, tnn.spots, ct.traffic_factor 
	into #temp_tt
	from
		dbo.traffic_orders tord inner join 
		dbo.traffic_details td on td.id = tord.traffic_detail_id inner join 
		@trafficNetworkNodes tnn on td.id = tnn.traffic_detail_id inner join 
		#systems s on tord.system_id = s.system_id inner join 
		dbo.traffic_detail_weeks on traffic_detail_weeks.traffic_detail_id = td.id 
					and tord.start_date >= traffic_detail_weeks.start_date 
					and tord.end_date <= traffic_detail_weeks.end_date inner join 
		dbo.traffic_detail_topographies on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id left join 
		#custom_traffic ct on   tord.system_id = ct.system_id  and tord.zone_id = ct.zone_id 
	where 
		tord.daypart_id = tnn.daypart_id 
		and ( tord.start_date >= tnn.start_date and tord.end_date <= tnn.end_date)
		and td.id = tnn.traffic_detail_id

	update t 
		set t.ordered_spots = round(tt.spots * ISNULL(tt.traffic_factor, 1), 0) 
	from 
		dbo.traffic_orders t join 
		#temp_tt tt on t.id=tt.id

	update traffic_detail_topographies set spots = tnn.spots 
	from 
		traffic_detail_topographies join 
		traffic_detail_weeks  on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id join 
		traffic_details  on traffic_details.id = traffic_detail_weeks.traffic_detail_id join 
		@trafficNetworkNodes tnn on traffic_details.id = tnn.traffic_detail_id
	where 
		traffic_detail_topographies.topography_id = tnn.topography_id 
		and   (traffic_detail_weeks.start_date >= tnn.start_date 
			   and traffic_detail_weeks.end_date <= tnn.end_date) 
		--and traffic_details.id = @ltraffic_detail_id 
end
