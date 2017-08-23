CREATE PROCEDURE usp_topography_business_histories_update
(
	@topography_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_business_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	business_id = @business_id AND
	start_date = @start_date
