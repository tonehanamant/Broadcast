CREATE PROCEDURE usp_topography_business_histories_insert
(
	@topography_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
INSERT INTO topography_business_histories
(
	topography_id,
	business_id,
	start_date,
	include,
	end_date
)
VALUES
(
	@topography_id,
	@business_id,
	@start_date,
	@include,
	@end_date
)

