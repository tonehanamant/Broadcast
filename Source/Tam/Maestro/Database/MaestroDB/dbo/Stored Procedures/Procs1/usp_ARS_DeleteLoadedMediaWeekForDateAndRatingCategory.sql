
-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/17/2012
-- Description:	Delete a loaded media week for a specific date and rating category code
-- =============================================
CREATE PROCEDURE usp_ARS_DeleteLoadedMediaWeekForDateAndRatingCategory
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

    -- Insert statements for procedure here
	DELETE FROM loaded_media_weeks WHERE media_week_id = @mediaWeekId AND rating_category_id = @ratingCategoryId
END
