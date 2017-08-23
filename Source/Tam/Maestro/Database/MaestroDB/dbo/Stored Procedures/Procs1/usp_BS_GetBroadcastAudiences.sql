CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastAudiences]
AS
BEGIN
	SELECT DISTINCT 
		a.id, 
		a.name, 
		a.range_start, 
		a.range_end
	FROM 
		audiences a (NOLOCK)
	WHERE
		a.id IN (
			SELECT aa.custom_audience_id FROM audience_audiences aa (NOLOCK) WHERE aa.rating_category_group_id=2
		)
	ORDER BY 
		a.id
END
