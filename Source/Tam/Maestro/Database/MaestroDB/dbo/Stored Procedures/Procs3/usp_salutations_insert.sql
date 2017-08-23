CREATE PROCEDURE usp_salutations_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(15),
	@is_default		Bit
)
AS
INSERT INTO salutations
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

