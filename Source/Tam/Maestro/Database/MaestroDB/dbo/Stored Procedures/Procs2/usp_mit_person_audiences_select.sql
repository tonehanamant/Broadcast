CREATE PROCEDURE [dbo].[usp_mit_person_audiences_select]
(
	@media_month_id		Int,
	@mit_rating_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	dbo.mit_person_audiences WITH(NOLOCK)
WHERE
	media_month_id=@media_month_id
	AND
	mit_rating_id=@mit_rating_id
	AND
	audience_id=@audience_id
