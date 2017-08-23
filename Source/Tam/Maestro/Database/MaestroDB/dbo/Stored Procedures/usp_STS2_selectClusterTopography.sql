
CREATE PROCEDURE [dbo].[usp_STS2_selectClusterTopography]
	@cluster_id int
AS
BEGIN
	SELECT 
		t.Id,
		t.Name
	FROM topographies t
	JOIN clusters c on c.topography_Id = t.id
	WHERE c.id = @cluster_id
END