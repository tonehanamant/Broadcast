-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/19/2012
-- Description:	Copies hispanic universes to the next month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CopyHispanicUniversesToNextMonth]
	@monthBase as varchar(7),
	@ratingCategoryCode AS VARCHAR(15)
AS
BEGIN
	SET NOCOUNT ON;

	declare
		@monthsAfterBase as int,
		@idMonthNext as int,
		@ratingCategoryId AS INT;

	set @monthsAfterBase = 35;
	
	select
		@idMonthNext = mm2.id
	from
		media_months mm (NOLOCK)
		join media_months mm2 (NOLOCK) ON dateadd(month,@monthsAfterBase,dateadd(day, 15, mm.start_date)) between mm2.start_date and mm2.end_date
	where
		mm.media_month = @monthBase;

	SELECT 
		@ratingCategoryId = id
	FROM 
		rating_categories
	WHERE 
		code = @ratingCategoryCode;
		
	DELETE 
		universes
	FROM
		universes u
		JOIN nielsen_networks nn (NOLOCK) ON nn.id = u.nielsen_network_id
	WHERE
		u.rating_category_id = @ratingCategoryId
		AND u.base_media_month_id = @idMonthNext
		AND nn.code LIKE '%-H';
	
	INSERT INTO universes(
			rating_category_id,
			base_media_month_id,
			forecast_media_month_id,
			nielsen_network_id,
			audience_id,
			universe
		)
		SELECT
			u.rating_category_id,
			bmm2.id base_media_month_id, 
			fmm2.id forecast_media_month_id, 
			u.nielsen_network_id,
			u.audience_id, 
			u.universe			
		FROM
			universes u (NOLOCK)
			join nielsen_networks nn (NOLOCK) on nn.id = u.nielsen_network_id
			join media_months bmm (NOLOCK) on bmm.id = u.base_media_month_id
			join media_months bmm2 (NOLOCK) on bmm2.id = @idMonthNext
			join media_months fmm (NOLOCK) on fmm.id = u.forecast_media_month_id
			join media_months fmm2 (NOLOCK) on 
				dbo.Period2FirstOfMonth(fmm2.media_month) between 
					dbo.Period2FirstOfMonth(bmm2.media_month) 
	--				dateadd(month,1,dateadd(year,1,dbo.Period2FirstOfMonth(bmm2.media_month)))
					and 
					dateadd(year,2,dbo.Period2FirstOfMonth(bmm2.media_month))
				and
				fmm.month = fmm2.month
				and
				fmm2.year between bmm.year and bmm.year+5
		where
			u.rating_category_id = @ratingCategoryId
			AND bmm.media_month = @monthBase
			AND nn.code like '%-H'
		order by
			bmm2.start_date,
			fmm2.start_date,
			nn.code;
END
