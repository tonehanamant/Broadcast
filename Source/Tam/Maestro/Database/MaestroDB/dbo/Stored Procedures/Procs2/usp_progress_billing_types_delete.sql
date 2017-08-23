CREATE PROCEDURE usp_progress_billing_types_delete
(
	@id Int
)
AS
DELETE FROM progress_billing_types WHERE id=@id
