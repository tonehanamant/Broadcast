CREATE PROCEDURE usp_proposal_detail_worksheets_delete
(
	@proposal_detail_id		Int,
	@media_week_id		Int)
AS
DELETE FROM
	proposal_detail_worksheets
WHERE
	proposal_detail_id = @proposal_detail_id
 AND
	media_week_id = @media_week_id
