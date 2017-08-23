CREATE PROCEDURE [dbo].[usp_ACS_GetMaterialItems]
AS
SELECT
	id,
	code
FROM
	materials (NOLOCK)
ORDER BY
	code
