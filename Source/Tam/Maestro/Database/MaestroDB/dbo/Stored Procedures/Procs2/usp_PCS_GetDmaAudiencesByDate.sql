-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/2/2011
-- Modified:	12/10/2013 - Redesigned dma_audiences table and changes real time dependency on external_rating database.
-- Description:	
-- =============================================
-- usp_PCS_GetDmaAudiencesByDate 3,'58',388
CREATE PROCEDURE [dbo].[usp_PCS_GetDmaAudiencesByDate]
	@rating_source_id TINYINT,
	@audience_ids VARCHAR(MAX),
	@media_month_id INT
AS
BEGIN
	DECLARE @total INT;
	DECLARE @rating_category_group_id TINYINT;
	
	SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@rating_source_id)
	
	-- does data already exist?
	SELECT 
		@total = COUNT(1)
	FROM 
		dbo.dma_audiences da (NOLOCK) 
	WHERE 
		da.rating_category_group_id=@rating_category_group_id
		AND da.media_month_id=@media_month_id;
	
	-- if not already calculated for this month use the proc below which will insert data into dma_audiences from external_rating
	IF @total = 0 AND (@rating_category_group_id = 1 OR @rating_category_group_id = 2)
		EXEC external_rating.dbo.usp_ARS_RefreshDmaAudiences @rating_category_group_id, @media_month_id			
		
	SELECT
		da.rating_category_group_id,
		da.media_month_id,
		da.dma_id,
		aa.custom_audience_id 'audience_id',
		SUM(da.universe) 'universe'
	FROM
		dbo.dma_audiences da (NOLOCK)
		JOIN audience_audiences aa ON da.audience_id=aa.rating_audience_id
			AND aa.rating_category_group_id=@rating_category_group_id
	WHERE
		da.rating_category_group_id=@rating_category_group_id
		AND aa.custom_audience_id IN (
			SELECT id FROM dbo.SplitIntegers(@audience_ids)
		)
		AND da.media_month_id=@media_month_id
	GROUP BY
		da.rating_category_group_id,
		da.media_month_id,
		da.dma_id,
		aa.custom_audience_id
END
