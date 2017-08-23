CREATE PROCEDURE usp_email_outboxes_insert
(
	@id		Int		OUTPUT,
	@email_profile_id		Int,
	@subject		VarChar(255),
	@body		Text,
	@is_html		Bit,
	@mail_priority		TinyInt,
	@reply_to_email_address		VarChar(255),
	@date_created		DateTime
)
AS
INSERT INTO email_outboxes
(
	email_profile_id,
	subject,
	body,
	is_html,
	mail_priority,
	reply_to_email_address,
	date_created
)
VALUES
(
	@email_profile_id,
	@subject,
	@body,
	@is_html,
	@mail_priority,
	@reply_to_email_address,
	@date_created
)

SELECT
	@id = SCOPE_IDENTITY()

