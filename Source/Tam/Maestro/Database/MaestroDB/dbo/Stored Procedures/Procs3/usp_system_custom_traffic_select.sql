CREATE PROCEDURE usp_system_custom_traffic_select
(
	@system_id		Int
)
AS
SELECT
	*
FROM
	system_custom_traffic WITH(NOLOCK)
WHERE
	system_id=@system_id

