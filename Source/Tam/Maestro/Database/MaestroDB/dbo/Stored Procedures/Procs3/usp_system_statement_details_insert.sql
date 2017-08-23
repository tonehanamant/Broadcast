CREATE PROCEDURE usp_system_statement_details_insert
(
	@system_statement_id		Int,
	@date_sent		DateTime,
	@email_outbox_id		Int
)
AS
INSERT INTO system_statement_details
(
	system_statement_id,
	date_sent,
	email_outbox_id
)
VALUES
(
	@system_statement_id,
	@date_sent,
	@email_outbox_id
)

