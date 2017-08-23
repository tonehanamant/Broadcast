CREATE PROCEDURE usp_topography_businesses_delete
(
	@topography_id		Int,
	@business_id		Int
)
AS
DELETE FROM topography_businesses WHERE topography_id=@topography_id AND business_id=@business_id
