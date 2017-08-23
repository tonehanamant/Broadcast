CREATE PROCEDURE usp_custom_traffic_plans_delete
(
	@id Int
)
AS
DELETE FROM custom_traffic_plans WHERE id=@id
