CREATE PROCEDURE usp_zone_business_histories_select
(
	@zone_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@type		VarChar(15)
)
AS
SELECT
	*
FROM
	zone_business_histories WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	business_id=@business_id
	AND
	start_date=@start_date
	AND
	type=@type

