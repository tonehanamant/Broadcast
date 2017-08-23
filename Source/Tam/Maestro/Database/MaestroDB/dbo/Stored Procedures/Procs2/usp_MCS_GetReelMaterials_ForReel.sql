
CREATE PROCEDURE [dbo].[usp_MCS_GetReelMaterials_ForReel]
	@reel_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		rm.*
	FROM
		reel_materials rm WITH (NOLOCK) 
	WHERE
		rm.reel_id=@reel_id
	ORDER BY
		rm.line_number, rm.cut
END
