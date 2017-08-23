CREATE PROCEDURE usp_topography_businesses_insert
(
	@topography_id		Int,
	@business_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_businesses
(
	topography_id,
	business_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@business_id,
	@include,
	@effective_date
)

