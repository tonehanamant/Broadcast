
CREATE PROCEDURE [dbo].[usp_tam_post_gap_projections_insert]
(
	@tam_post_id		Int,
	@media_month_id		Int,
	@rate_card_type_id		Int,
	@gap_projection		Float
)
AS
INSERT INTO tam_post_gap_projections
(
	tam_post_id,
	media_month_id,
	rate_card_type_id,
	gap_projection
)
VALUES
(
	@tam_post_id,
	@media_month_id,
	@rate_card_type_id,
	@gap_projection
)

