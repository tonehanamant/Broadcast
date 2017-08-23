CREATE PROCEDURE [dbo].[usp_REL_GetCOXDayparts]
AS
BEGIN
	SELECT 
		dm.id, 
		dm.daypart_id, 
		dm.map_set, 
		dm.map_value 
	FROM 
		daypart_maps dm (NOLOCK)
	WHERE 
		dm.map_set = 'REL_COX'
	ORDER BY
		dm.map_value
END
