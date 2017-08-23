CREATE PROCEDURE usp_system_statement_details_update
(
	@system_statement_id		Int,
	@date_sent		DateTime,
	@email_outbox_id		Int
)
AS
UPDATE system_statement_details SET
	email_outbox_id = @email_outbox_id
WHERE
	system_statement_id = @system_statement_id AND
	date_sent = @date_sent
