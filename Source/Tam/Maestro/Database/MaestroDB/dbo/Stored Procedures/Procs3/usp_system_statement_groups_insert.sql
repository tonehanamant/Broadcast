CREATE PROCEDURE [dbo].[usp_system_statement_groups_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(255),
	@statement_type		TinyInt
)
AS
INSERT INTO dbo.system_statement_groups
(
	name,
	statement_type
)
VALUES
(
	@name,
	@statement_type
)

SELECT
	@id = SCOPE_IDENTITY()
