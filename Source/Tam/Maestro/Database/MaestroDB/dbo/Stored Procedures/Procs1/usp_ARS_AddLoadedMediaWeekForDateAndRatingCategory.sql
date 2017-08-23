-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/17/2012
-- Description:	Set a media week as loaded in the loaded_media_weeks table for a specific date and rating category code
-- =============================================
CREATE PROCEDURE usp_ARS_AddLoadedMediaWeekForDateAndRatingCategory
	-- Add the parameters for the stored procedure here
	@ratingStartDate DATETIME,
	@ratingCategoryCode VARCHAR(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE
		@mediaWeekId int,
		@ratingCategoryId int;
		
	SELECT @mediaWeekId = id 
	FROM media_weeks 
	WHERE @ratingStartDate BETWEEN start_date AND end_date
	
	SELECT @ratingCategoryId = id 
	FROM rating_categories 
	WHERE code = @ratingCategoryCode

	INSERT INTO loaded_media_weeks (media_week_id, rating_category_id)
	VALUES (@mediaWeekId, @ratingCategoryId)
END
