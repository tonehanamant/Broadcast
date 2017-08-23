CREATE PROCEDURE usp_material_map_histories_select
(
	@material_map_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	material_map_histories WITH(NOLOCK)
WHERE
	material_map_id=@material_map_id
	AND
	start_date=@start_date

