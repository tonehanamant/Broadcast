CREATE PROCEDURE usp_email_outbox_details_insert
(
	@id		Int		OUTPUT,
	@email_outbox_id		Int,
	@email_address		VarChar(255),
	@display_name		VarChar(100),
	@num_attempts		TinyInt,
	@status_code		TinyInt,
	@date_sent		DateTime,
	@date_last_attempt		DateTime
)
AS
INSERT INTO email_outbox_details
(
	email_outbox_id,
	email_address,
	display_name,
	num_attempts,
	status_code,
	date_sent,
	date_last_attempt
)
VALUES
(
	@email_outbox_id,
	@email_address,
	@display_name,
	@num_attempts,
	@status_code,
	@date_sent,
	@date_last_attempt
)

SELECT
	@id = SCOPE_IDENTITY()

