CREATE PROCEDURE usp_proposal_types_delete
(
	@id Int
)
AS
DELETE FROM proposal_types WHERE id=@id
