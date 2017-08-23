/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXXX		Joe Jacobs
** 03/13/2017	Abdul Sukkur	Added @flight_start_date
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficDetailsForSystemInsertion] 
	@traffic_id int,
	@system_id int,
	@flight_start_date datetime = NULL
	WITH RECOMPILE
AS
BEGIN
	with a1 (tdid, subs, network, dpid, startdate, enddate, spot_rate, spots, start_time, end_time)
	as
	(
		select traffic_orders.traffic_detail_id, 
			sum(traffic_orders.subscribers), 
			networks.code, 
			traffic_orders.daypart_id, 
			traffic_orders.start_date, 
			traffic_orders.end_date,
			sum(traffic_orders.ordered_spot_rate), 
			sum(traffic_orders.ordered_spots) / count(traffic_orders.zone_id),
			ts.start_time,
			ts.end_time
		from 
			traffic_orders (NOLOCK)
			join traffic_details (NOLOCK) 
				on traffic_details.id = traffic_orders.traffic_detail_id
			join traffic_detail_weeks (NOLOCK) 
				on traffic_detail_weeks.traffic_detail_id = traffic_details.id 
				and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
			join traffic (NOLOCK) 
				on traffic.id = traffic_details.traffic_id
			join networks (NOLOCK) 
				on traffic_details.network_id = networks.id
			join dayparts dp (NOLOCK)  on dp.id = traffic_orders.daypart_id 
			JOIN timespans ts (NOLOCK)  on ts.id = dp.timespan_id
		where 
			traffic_orders.traffic_id = @traffic_id
			and traffic_orders.system_id = @system_id 
			and traffic.id = @traffic_id
			and traffic_detail_weeks.suspended = 0 
			and traffic_orders.active = 1
			and (@flight_start_date is null or traffic_orders.end_date >= @flight_start_date) 
		group by 
			traffic_orders.traffic_detail_id, 
			networks.code, 
			traffic_orders.daypart_id, 
			traffic_orders.start_date, 
			traffic_orders.end_date,
			ts.start_time,
			ts.end_time
	)
	select 
		a1.tdid, 
		a1.subs, 
		a1.network, 
		a1.dpid, 
		min(a1.startdate), 
		max(a1.enddate), 
		a1.spot_rate, 
		sum(a1.spots),
		count(*) [numofweeks],
		a1.start_time,
		a1.end_time
	from
		a1 
	group by
		a1.tdid, 
		a1.subs, 
		a1.network, 
		a1.dpid, 
		a1.spot_rate,
		a1.start_time,
		a1.end_time
	having 
		sum(a1.spots) > 0
	order by 
		a1.network,  
		min(a1.startdate)
END
