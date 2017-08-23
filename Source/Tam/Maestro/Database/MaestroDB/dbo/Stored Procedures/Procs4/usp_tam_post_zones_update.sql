CREATE PROCEDURE [dbo].[usp_tam_post_zones_update]
(
	@tam_post_proposal_id		Int,
	@zone_id		Int,
	@total_spots		Int
)
AS
UPDATE dbo.tam_post_zones SET
	total_spots = @total_spots
WHERE
	tam_post_proposal_id = @tam_post_proposal_id AND
	zone_id = @zone_id
