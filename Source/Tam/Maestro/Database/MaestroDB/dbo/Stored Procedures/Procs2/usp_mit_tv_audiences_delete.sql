CREATE PROCEDURE [dbo].[usp_mit_tv_audiences_delete]
(
	@media_month_id		Int,
	@mit_rating_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	dbo.mit_tv_audiences
WHERE
	media_month_id = @media_month_id
 AND
	mit_rating_id = @mit_rating_id
 AND
	audience_id = @audience_id
