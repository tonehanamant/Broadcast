CREATE PROCEDURE usp_cmw_traffic_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_traffic_details WITH(NOLOCK)
WHERE
	id = @id
