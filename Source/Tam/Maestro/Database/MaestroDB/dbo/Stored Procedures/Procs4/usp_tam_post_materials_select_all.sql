CREATE PROCEDURE [dbo].[usp_tam_post_materials_select_all]
AS
SELECT
	*
FROM
	tam_post_materials WITH(NOLOCK)
