CREATE Procedure [dbo].[usp_TCS_GetUniverseAndSpotsForTopographyandDetailID]
      (
            @traffic_detail_id int,
			@start_date datetime,
			@topography_id int
      )

AS

SELECT 
	tdt.universe, 
	tdt.spots 
from 
	traffic_detail_topographies (NOLOCK) tdt 
	join traffic_detail_weeks (NOLOCK) tdw on tdt.traffic_detail_week_id = tdw.id
where 
	tdt.topography_id = @topography_id 
	and tdw.traffic_detail_id = @traffic_detail_id 
	and tdw.start_date = @start_date
