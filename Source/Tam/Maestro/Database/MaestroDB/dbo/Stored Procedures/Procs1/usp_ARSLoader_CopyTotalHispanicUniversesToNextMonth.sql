-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/19/2012
-- Description:	Copies Hispanic universes to the next month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CopyTotalHispanicUniversesToNextMonth]
	@monthBase AS VARCHAR(7),
	@ratingCategoryCode AS VARCHAR(15)
AS
BEGIN
	DECLARE
		@monthsAfterBase as int,
		@idMonthNext as int,
		@ratingCategoryId AS INT;
	
	--set @monthBase = '0109';
	set @monthsAfterBase = 35;
	
	select
		@idMonthNext = mm2.id
	from
		media_months mm (NOLOCK)
		join media_months mm2 (NOLOCK) on
			dateadd(month,@monthsAfterBase,dateadd(day, 15, mm.start_date)) between mm2.start_date and mm2.end_date
	where
		mm.media_month = @monthBase;
	
	--set @monthBase = '0908';
	
	DELETE 
		universes
	FROM
		universes u
		JOIN nielsen_networks nn ON nn.id = u.nielsen_network_id
	WHERE
		u.rating_category_id = @ratingCategoryId
		AND u.base_media_month_id = @idMonthNext
		AND nn.code LIKE 'TotUSH';
				
	INSERT INTO universes (rating_category_id, base_media_month_id, forecast_media_month_id, nielsen_network_id, audience_id, universe)
		SELECT
			u.rating_category_id,
			bmm2.id base_media_month_id,
			fmm2.id forecast_media_month_id,
			u.nielsen_network_id,
			u.audience_id,
			u.universe
		FROM
			universes u
			join nielsen_networks nn on nn.id = u.nielsen_network_id
			join media_months bmm on bmm.id = u.base_media_month_id
				and bmm.id = u.forecast_media_month_id
			join media_months bmm2 on bmm2.id = @idMonthNext
			join media_months fmm2 on dbo.Period2FirstOfMonth(fmm2.media_month) between 
				dbo.Period2FirstOfMonth(bmm2.media_month) 
--				dateadd(month,1,dateadd(year,1,dbo.Period2FirstOfMonth(bmm2.media_month)))
				and dateadd(year,2,dbo.Period2FirstOfMonth(bmm2.media_month))
		WHERE
			u.rating_category_id = @ratingCategoryId
			AND bmm.media_month = @monthBase
			AND nn.code like 'TotUSH'
		ORDER BY
			bmm2.start_date,
			fmm2.start_date,
			nn.code,
			dbo.GetAudienceSortPositionFromID(u.audience_id);
END
