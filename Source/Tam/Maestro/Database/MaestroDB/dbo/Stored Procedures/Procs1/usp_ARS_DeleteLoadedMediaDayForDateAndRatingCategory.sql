CREATE PROCEDURE [dbo].[usp_ARS_DeleteLoadedMediaDayForDateAndRatingCategory]
	@ratingStartDate DATETIME,
	@ratingCategoryCode VARCHAR(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE
		@date DATE,
		@ratingCategoryId INT;
		
	SET @date = CONVERT(date, @ratingStartDate)

	SELECT @ratingCategoryId = id 
	FROM rating_categories 
	WHERE code = @ratingCategoryCode

	DELETE FROM loaded_media_days WHERE date = @date AND rating_category_id = @ratingCategoryId
END

