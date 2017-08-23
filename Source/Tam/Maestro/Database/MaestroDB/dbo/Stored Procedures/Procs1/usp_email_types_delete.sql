CREATE PROCEDURE usp_email_types_delete
(
	@id Int
)
AS
DELETE FROM email_types WHERE id=@id
