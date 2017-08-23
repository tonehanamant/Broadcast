CREATE PROCEDURE [dbo].[usp_TCS_GetLatestTrafficIndexMediaMonth]

AS

select top 1 
	media_months.id,
	media_months.year,
	media_months.month,
	media_months.media_month,
	media_months.start_date,
	media_months.end_date
from 
	traffic_index_index (NOLOCK)
	join media_months (NOLOCK) on media_months.id = traffic_index_index.media_month_id
order by
	media_months.start_date DESC
