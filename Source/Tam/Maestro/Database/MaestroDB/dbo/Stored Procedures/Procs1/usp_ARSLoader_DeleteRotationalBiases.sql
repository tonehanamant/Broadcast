
-- =============================================
-- Author:		Mike Deaven
-- Create date: 6/10/2014
-- Description:	Delete rotational bias data for a media month and rating category
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_DeleteRotationalBiases]
	@baseMediaMonth varchar(15),
	@ratingCategoryId int
AS
BEGIN
	DECLARE
		@isLive bit,
		@baseMonthID int;

	-- If the rating category is not live, skip everything
	SELECT
		@isLive = is_live
	FROM
		rating_categories
	WHERE
		id = @ratingCategoryId

	IF (@isLive = 0)
		RETURN
	
	SELECT
		@baseMonthID = id
	FROM
		media_months
	WHERE
		media_month = @baseMediaMonth;
		
	DELETE
	FROM dbo.rotational_biases
	WHERE media_month_id = @baseMonthID
		AND rating_category_id = @ratingCategoryId
END

