CREATE PROCEDURE [dbo].[usp_sales_models_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.sales_models WITH(NOLOCK)
WHERE
	id = @id
