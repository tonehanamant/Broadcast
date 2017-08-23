CREATE PROCEDURE [dbo].[usp_ARS_AddLoadedMediaDayForDateAndRatingCategory]
	@ratingStartDate DATETIME,
	@ratingCategoryCode VARCHAR(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE
		@date DATE,
		@ratingCategoryId int;
		
	SET @date = CONVERT(date, @ratingStartDate)
	
	SELECT @ratingCategoryId = id 
	FROM rating_categories 
	WHERE code = @ratingCategoryCode

	INSERT INTO loaded_media_days (date, rating_category_id)
	VALUES (@date, @ratingCategoryId)
END

