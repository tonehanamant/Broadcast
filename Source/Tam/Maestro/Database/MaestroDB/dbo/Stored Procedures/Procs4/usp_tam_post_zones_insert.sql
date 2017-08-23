CREATE PROCEDURE [dbo].[usp_tam_post_zones_insert]
(
	@tam_post_proposal_id		Int,
	@zone_id		Int,
	@total_spots		Int
)
AS
INSERT INTO dbo.tam_post_zones
(
	tam_post_proposal_id,
	zone_id,
	total_spots
)
VALUES
(
	@tam_post_proposal_id,
	@zone_id,
	@total_spots
)
