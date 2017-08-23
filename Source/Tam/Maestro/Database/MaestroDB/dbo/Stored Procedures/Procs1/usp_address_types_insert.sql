CREATE PROCEDURE usp_address_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(20),
	@is_default		Bit
)
AS
INSERT INTO address_types
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

