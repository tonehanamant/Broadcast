CREATE PROCEDURE [dbo].[usp_roles_delete]
(
	@id Int
)
AS
DELETE FROM dbo.roles WHERE id=@id
