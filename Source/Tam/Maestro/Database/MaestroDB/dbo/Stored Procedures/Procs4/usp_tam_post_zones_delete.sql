CREATE PROCEDURE [dbo].[usp_tam_post_zones_delete]
(
	@tam_post_proposal_id		Int,
	@zone_id		Int)
AS
DELETE FROM
	dbo.tam_post_zones
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	zone_id = @zone_id
