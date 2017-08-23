CREATE PROCEDURE usp_progress_billing_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO progress_billing_types
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

