CREATE PROCEDURE [dbo].[usp_system_statement_groups_delete]
(
	@id Int
)
AS
DELETE FROM dbo.system_statement_groups WHERE id=@id
