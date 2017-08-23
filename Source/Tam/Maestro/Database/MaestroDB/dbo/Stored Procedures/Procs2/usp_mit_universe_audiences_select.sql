CREATE PROCEDURE [dbo].[usp_mit_universe_audiences_select]
(
	@media_month_id		Int,
	@mit_universe_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	dbo.mit_universe_audiences WITH(NOLOCK)
WHERE
	media_month_id=@media_month_id
	AND
	mit_universe_id=@mit_universe_id
	AND
	audience_id=@audience_id
