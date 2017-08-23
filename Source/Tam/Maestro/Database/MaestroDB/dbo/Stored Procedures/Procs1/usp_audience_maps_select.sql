CREATE PROCEDURE usp_audience_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	audience_maps WITH(NOLOCK)
WHERE
	id = @id
