
CREATE PROCEDURE [dbo].[usp_sales_model_rating_sources_delete]
(
	@sales_model_id		Int,
	@rating_source_id		TinyInt)
AS
DELETE FROM
	sales_model_rating_sources
WHERE
	sales_model_id = @sales_model_id
 AND
	rating_source_id = @rating_source_id

