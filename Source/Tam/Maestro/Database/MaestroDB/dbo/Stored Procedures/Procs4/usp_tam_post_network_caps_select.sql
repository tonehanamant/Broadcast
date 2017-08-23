CREATE PROCEDURE [dbo].[usp_tam_post_network_caps_select]
(
	@tam_post_id		Int,
	@network_id		Int
)
AS
SELECT
	*
FROM
	dbo.tam_post_network_caps WITH(NOLOCK)
WHERE
	tam_post_id=@tam_post_id
	AND
	network_id=@network_id
