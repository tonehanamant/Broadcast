CREATE PROCEDURE [dbo].[usp_materials_select_all]
AS
SELECT
	*
FROM
	dbo.materials WITH(NOLOCK)
