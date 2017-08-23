CREATE PROCEDURE [dbo].[usp_system_custom_traffic_select_by_date]
(
	 @system_id int
	,@effective_date datetime
)
AS
BEGIN

SELECT [system_id]
      ,[traffic_factor]
      ,[effective_date] = [start_date]
FROM 
	[dbo].[uvw_system_custom_traffic_universe] vw WITH (NOLOCK)
WHERE
	[system_id] = @system_id
	AND
		(vw.start_date <= @effective_date
		AND
			(vw.end_date >= @effective_date
			OR vw.end_date IS NULL))
END
