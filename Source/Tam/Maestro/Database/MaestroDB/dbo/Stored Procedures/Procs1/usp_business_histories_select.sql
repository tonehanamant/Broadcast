CREATE PROCEDURE usp_business_histories_select
(
	@business_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	business_histories WITH(NOLOCK)
WHERE
	business_id=@business_id
	AND
	start_date=@start_date

