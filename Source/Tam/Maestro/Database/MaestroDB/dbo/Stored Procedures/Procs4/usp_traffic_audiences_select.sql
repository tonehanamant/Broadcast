CREATE PROCEDURE usp_traffic_audiences_select
(
	@traffic_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	traffic_audiences WITH(NOLOCK)
WHERE
	traffic_id=@traffic_id
	AND
	audience_id=@audience_id

