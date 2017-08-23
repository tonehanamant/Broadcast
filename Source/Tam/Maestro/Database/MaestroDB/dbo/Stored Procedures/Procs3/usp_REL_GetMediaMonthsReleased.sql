

CREATE PROCEDURE [dbo].[usp_REL_GetMediaMonthsReleased]
AS

select distinct media_months.id
	from traffic_orders (NOLOCK)
    join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id 
	join media_months (NOLOCK) on 1=1
    where
		traffic_orders.start_date between media_months.start_date and media_months.end_date 
		and
		traffic_orders.end_date between media_months.start_date and media_months.end_date 
order by media_months.id

