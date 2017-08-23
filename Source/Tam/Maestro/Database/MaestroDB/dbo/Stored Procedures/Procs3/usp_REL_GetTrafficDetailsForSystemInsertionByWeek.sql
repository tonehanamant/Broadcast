
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXXX		Joe Jacobs
** 04/07/2017	Abdul Sukkur	Added @flight_start_date
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficDetailsForSystemInsertionByWeek]
	@traffic_id int,
	@system_id int,
	@flight_start_date datetime = NULL
	WITH RECOMPILE
AS
BEGIN
      select 
            traffic_orders.traffic_detail_id, 
            sum(traffic_orders.subscribers), 
            networks.code, 
            traffic_orders.daypart_id, 
            traffic_orders.start_date, 
            traffic_orders.end_date,
            sum(traffic_orders.ordered_spot_rate) [Rate], 
            sum(traffic_orders.ordered_spots) / count(traffic_orders.zone_id) [Spots],
            spot_lengths.length,
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
            join uvw_network_universe (NOLOCK) networks  
				on traffic_details.network_id = networks.network_id 
				and (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
            join spot_lengths (NOLOCK)
                on spot_lengths.id = traffic_details.spot_length_id
            join dayparts dp (NOLOCK)  on dp.id = traffic_orders.daypart_id 
			JOIN timespans ts (NOLOCK)  on ts.id = dp.timespan_id
      where 
			traffic_orders.traffic_id = @traffic_id
            and traffic_orders.system_id = @system_id 
            and traffic.id = @traffic_id
            and traffic_orders.ordered_spots > 0
			and traffic_detail_weeks.suspended = 0
			and traffic_orders.active = 1
			AND (@flight_start_date is null or traffic_orders.end_date >= @flight_start_date) 
      group by 
            traffic_orders.traffic_detail_id, 
            networks.code, 
            traffic_orders.daypart_id, 
            traffic_orders.start_date, 
            traffic_orders.end_date,
            spot_lengths.length,
            ts.start_time,
            ts.end_time
	  order by 
		  networks.code, 
		  traffic_orders.daypart_id,
		  traffic_orders.start_date
END
