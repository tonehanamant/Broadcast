CREATE PROCEDURE usp_proposal_detail_worksheets_update
(
	@proposal_detail_id		Int,
	@media_week_id		Int,
	@units		Int
)
AS
UPDATE proposal_detail_worksheets SET
	units = @units
WHERE
	proposal_detail_id = @proposal_detail_id AND
	media_week_id = @media_week_id
