CREATE PROCEDURE [dbo].[usp_traffic_material_flights_delete]
(
	@id Int)
AS
DELETE FROM traffic_material_flights WHERE id=@id
