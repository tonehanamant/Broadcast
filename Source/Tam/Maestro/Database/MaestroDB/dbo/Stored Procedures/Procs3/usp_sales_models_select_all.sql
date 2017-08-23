CREATE PROCEDURE [dbo].[usp_sales_models_select_all]
AS
SELECT
	*
FROM
	dbo.sales_models WITH(NOLOCK)
