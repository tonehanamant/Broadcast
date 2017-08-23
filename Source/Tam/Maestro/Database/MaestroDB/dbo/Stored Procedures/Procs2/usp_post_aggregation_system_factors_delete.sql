CREATE PROCEDURE usp_post_aggregation_system_factors_delete
(
	@id Int
)
AS
DELETE FROM post_aggregation_system_factors WHERE id=@id
