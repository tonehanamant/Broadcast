
CREATE PROCEDURE [dbo].[usp_sales_model_rating_sources_select_all]
AS
SELECT
	*
FROM
	sales_model_rating_sources WITH(NOLOCK)

