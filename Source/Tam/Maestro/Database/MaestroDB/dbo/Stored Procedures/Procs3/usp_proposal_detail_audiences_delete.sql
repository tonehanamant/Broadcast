CREATE PROCEDURE usp_proposal_detail_audiences_delete
(
	@proposal_detail_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	proposal_detail_audiences
WHERE
	proposal_detail_id = @proposal_detail_id
 AND
	audience_id = @audience_id
