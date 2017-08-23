
CREATE PROCEDURE [dbo].[usp_default_fast_track_gap_projections_insert]
(
	@media_month_id		Int,
	@rate_card_type_id		Int,
	@gap_projection		Float,
	@lock_date		DateTime
)
AS
INSERT INTO default_fast_track_gap_projections
(
	media_month_id,
	rate_card_type_id,
	gap_projection,
	lock_date
)
VALUES
(
	@media_month_id,
	@rate_card_type_id,
	@gap_projection,
	@lock_date
)

