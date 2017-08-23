
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_maps_delete]
(
	@id Int)
AS
DELETE FROM system_image_sales_model_maps WHERE id=@id

