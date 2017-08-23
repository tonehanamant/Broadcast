
CREATE PROCEDURE [dbo].[usp_tam_post_gap_projections_delete]
(
	@tam_post_id		Int,
	@media_month_id		Int,
	@rate_card_type_id		Int)
AS
DELETE FROM
	tam_post_gap_projections
WHERE
	tam_post_id = @tam_post_id
 AND
	media_month_id = @media_month_id
 AND
	rate_card_type_id = @rate_card_type_id

