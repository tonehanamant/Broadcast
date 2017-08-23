CREATE PROCEDURE usp_zone_types_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63)
)
AS
BEGIN
INSERT INTO zone_types
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

END
