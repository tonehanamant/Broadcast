
CREATE Procedure [dbo].[usp_TCS_GetTrafficRateCardsForDateAndTopography]
(
      @start_date datetime,
      @topography_id int
)
AS
select distinct
      trc.*
from
      traffic_rate_cards trc WITH (NOLOCK)
where
      @start_date between trc.start_date and case when trc.end_date is null then '01/01/2030' else trc.end_date end 
      and topography_id = @topography_id
order by
      trc.start_date desc
