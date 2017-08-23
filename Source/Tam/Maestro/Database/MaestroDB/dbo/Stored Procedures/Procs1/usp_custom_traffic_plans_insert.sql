CREATE PROCEDURE usp_custom_traffic_plans_insert
(
	@id		Int		OUTPUT,
	@topography_id		Int,
	@release_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO custom_traffic_plans
(
	topography_id,
	release_id,
	date_created,
	date_last_modified
)
VALUES
(
	@topography_id,
	@release_id,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

