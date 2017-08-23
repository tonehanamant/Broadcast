CREATE PROCEDURE usp_traffic_materials_disposition_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_materials_disposition WITH(NOLOCK)
WHERE
	id = @id
