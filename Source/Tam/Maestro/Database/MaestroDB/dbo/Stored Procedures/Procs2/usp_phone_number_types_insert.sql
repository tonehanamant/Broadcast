CREATE PROCEDURE usp_phone_number_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO phone_number_types
(
	name,
	is_default
)
VALUES
(
	@name,
	@is_default
)

SELECT
	@id = SCOPE_IDENTITY()

