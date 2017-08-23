CREATE PROCEDURE usp_system_statement_group_systems_insert
(
	@system_statement_group_id		Int,
	@system_id		Int
)
AS
INSERT INTO system_statement_group_systems
(
	system_statement_group_id,
	system_id
)
VALUES
(
	@system_statement_group_id,
	@system_id
)

