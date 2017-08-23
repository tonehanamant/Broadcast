
--END CRMWTRF-709-Release Composer - Error when regenerating release for Dish default topography
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[usp_TCS_GetDistinctTopographyDetails]  
 @id INT  
AS  
BEGIN  
 SELECT DISTINCT  
  tdt.traffic_detail_week_id,  
  topography_id,   
  tdt.daypart_id,  
  CAST(0.0 AS FLOAT),  
  universe,   
  tdt.rate,   
  tdt.lookup_rate,
  tdt.ordered_spot_cost,
  tdt.calculated_spot_cost,
  tdt.fixed_spot_cost,
  tdt.spot_cost1,
  tdt.spot_cost2    
 FROM   
  traffic_detail_topographies tdt (NOLOCK)   
  JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id  
 WHERE  
  tdw.traffic_detail_id = @id  
END
