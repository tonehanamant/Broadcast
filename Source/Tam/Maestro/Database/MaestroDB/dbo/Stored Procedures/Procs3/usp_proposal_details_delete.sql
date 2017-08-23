CREATE PROCEDURE usp_proposal_details_delete
(
	@id Int
)
AS
DELETE FROM proposal_details WHERE id=@id
