-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/20/2012
-- Description:	Copies hispanic ratings to the next month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CopyHispanicRatingsToNextMonth]
	@monthBase AS VARCHAR(7),
	@ratingCategoryCode AS VARCHAR(15)
AS
BEGIN
	DECLARE
		@monthsAfterBase AS INT,
		@idMonthNext AS INT,
		@ratingCategoryId AS INT;
	
	-- SET @monthBase = '0109';
	SET @monthsAfterBase = 35;
	
	SELECT
		@idMonthNext = mm2.id
	FROM
		media_months mm
		JOIN media_months mm2 ON
			dateadd(month,@monthsAfterBase,dateadd(day, 15, mm.start_date)) BETWEEN mm2.start_date AND mm2.end_date
	WHERE
		mm.media_month = @monthBase;

	SELECT 
		@ratingCategoryId = id
	FROM 
		rating_categories
	WHERE 
		code = @ratingCategoryCode;
	
	--/*
	IF OBJECT_ID('tempdb..##hispanic_ratings') > 0 
		DROP TABLE ##hispanic_ratings;
	
	CREATE TABLE [##hispanic_ratings](
		[base_media_month_id] [INT] NOT NULL,
		[forecast_media_month_id] [INT] NOT NULL,
		[nielsen_network_id] [INT] NOT NULL,
		[audience_id] [INT] NOT NULL,
		[daypart_id] [INT] NOT NULL,
		[audience_usage] [float] NOT NULL,
		[tv_usage] [float] NOT NULL
	);
	
	INSERT INTO
		[##hispanic_ratings](
			[rating_category_id],
			[base_media_month_id],
			[forecast_media_month_id],
			[nielsen_network_id],
			[audience_id],
			[daypart_id],
			[audience_usage],
			[tv_usage]
		)
		SELECT
			r.[rating_category_id],
			r.[base_media_month_id],
			r.[forecast_media_month_id],
			r.[nielsen_network_id],
			r.[audience_id],
			r.[daypart_id],
			r.[audience_usage],
			r.[tv_usage]			
		FROM
			ratings r (NOLOCK)
			JOIN media_months bmm (NOLOCK) ON bmm.id = r.base_media_month_id
			JOIN media_months fmm (NOLOCK) ON fmm.id = r.forecast_media_month_id
			JOIN nielsen_networks nn (NOLOCK) ON nn.id = r.nielsen_network_id
			JOIN dayparts dp (NOLOCK) ON dp.id = r.daypart_id
		WHERE
			nn.code like '%-H'
			AND bmm.media_month = @monthBase
			AND r.rating_category_id = @ratingCategoryId;
	
	DELETE
		ratings
	FROM
		ratings r
		JOIN nielsen_networks (NOLOCK) nn ON nn.id = r.nielsen_network_id
		JOIN media_months (NOLOCK) bmm ON bmm.id = r.base_media_month_id
	WHERE
		nn.code like '%-H'
		AND bmm.id = @idMonthNext
		AND r.rating_category_id = @ratingCategoryId
	
	INSERT INTO ratings(rating_category_id, base_media_month_id, forecast_media_month_id, nielsen_network_id, audience_id, daypart_id, audience_usage, tv_usage)
		SELECT
			r.rating_category_id,
			bmm2.id base_media_month_id,
			fmm2.id forecast_media_month_id,
			r.nielsen_network_id,
			r.audience_id,
			r.daypart_id,
			r.audience_usage,
			r.tv_usage			
		FROM
			[##hispanic_ratings] r (NOLOCK) 
			JOIN dayparts dp (NOLOCK) ON dp.id = r.daypart_id
			JOIN nielsen_networks nn (NOLOCK) ON nn.id = r.nielsen_network_id
			JOIN media_months bmm (NOLOCK) ON bmm.id = r.base_media_month_id
			JOIN media_months bmm2 (NOLOCK) ON bmm2.id = @idMonthNext
			JOIN media_months fmm (NOLOCK) ON fmm.id = r.forecast_media_month_id
			JOIN media_months fmm2 (NOLOCK) ON dbo.Period2FirstOfMonth(fmm2.media_month) BETWEEN dbo.Period2FirstOfMonth(bmm2.media_month) AND dateadd(year,2,dbo.Period2FirstOfMonth(bmm2.media_month))
				AND fmm.month = fmm2.month
				AND fmm2.year BETWEEN bmm.year AND bmm.year+5
		WHERE 
			nn.code like '%-H'
			AND bmm.media_month = @monthBase
			AND r.rating_category_id = @ratingCategoryId;
END
