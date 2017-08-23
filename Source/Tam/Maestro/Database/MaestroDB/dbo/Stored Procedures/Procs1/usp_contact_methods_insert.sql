CREATE PROCEDURE usp_contact_methods_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63)
)
AS
INSERT INTO contact_methods
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

