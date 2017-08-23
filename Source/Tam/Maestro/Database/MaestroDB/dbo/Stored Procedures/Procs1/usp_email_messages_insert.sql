CREATE PROCEDURE usp_email_messages_insert
(
	@id		Int		OUTPUT,
	@subject		VarChar(255),
	@body		VARCHAR(MAX),
	@is_html		Bit,
	@mail_priority		TinyInt,
	@enabled		Bit,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO email_messages
(
	subject,
	body,
	is_html,
	mail_priority,
	enabled,
	date_created,
	date_last_modified
)
VALUES
(
	@subject,
	@body,
	@is_html,
	@mail_priority,
	@enabled,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

