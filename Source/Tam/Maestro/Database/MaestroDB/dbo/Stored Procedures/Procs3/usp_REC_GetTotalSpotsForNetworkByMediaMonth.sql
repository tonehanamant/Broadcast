
CREATE PROCEDURE [dbo].[usp_REC_GetTotalSpotsForNetworkByMediaMonth]
(
	@network_id as int,
	@media_month_id as int,
	@spot_length_id as int
)

AS

with SumSpots (system_id, spots)
as
(
       select system_id, sum(ordered_spots) 
              from traffic_orders (NOLOCK) 
              join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id 
			  join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
			  join media_months (NOLOCK) on media_months.id = @media_month_id
       where
              traffic_details.network_id = @network_id and traffic_details.spot_length_id = @spot_length_id 
			and traffic_orders.release_id is not null 
			and 
			traffic_orders.start_date between media_months.start_date and media_months.end_date 
			and
			traffic_orders.end_date between media_months.start_date and media_months.end_date 
			and traffic_detail_weeks.suspended = 0
			and traffic_orders.on_financial_reports = 1
       group by system_id
),

AggSpots (spots, spotsbycount)
as
(
       select spots, count(spots) from SumSpots
       group by spots
)

select top 1 spots, max(spotsbycount) 
      from AggSpots 
group by spots
order by max(spotsbycount) DESC
