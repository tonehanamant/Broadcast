CREATE PROCEDURE usp_reel_materials_select_all
AS
SELECT
	*
FROM
	reel_materials WITH(NOLOCK)
