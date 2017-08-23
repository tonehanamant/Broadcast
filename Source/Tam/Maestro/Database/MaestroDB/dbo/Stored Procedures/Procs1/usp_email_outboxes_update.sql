CREATE PROCEDURE usp_email_outboxes_update
(
	@id		Int,
	@email_profile_id		Int,
	@subject		VarChar(255),
	@body		Text,
	@is_html		Bit,
	@mail_priority		TinyInt,
	@reply_to_email_address		VarChar(255),
	@date_created		DateTime
)
AS
UPDATE email_outboxes SET
	email_profile_id = @email_profile_id,
	subject = @subject,
	body = @body,
	is_html = @is_html,
	mail_priority = @mail_priority,
	reply_to_email_address = @reply_to_email_address,
	date_created = @date_created
WHERE
	id = @id

