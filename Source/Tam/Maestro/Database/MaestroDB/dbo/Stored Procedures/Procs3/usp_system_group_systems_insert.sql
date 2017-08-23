CREATE PROCEDURE usp_system_group_systems_insert
(
	@system_group_id		Int,
	@system_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO system_group_systems
(
	system_group_id,
	system_id,
	effective_date
)
VALUES
(
	@system_group_id,
	@system_id,
	@effective_date
)

