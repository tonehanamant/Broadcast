
CREATE PROCEDURE [dbo].[usp_tam_post_gap_projections_select]
(
	@tam_post_id		Int,
	@media_month_id		Int,
	@rate_card_type_id		Int
)
AS
SELECT
	*
FROM
	tam_post_gap_projections WITH(NOLOCK)
WHERE
	tam_post_id=@tam_post_id
	AND
	media_month_id=@media_month_id
	AND
	rate_card_type_id=@rate_card_type_id

