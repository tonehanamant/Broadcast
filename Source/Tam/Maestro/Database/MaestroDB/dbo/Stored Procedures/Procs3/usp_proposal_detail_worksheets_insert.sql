CREATE PROCEDURE usp_proposal_detail_worksheets_insert
(
	@proposal_detail_id		Int,
	@media_week_id		Int,
	@units		Int
)
AS
INSERT INTO proposal_detail_worksheets
(
	proposal_detail_id,
	media_week_id,
	units
)
VALUES
(
	@proposal_detail_id,
	@media_week_id,
	@units
)

