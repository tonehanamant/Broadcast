CREATE PROCEDURE usp_network_types_update
(
	@id		TinyInt,
	@name		VarChar(31)
)
AS
BEGIN
UPDATE dbo.network_types SET
	name = @name
WHERE
	id = @id

END
