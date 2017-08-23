CREATE PROCEDURE [dbo].[usp_ARS_GetMediaMonthIdAndIfAvailableForForecastingFromDate]
	@date DATETIME,
	@ratingCategoryCode varchar(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE
		@ratingCategoryId AS INT,
		@ratingCategoryGroupId AS INT;

	SELECT @ratingCategoryId = id
	FROM rating_categories
	WHERE code = @ratingCategoryCode;
		
	SELECT @ratingCategoryGroupId = rc.rating_category_group_id
	FROM dbo.rating_categories rc
	WHERE rc.id = @ratingCategoryId
	
	IF (@ratingCategoryGroupId = 1)
	BEGIN
		SELECT mm.id, dbo.IsMediaMonthLoadedForWeeklyFiles(mm.id, @ratingCategoryId)
		FROM media_months mm (NOLOCK)
		WHERE @date BETWEEN mm.start_date AND mm.end_date
	END
	ELSE IF (@ratingCategoryGroupId = 3)
	BEGIN
		SELECT mm.id, dbo.IsMediaMonthLoadedForDailyFiles(mm.id, @ratingCategoryId)
		FROM media_months mm (NOLOCK)
		WHERE @date BETWEEN mm.start_date AND mm.end_date
	END
END

