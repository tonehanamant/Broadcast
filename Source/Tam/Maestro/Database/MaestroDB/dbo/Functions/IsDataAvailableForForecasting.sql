-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/18/2012
-- Description:	Checks IF sufficient past data IS available to forecast FROM the month defined by the given date AND for the given 
-- =============================================
CREATE FUNCTION [dbo].[IsDataAvailableForForecasting]
(
	@currentRatingsDate DATETIME,
	@ratingCategoryCode VARCHAR(15)
)
RETURNS BIT
AS
BEGIN
	DECLARE @baseDate DATETIME, @backDate DATETIME, @monthCount INT, @universeCount INT, @ratingsCount INT, @ratingCategoryId INT;

	SELECT @ratingCategoryId = id FROM rating_categories WHERE code = 'NHIMIT'

	SELECT @baseDate = DATEADD(M, -1, DATEADD(D, 15, start_date)) FROM media_months (NOLOCK) WHERE @currentRatingsDate BETWEEN start_date AND end_date;
	SET @backDate = DATEADD(YY, -2, @baseDate);
	
	-- universes
	SELECT 
		@universeCount = COUNT(DISTINCT u.base_media_month_id)
	FROM 
		universes u (NOLOCK)
		JOIN media_months mm (NOLOCK) ON u.base_media_month_id = mm.id
	WHERE 
		rating_category_id = @ratingCategoryId
		AND (start_date BETWEEN @backDate AND @baseDate OR @backDate BETWEEN start_date AND end_date)
	
	-- average_ratings
	SELECT 
		@ratingsCount = COUNT(DISTINCT ar.base_media_month_id)
	FROM 
		average_ratings ar (NOLOCK)
		JOIN media_months mm (NOLOCK) ON ar.base_media_month_id = mm.id
	WHERE 
		rating_category_id = @ratingCategoryId
		AND (start_date BETWEEN @backDate AND @baseDate OR @backDate BETWEEN start_date AND end_date)
		
	IF (@universeCount = 25 AND @ratingsCount = 25)
		return 1
	return 0
END
