
CREATE PROCEDURE [dbo].[usp_default_fast_track_gap_projections_delete]
(
	@media_month_id		Int,
	@rate_card_type_id		Int)
AS
DELETE FROM
	default_fast_track_gap_projections
WHERE
	media_month_id = @media_month_id
 AND
	rate_card_type_id = @rate_card_type_id

