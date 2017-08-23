CREATE PROCEDURE usp_proposal_statuses_delete
(
	@id Int
)
AS
DELETE FROM proposal_statuses WHERE id=@id
