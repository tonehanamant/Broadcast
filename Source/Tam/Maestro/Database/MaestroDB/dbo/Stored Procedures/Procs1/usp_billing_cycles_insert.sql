CREATE PROCEDURE usp_billing_cycles_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(20),
	@is_default		Bit
)
AS
INSERT INTO billing_cycles
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

