

CREATE PROCEDURE [dbo].[usp_REL_UpdateSpotsByTraffic]
      @daypart_id int,
      @traffic_detail_id int,
      @spots int,
      @start_date datetime,
      @end_date datetime,
      @topography_id int
AS

-- SET NOCOUNT ON

declare @ldaypart_id int,
      @ltraffic_detail_id int,
      @lspots int,
      @lstart_date datetime,
      @lend_date datetime,
      @ltopography_id int;
      
set @ldaypart_id = @daypart_id;
set @ltraffic_detail_id = @traffic_detail_id;
set @lspots = @spots;
set @lstart_date = @start_date;
set @lend_date = @end_date;
set @ltopography_id = @topography_id;

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

begin

	Select system_id
	into #systems
	from dbo.GetSystemsByTopographyAndDate(@ltopography_id, @lstart_date)

	create nonclustered index IX_SYSTEM_ID on #systems(system_id)

	Select system_id, zone_id, traffic_factor
	into #custom_traffic
	from dbo.udf_GetCustomTrafficAsOf(@lstart_date) ct 

	create nonclustered index IX_SYSTEM_ID_ZONE_ID on #custom_traffic(system_id, zone_id)

	select traffic_orders.id, ct.traffic_factor 
	into #temp_tt
	from 
		dbo.traffic_orders 
		inner join dbo.traffic_details 
			  on traffic_details.id = traffic_orders.traffic_detail_id 
		inner join #systems l1 
			  on traffic_orders.system_id = l1.system_id 
		join dbo.traffic_detail_weeks 
			  on traffic_detail_weeks.traffic_detail_id = traffic_details.id 
					and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join dbo.traffic_detail_topographies 
			  on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
		left join #custom_traffic ct 
			  on    traffic_orders.system_id = ct.system_id  and traffic_orders.zone_id = ct.zone_id 
	where 
		traffic_orders.daypart_id = @ldaypart_id 
		and ( traffic_orders.start_date >= @lstart_date and traffic_orders.end_date <= @lend_date) 
		and traffic_details.id = @ltraffic_detail_id

	update t 
		set t.ordered_spots = round(@lspots * case when tt.traffic_factor is null then 1 else tt.traffic_factor end, 0) 
		from dbo.traffic_orders t
			  join #temp_tt tt
			  on t.id=tt.id

	update traffic_detail_topographies set spots = @lspots 
		from traffic_detail_topographies 
		join traffic_detail_weeks  
			  on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		join traffic_details  
			  on traffic_details.id = traffic_detail_weeks.traffic_detail_id 
	where 
		traffic_detail_topographies.topography_id = @ltopography_id 
		and   (traffic_detail_weeks.start_date >= @lstart_date and traffic_detail_weeks.end_date <= @lend_date) 
		and traffic_details.id = @ltraffic_detail_id 

end
