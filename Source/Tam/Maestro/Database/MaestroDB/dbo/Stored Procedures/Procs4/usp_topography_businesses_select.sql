CREATE PROCEDURE usp_topography_businesses_select
(
	@topography_id		Int,
	@business_id		Int
)
AS
SELECT
	*
FROM
	topography_businesses WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	business_id=@business_id

