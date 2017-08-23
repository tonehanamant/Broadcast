CREATE PROCEDURE [dbo].[usp_RCS_GetTrafficRateCardByTopography]
(
	@topography_id int,
	@effective_date datetime
)
AS

select 
	id 
from 
	traffic_rate_cards (NOLOCK)
where 
	topography_id = @topography_id
	and @effective_date between start_date 
	and case when end_date is null then '1/1/2500' else end_date end
