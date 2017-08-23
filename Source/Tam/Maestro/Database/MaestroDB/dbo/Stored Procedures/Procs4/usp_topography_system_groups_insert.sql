CREATE PROCEDURE usp_topography_system_groups_insert
(
	@topography_id		Int,
	@system_group_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_system_groups
(
	topography_id,
	system_group_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@system_group_id,
	@include,
	@effective_date
)

