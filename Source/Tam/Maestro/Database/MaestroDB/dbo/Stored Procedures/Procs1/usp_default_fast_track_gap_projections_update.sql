
CREATE PROCEDURE [dbo].[usp_default_fast_track_gap_projections_update]
(
	@media_month_id		Int,
	@rate_card_type_id		Int,
	@gap_projection		Float,
	@lock_date		DateTime
)
AS
UPDATE default_fast_track_gap_projections SET
	gap_projection = @gap_projection,
	lock_date = @lock_date
WHERE
	media_month_id = @media_month_id AND
	rate_card_type_id = @rate_card_type_id

