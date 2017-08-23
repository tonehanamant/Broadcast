CREATE PROCEDURE [dbo].[usp_tam_post_network_caps_select_all]
AS
SELECT
	*
FROM
	dbo.tam_post_network_caps WITH(NOLOCK)
