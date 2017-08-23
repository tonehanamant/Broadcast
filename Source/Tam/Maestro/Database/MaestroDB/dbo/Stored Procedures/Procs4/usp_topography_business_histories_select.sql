CREATE PROCEDURE usp_topography_business_histories_select
(
	@topography_id		Int,
	@business_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	topography_business_histories WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	business_id=@business_id
	AND
	start_date=@start_date

