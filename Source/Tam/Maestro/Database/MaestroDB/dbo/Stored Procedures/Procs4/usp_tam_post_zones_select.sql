CREATE PROCEDURE [dbo].[usp_tam_post_zones_select]
(
	@tam_post_proposal_id		Int,
	@zone_id		Int
)
AS
SELECT
	*
FROM
	dbo.tam_post_zones WITH(NOLOCK)
WHERE
	tam_post_proposal_id=@tam_post_proposal_id
	AND
	zone_id=@zone_id
