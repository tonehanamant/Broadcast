CREATE Procedure [dbo].[usp_TCS_GetOptimizationRulesForTraffic]
(
	@traffic_id int
)
AS

select 
	distinct
	traffic_detail_id,	
	topography_id,
	target_topography_id,
	percentage
from 
	dbo.GetOptimizationRulesForTraffic(@traffic_id)
order by
	topography_id,
	traffic_detail_id,
	target_topography_id
