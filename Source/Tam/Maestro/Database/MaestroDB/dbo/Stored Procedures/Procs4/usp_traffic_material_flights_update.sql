CREATE PROCEDURE [dbo].[usp_traffic_material_flights_update]
(
	@id		Int,
	@traffic_material_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE traffic_material_flights SET
	traffic_material_id = @traffic_material_id,
	start_date = @start_date,
	end_date = @end_date,
	selected = @selected
WHERE
	id = @id
