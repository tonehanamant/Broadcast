CREATE PROCEDURE usp_topography_businesses_update
(
	@topography_id		Int,
	@business_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_businesses SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	business_id = @business_id
