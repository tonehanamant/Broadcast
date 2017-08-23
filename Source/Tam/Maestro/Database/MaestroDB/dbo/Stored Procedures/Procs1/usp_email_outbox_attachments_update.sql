CREATE PROCEDURE usp_email_outbox_attachments_update
(
	@id		Int,
	@email_outbox_id		Int,
	@file_size		BigInt,
	@file_name		VarChar(255),
	@data		VARBINARY(MAX)
)
AS
UPDATE email_outbox_attachments SET
	email_outbox_id = @email_outbox_id,
	file_size = @file_size,
	file_name = @file_name,
	data = @data
WHERE
	id = @id

