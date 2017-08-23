CREATE PROCEDURE usp_billing_categories_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO billing_categories
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

