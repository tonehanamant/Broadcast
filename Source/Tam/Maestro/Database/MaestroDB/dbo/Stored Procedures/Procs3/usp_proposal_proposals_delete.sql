CREATE PROCEDURE usp_proposal_proposals_delete
(
	@id Int)
AS
DELETE FROM proposal_proposals WHERE id=@id
