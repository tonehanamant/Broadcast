CREATE PROCEDURE usp_email_outbox_details_update
(
	@id		Int,
	@email_outbox_id		Int,
	@email_address		VarChar(255),
	@display_name		VarChar(100),
	@num_attempts		TinyInt,
	@status_code		TinyInt,
	@date_sent		DateTime,
	@date_last_attempt		DateTime
)
AS
UPDATE email_outbox_details SET
	email_outbox_id = @email_outbox_id,
	email_address = @email_address,
	display_name = @display_name,
	num_attempts = @num_attempts,
	status_code = @status_code,
	date_sent = @date_sent,
	date_last_attempt = @date_last_attempt
WHERE
	id = @id

