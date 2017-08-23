CREATE PROCEDURE usp_emails_update
(
	@id		Int,
	@email_type_id		Int,
	@email		VarChar(127),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE emails SET
	email_type_id = @email_type_id,
	email = @email,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

