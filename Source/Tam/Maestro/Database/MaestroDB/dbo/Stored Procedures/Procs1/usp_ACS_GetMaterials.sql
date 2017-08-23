CREATE PROCEDURE [dbo].[usp_ACS_GetMaterials]
AS
BEGIN
	SELECT
		m.*
	FROM
		materials m (NOLOCK)
	ORDER BY
		m.code
END