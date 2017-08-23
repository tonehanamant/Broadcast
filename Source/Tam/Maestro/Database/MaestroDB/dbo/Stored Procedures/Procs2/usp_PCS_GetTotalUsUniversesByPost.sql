-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/7/2012
-- Description:	Get total US universes applicable to post.
-- =============================================
-- usp_PCS_GetTotalUsUniversesByPost 1000937,'40622'
CREATE PROCEDURE [dbo].[usp_PCS_GetTotalUsUniversesByPost]
	@tam_post_id INT,
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @sales_model_id INT
	DECLARE @ratings_source_id TINYINT
					
	-- either all proposals in post or select proposals
	CREATE TABLE #proposal_ids (proposal_id INT)
	IF @proposal_ids IS NULL
		BEGIN
			INSERT INTO #proposal_ids
				SELECT DISTINCT posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.tam_post_id=@tam_post_id
		END
	ELSE
		BEGIN
			INSERT INTO #proposal_ids
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		END
	
	SET @sales_model_id = (
		SELECT TOP 1 
			psm.sales_model_id 
		FROM 
			tam_post_proposals tpp (NOLOCK) 
			JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=tpp.posting_plan_proposal_id 
		WHERE 
			tpp.tam_post_id=@tam_post_id
			AND tpp.posting_plan_proposal_id IN (
				SELECT proposal_id FROM #proposal_ids
			)
	)
	
	SET @ratings_source_id = (
		SELECT 
			tp.rating_source_id 
		FROM 
			tam_posts tp (NOLOCK) 
		WHERE 
			tp.id=@tam_post_id
	)

	-- months we need
	CREATE TABLE #media_months (media_month_id INT)
	INSERT INTO #media_months
		SELECT DISTINCT p.posting_media_month_id FROM proposals p (NOLOCK) WHERE p.id IN (SELECT proposal_id FROM #proposal_ids)

	-- audiences we need
	CREATE TABLE #audiences (audience_id INT)
	INSERT INTO #audiences
		SELECT DISTINCT pa.audience_id FROM proposal_audiences pa (NOLOCK) WHERE pa.proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	
	DECLARE @default_rating_source_id TINYINT
	SELECT @default_rating_source_id = rs.default_rating_source_id FROM rating_sources rs (NOLOCK) WHERE rs.id=@ratings_source_id

	SELECT
		aa.custom_audience_id 'audience_id',
		u.base_media_month_id 'media_month_id',
		SUM(u.universe) 'universe'
	FROM
		#audiences a
		JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_source_id=CASE WHEN @default_rating_source_id IS NOT NULL THEN @default_rating_source_id ELSE @ratings_source_id END
		JOIN rating_categories rc (NOLOCK) ON rc.id=rsrc.rating_category_id
		JOIN audience_audiences aa ON aa.custom_audience_id=a.audience_id
			AND aa.rating_category_group_id=rc.rating_category_group_id
		CROSS APPLY #media_months mm
		JOIN universes u (NOLOCK) ON u.rating_category_id=rsrc.rating_category_id
			AND u.base_media_month_id=mm.media_month_id
			AND u.base_media_month_id=u.forecast_media_month_id
			AND u.audience_id=aa.rating_audience_id
			AND u.nielsen_network_id = 
				CASE @sales_model_id 
					WHEN 1 THEN 336	-- TotUS
					WHEN 2 THEN 347	-- TotUSH
					WHEN 3 THEN 347	-- TotUSH
					ELSE 336		-- TotUS
				END
	GROUP BY
		aa.custom_audience_id,
		u.base_media_month_id
	
	DROP TABLE #audiences;
	DROP TABLE #media_months;
	DROP TABLE #proposal_ids;
END
