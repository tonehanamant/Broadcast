CREATE PROCEDURE [dbo].[usp_tam_post_zones_select_all]
AS
SELECT
	*
FROM
	dbo.tam_post_zones WITH(NOLOCK)
