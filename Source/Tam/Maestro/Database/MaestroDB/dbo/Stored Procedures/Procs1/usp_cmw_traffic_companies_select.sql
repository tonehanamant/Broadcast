CREATE PROCEDURE usp_cmw_traffic_companies_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_traffic_companies WITH(NOLOCK)
WHERE
	id = @id
