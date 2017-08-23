CREATE PROCEDURE usp_custom_traffic_plans_update
(
	@id		Int,
	@topography_id		Int,
	@release_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE custom_traffic_plans SET
	topography_id = @topography_id,
	release_id = @release_id,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

