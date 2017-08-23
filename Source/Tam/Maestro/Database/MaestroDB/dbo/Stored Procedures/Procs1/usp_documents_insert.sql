CREATE PROCEDURE usp_documents_insert
(
	@id		Int		OUTPUT,
	@file_name		VarChar(255),
	@file_size		BigInt,
	@hash		VarChar(63),
	@file_data		VARBINARY(MAX)
)
AS
INSERT INTO documents
(
	file_name,
	file_size,
	hash,
	file_data
)
VALUES
(
	@file_name,
	@file_size,
	@hash,
	@file_data
)

SELECT
	@id = SCOPE_IDENTITY()

