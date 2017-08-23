CREATE PROCEDURE usp_system_statement_systems_insert
(
	@system_statement_id		Int,
	@system_id		Int
)
AS
INSERT INTO system_statement_systems
(
	system_statement_id,
	system_id
)
VALUES
(
	@system_statement_id,
	@system_id
)

