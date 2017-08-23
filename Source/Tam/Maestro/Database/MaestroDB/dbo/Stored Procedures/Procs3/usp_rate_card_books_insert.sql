CREATE PROCEDURE usp_rate_card_books_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO rate_card_books
(
	name,
	version,
	rate_card_version_id,
	base_ratings_media_month_id,
	base_universe_media_month_id,
	date_created,
	date_approved,
	effective_date
)
VALUES
(
	@name,
	@version,
	@rate_card_version_id,
	@base_ratings_media_month_id,
	@base_universe_media_month_id,
	@date_created,
	@date_approved,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

