CREATE PROCEDURE usp_email_messages_update
(
	@id		Int,
	@subject		VarChar(255),
	@body		VARCHAR(MAX),
	@is_html		Bit,
	@mail_priority		TinyInt,
	@enabled		Bit,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE email_messages SET
	subject = @subject,
	body = @body,
	is_html = @is_html,
	mail_priority = @mail_priority,
	enabled = @enabled,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

