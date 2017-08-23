CREATE PROCEDURE [dbo].[usp_roles_update]
(
	@id		Int,
	@name		VarChar(63),
	@description		VarChar(3071),
	@maestro_composers		VarChar(63)
)
AS
UPDATE dbo.roles SET
	name = @name,
	description = @description,
	maestro_composers = @maestro_composers
WHERE
	id = @id
