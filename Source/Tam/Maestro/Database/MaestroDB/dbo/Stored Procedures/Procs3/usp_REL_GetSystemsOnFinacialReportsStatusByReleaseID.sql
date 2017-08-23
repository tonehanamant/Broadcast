


CREATE PROCEDURE [dbo].[usp_REL_GetSystemsOnFinacialReportsStatusByReleaseID]
      @release_id as int
AS

	select distinct
		systems.id, 
		systems.code,
		(SELECT  TOP 1 WITH ties traffic_orders.on_financial_reports
						 FROM  traffic_orders WITH (NOLOCK)
						 WHERE traffic_orders.system_id = systems.id
						AND traffic_orders.release_id = @release_id
						 GROUP  BY traffic_orders.on_financial_reports
						 ORDER  BY COUNT(*) DESC)
	from
		systems (NOLOCK) 
	order by
		systems.code
