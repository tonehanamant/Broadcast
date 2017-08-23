CREATE PROCEDURE usp_topography_systems_insert
(
	@topography_id		Int,
	@system_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_systems
(
	topography_id,
	system_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@system_id,
	@include,
	@effective_date
)

