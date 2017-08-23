-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/2/2014
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetAudiencesByRatingsSource]
	@rating_source_id TINYINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		a.*
	FROM 
		audiences a(NOLOCK)
	WHERE
		a.id IN (
			SELECT DISTINCT 
				aa.custom_audience_id 
			FROM 
				audience_audiences aa (NOLOCK) 
				JOIN rating_categories rc (NOLOCK) ON rc.rating_category_group_id=aa.rating_category_group_id
				JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=rc.id
					AND rsrc.rating_source_id=@rating_source_id
		)
	ORDER BY 
		a.category_code,
		a.sub_category_code,
		a.range_start,
		a.range_end,
		a.name
END
