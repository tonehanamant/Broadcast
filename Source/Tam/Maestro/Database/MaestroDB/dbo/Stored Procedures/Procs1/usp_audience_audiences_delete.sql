CREATE PROCEDURE [dbo].[usp_audience_audiences_delete]
(
	@rating_category_group_id		TinyInt,
	@custom_audience_id		Int,
	@rating_audience_id		Int)
AS
DELETE FROM
	dbo.audience_audiences
WHERE
	rating_category_group_id = @rating_category_group_id
 AND
	custom_audience_id = @custom_audience_id
 AND
	rating_audience_id = @rating_audience_id
