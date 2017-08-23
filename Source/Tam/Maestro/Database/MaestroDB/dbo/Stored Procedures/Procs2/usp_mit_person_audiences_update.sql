CREATE PROCEDURE [dbo].[usp_mit_person_audiences_update]
(
	@media_month_id		Int,
	@mit_rating_id		Int,
	@audience_id		Int,
	@type		VarChar(15),
	@usage		Float,
	@effective_date		Date
)
AS
UPDATE dbo.mit_person_audiences SET
	type = @type,
	usage = @usage,
	effective_date = @effective_date
WHERE
	media_month_id = @media_month_id AND
	mit_rating_id = @mit_rating_id AND
	audience_id = @audience_id
