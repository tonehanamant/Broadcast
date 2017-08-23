CREATE PROCEDURE usp_zone_types_update
(
	@id		Int,
	@name		VarChar(63)
)
AS
BEGIN
UPDATE zone_types SET
	name = @name
WHERE
	id = @id

END
