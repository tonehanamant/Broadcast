CREATE PROCEDURE usp_cmw_traffic_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_traffic WITH(NOLOCK)
WHERE
	id = @id
