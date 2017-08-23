
CREATE PROCEDURE [dbo].[usp_network_rate_card_books_update]
(
	@id		Int,
	@name		VarChar(63),
	@year		Int,
	@quarter		Int,
	@version		Int,
	@media_month_id		Int,
	@sales_model_id		Int,
	@base_ratings_media_month_id		Int,
	@base_coverage_universe_media_month_id		Int,
	@approved_by_employee_id		Int,
	@date_approved		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@rating_source_id		TinyInt
)
AS
UPDATE network_rate_card_books SET
	name = @name,
	year = @year,
	quarter = @quarter,
	version = @version,
	media_month_id = @media_month_id,
	sales_model_id = @sales_model_id,
	base_ratings_media_month_id = @base_ratings_media_month_id,
	base_coverage_universe_media_month_id = @base_coverage_universe_media_month_id,
	approved_by_employee_id = @approved_by_employee_id,
	date_approved = @date_approved,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	rating_source_id = @rating_source_id
WHERE
	id = @id


