
-- =============================================
-- Author:		Mike Deaven
-- Create date: 8/30/2012
-- Description:	Generates ratings forecast for given category and media month containing the given date
-- =============================================
CREATE PROCEDURE usp_ARSLoader_WorkflowRatingsForecasting
	@date datetime,
	@ratingCategoryCode varchar(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE
		@ratingCategoryId AS int,
		@id AS int,
		@month AS varchar(5),
		@textTimestamp AS varchar(63);

	SELECT
		@ratingCategoryId = id
	FROM
		rating_categories
	WHERE
		code = @ratingCategoryCode;
	
	SELECT
		@id = id,
		@month = media_month
	FROM
		media_months
	WHERE
		@date between start_date AND end_date;

	-- Forecast
	exec [usp_ARSLoader_GenerateRatingsForecast] @id, @ratingCategoryCode;

	-- Hispanic estimates
	exec [usp_ARSLoader_CopyHispanicUniversesToNextMonth] @month, @ratingCategoryCode;
	exec [usp_ARSLoader_CopyHispanicRatingsToNextMonth] @month, @ratingCategoryCode;
	exec [usp_ARSLoader_CopyTotalHispanicUniversesToNextMonth] @month, @ratingCategoryCode;

	-- C3 Biases
	exec [usp_ARSLoader_CalculateC3Bias] @month;

	-- Delivered ratings
	exec [usp_ARSLoader_DeliveredRatings] @month;
	exec [usp_ARSLoader_InsertDeliveredRatings];

	-- Rotational biases
	exec [usp_ARSLoader_CalculateRotationalBiases] @month, @month, @ratingCategoryId
END
