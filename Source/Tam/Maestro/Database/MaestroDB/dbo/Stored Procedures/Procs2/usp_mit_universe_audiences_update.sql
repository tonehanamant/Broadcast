CREATE PROCEDURE [dbo].[usp_mit_universe_audiences_update]
(
	@media_month_id		Int,
	@mit_universe_id		Int,
	@audience_id		Int,
	@universe		Float,
	@effective_date		Date
)
AS
UPDATE dbo.mit_universe_audiences SET
	universe = @universe,
	effective_date = @effective_date
WHERE
	media_month_id = @media_month_id AND
	mit_universe_id = @mit_universe_id AND
	audience_id = @audience_id
