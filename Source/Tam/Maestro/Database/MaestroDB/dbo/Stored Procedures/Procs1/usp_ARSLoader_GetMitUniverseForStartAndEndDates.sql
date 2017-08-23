
CREATE procedure [dbo].[usp_ARSLoader_GetMitUniverseForStartAndEndDates]
	@StartDate datetime,
	@EndDate datetime,
	@RatingCategoryId int,
	@NielsenNetworkId int
AS
BEGIN
	SELECT  media_month_id ,
	        rating_category_id ,
	        nielsen_network_id ,
	        start_date ,
	        end_date ,
	        id
	FROM mit_universes mu (NOLOCK)
	WHERE mu.start_date = @StartDate 
		AND mu.end_date = @EndDate 
		AND mu.rating_category_id = @RatingCategoryId
		AND mu.nielsen_network_id = @NielsenNetworkId
END

