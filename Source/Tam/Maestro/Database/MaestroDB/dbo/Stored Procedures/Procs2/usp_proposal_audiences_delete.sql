CREATE PROCEDURE usp_proposal_audiences_delete
(
	@proposal_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	proposal_audiences
WHERE
	proposal_id = @proposal_id
 AND
	audience_id = @audience_id
