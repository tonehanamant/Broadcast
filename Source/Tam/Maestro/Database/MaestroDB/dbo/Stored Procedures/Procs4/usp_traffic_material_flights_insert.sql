CREATE PROCEDURE [dbo].[usp_traffic_material_flights_insert]
(
	@id		int		OUTPUT,
	@traffic_material_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO traffic_material_flights
(
	traffic_material_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@traffic_material_id,
	@start_date,
	@end_date,
	@selected
)

SELECT
	@id = SCOPE_IDENTITY()
