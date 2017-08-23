
CREATE PROCEDURE [dbo].[usp_sales_model_rating_sources_insert]
(
	@sales_model_id		Int,
	@rating_source_id		TinyInt,
	@is_default		Bit
)
AS
INSERT INTO sales_model_rating_sources
(
	sales_model_id,
	rating_source_id,
	is_default
)
VALUES
(
	@sales_model_id,
	@rating_source_id,
	@is_default
)


