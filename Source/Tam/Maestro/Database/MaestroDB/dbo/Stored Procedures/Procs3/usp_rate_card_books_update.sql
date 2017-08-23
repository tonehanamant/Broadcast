CREATE PROCEDURE usp_rate_card_books_update
(
	@id		Int,
	@name		VarChar(63),
	@version		Int,
	@rate_card_version_id		Int,
	@base_ratings_media_month_id		Int,
	@base_universe_media_month_id		Int,
	@date_created		DateTime,
	@date_approved		DateTime,
	@effective_date		DateTime
)
AS
UPDATE rate_card_books SET
	name = @name,
	version = @version,
	rate_card_version_id = @rate_card_version_id,
	base_ratings_media_month_id = @base_ratings_media_month_id,
	base_universe_media_month_id = @base_universe_media_month_id,
	date_created = @date_created,
	date_approved = @date_approved,
	effective_date = @effective_date
WHERE
	id = @id

