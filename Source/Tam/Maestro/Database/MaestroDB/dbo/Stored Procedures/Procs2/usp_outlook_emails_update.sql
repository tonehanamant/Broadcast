CREATE PROCEDURE usp_outlook_emails_update
(
	@id		Int,
	@email_type_id		Int,
	@email		VarChar(127)
)
AS
UPDATE outlook_emails SET
	email_type_id = @email_type_id,
	email = @email
WHERE
	id = @id

