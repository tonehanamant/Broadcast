CREATE PROCEDURE usp_property_histories_select
(
	@property_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	property_histories WITH(NOLOCK)
WHERE
	property_id=@property_id
	AND
	start_date=@start_date

