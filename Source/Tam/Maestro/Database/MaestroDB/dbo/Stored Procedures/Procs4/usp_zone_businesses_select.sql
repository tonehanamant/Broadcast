CREATE PROCEDURE usp_zone_businesses_select
(
	@zone_id		Int,
	@business_id		Int,
	@type		VarChar(15)
)
AS
SELECT
	*
FROM
	zone_businesses WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	business_id=@business_id
	AND
	type=@type

