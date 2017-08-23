CREATE PROCEDURE usp_contact_methods_update
(
	@id		Int,
	@name		VarChar(63)
)
AS
UPDATE contact_methods SET
	name = @name
WHERE
	id = @id

