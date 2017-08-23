CREATE PROCEDURE [dbo].[usp_system_statement_groups_update]
(
	@id		Int,
	@name		VarChar(255),
	@statement_type		TinyInt
)
AS
UPDATE dbo.system_statement_groups SET
	name = @name,
	statement_type = @statement_type
WHERE
	id = @id
