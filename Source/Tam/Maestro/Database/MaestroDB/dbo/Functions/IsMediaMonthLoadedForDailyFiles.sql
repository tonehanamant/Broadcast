CREATE FUNCTION [dbo].[IsMediaMonthLoadedForDailyFiles]
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
	FROM loaded_media_days lmd
	JOIN media_months mm
		ON lmd.date BETWEEN mm.start_date AND mm.end_date	
	WHERE mm.id = @mediaMonthId
	AND lmd.rating_category_id = @ratingCategoryId
	
	SELECT @totalCount = SUM(DATEDIFF(DAY, start_date, DATEADD(DAY, 1, end_date)))
	FROM dbo.media_months mm
	WHERE mm.id = @mediaMonthId

	IF (@loadedCount = @totalCount)
		RETURN 1
	
	RETURN 0
END

