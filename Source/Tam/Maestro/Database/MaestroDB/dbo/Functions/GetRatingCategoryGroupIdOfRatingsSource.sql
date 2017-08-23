-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/15/2013
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetRatingCategoryGroupIdOfRatingsSource(1), dbo.GetRatingCategoryGroupIdOfRatingsSource(5)
CREATE FUNCTION [dbo].[GetRatingCategoryGroupIdOfRatingsSource]
(
	@rating_source_id TINYINT
)
RETURNS TINYINT
AS
BEGIN
	DECLARE @return TINYINT
	
	SET @return = (SELECT TOP 1 rating_category_group_id FROM rating_source_rating_categories rsrc (NOLOCK) JOIN rating_categories rc (NOLOCK) ON rc.id=rsrc.rating_category_id WHERE rsrc.rating_source_id=@rating_source_id)
	
	RETURN @return
END
