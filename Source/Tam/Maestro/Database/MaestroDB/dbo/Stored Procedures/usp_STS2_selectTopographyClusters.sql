
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyClusters]
	@topographyId int
AS
BEGIN
	SELECT 
		c.id,
		c.name,
		c.cluster_type
	FROM clusters c
	JOIN topographies t on t.id = c.topography_id
	WHERE t.id = @topographyId
END