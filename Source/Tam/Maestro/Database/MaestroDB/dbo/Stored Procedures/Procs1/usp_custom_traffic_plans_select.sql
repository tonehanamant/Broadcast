CREATE PROCEDURE usp_custom_traffic_plans_select
(
	@id Int
)
AS
SELECT
	*
FROM
	custom_traffic_plans WITH(NOLOCK)
WHERE
	id = @id
