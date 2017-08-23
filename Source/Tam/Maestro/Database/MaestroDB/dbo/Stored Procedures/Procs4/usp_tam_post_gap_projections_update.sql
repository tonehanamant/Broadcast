
CREATE PROCEDURE [dbo].[usp_tam_post_gap_projections_update]
(
	@tam_post_id		Int,
	@media_month_id		Int,
	@rate_card_type_id		Int,
	@gap_projection		Float
)
AS
UPDATE tam_post_gap_projections SET
	gap_projection = @gap_projection
WHERE
	tam_post_id = @tam_post_id AND
	media_month_id = @media_month_id AND
	rate_card_type_id = @rate_card_type_id

