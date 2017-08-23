CREATE PROCEDURE usp_email_outbox_attachments_insert
(
	@id		Int		OUTPUT,
	@email_outbox_id		Int,
	@file_size		BigInt,
	@file_name		VarChar(255),
	@data		VARBINARY(MAX)
)
AS
INSERT INTO email_outbox_attachments
(
	email_outbox_id,
	file_size,
	file_name,
	data
)
VALUES
(
	@email_outbox_id,
	@file_size,
	@file_name,
	@data
)

SELECT
	@id = SCOPE_IDENTITY()

