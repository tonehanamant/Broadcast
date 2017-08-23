
CREATE PROCEDURE [dbo].[usp_sales_model_rating_sources_update]
(
	@sales_model_id		Int,
	@rating_source_id		TinyInt,
	@is_default		Bit
)
AS
UPDATE sales_model_rating_sources SET
	is_default = @is_default
WHERE
	sales_model_id = @sales_model_id AND
	rating_source_id = @rating_source_id

