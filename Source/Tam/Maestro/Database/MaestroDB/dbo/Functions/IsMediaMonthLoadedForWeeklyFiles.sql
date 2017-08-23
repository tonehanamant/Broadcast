CREATE FUNCTION [dbo].[IsMediaMonthLoadedForWeeklyFiles]
(
	@mediaMonthId int,
	@ratingCategoryId int
)
RETURNS bit
AS
BEGIN
	DECLARE 
		@loadedCount int,
		@totalCount int;
	
	SELECT @loadedCount = COUNT(*)
	FROM loaded_media_weeks lmw
		JOIN media_weeks mw
			ON mw.id = lmw.media_week_id
	WHERE mw.media_month_id = @mediaMonthId
	AND lmw.rating_category_id = @ratingCategoryId
	
	SELECT @totalCount = COUNT(*)
	FROM media_weeks mw
	WHERE mw.media_month_id = @mediaMonthId

	IF (@loadedCount = @totalCount)
		RETURN 1
	
	RETURN 0
END

