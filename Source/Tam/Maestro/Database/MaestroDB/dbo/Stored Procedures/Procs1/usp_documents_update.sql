CREATE PROCEDURE usp_documents_update
(
	@id		Int,
	@file_name		VarChar(255),
	@file_size		BigInt,
	@hash		VarChar(63),
	@file_data		VARBINARY(MAX)
)
AS
UPDATE documents SET
	file_name = @file_name,
	file_size = @file_size,
	hash = @hash,
	file_data = @file_data
WHERE
	id = @id

