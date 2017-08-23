CREATE PROCEDURE [dbo].[usp_sales_models_delete]
(
	@id Int
)
AS
DELETE FROM dbo.sales_models WHERE id=@id
