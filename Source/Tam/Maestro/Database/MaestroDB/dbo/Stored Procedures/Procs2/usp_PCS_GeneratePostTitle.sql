
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/29/2011
-- Description:	
-- =============================================
--
-- 03/16/2016 - MNorris:	Added columns is_ADU BIT, adu_for VARCHAR(31), daypart varchar(255), agency varchar(255) to results table.
--							Added select to add additional data to results.
-- usp_PCS_GeneratePostTitle '32300, 32032, 32031, 32301, 32029, 32026, 32302, 32160, 32030'
CREATE PROCEDURE [dbo].[usp_PCS_GeneratePostTitle]
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	DECLARE @num_overnight_plans INT

	DECLARE @isADU BIT, @ADUFor varchar(31), @cluster varchar(255), @length int, @multidaypart bit, @daypart varchar(255), @agency int

	SELECT 
		@num_overnight_plans = COUNT(*) 
	FROM 
		proposals p (NOLOCK) 
	WHERE 
		p.id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
		AND p.is_overnight=1

	/*Added 3-16-16*/
	select
		 @isADU = p.is_audience_deficiency_unit_schedule
		,@ADUFor = p.audience_deficiency_unit_for
		,@cluster = ctg.name
		,@multidaypart = dbo.IsMultiDaypartPlan(p.id)
		,@daypart = d.daypart_text
		,@agency = p.agency_company_id
		,@length = sl.length
	from
		proposals p with (nolock)
		left join categories ctg on p.category_id = ctg.id
		left join dayparts d on p.primary_daypart_id = d.id
		left join spot_lengths sl on p.default_spot_length_id = sl.id
	where
		p.id in (select id from dbo.SplitIntegers(@proposal_ids))
	/*End 3-16-16 add*/
	CREATE TABLE #results (advertiser int,product VARCHAR(127),is_overnight BIT,start_date DATETIME,end_date DATETIME, is_ADU BIT, adu_for VARCHAR(31), daypart varchar(255), agency int, cluster varchar(255), length int)
	INSERT INTO #results
		SELECT
			p.advertiser_company_id 'advertiser',
			pr.name 'product',
			CASE WHEN @num_overnight_plans > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END 'is_overnight',
			MIN(mm.start_date) 'start_date',
			MAX(mm.start_date) 'end_date',
			@isADU as is_adu,
			case when ISNULL(@isADU, 0) = 1 then @ADUFor else '' end as adu_for,
			case when @multidaypart = 1 then 'MD' else @daypart end as daypart,
			@agency as agency,
			@cluster as cluster,
			@length as [length] 
		FROM
			proposals p (NOLOCK)
			JOIN products pr (NOLOCK) ON pr.id=p.product_id
			JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
		WHERE
			p.id IN (
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
			)
		GROUP BY
			p.advertiser_company_id,
			pr.name

	IF (SELECT COUNT(*) FROM #results) > 1
		SELECT
			r.advertiser,
			'Various',
			r.is_overnight,
			MIN(mm_start.media_month) 'start_month',
			MAX(mm_end.media_month) 'end_month',
			r.is_ADU,
			r.adu_for,
			r.daypart,
			r.agency,
			r.cluster,
			r.length
		FROM
			#results r
			JOIN media_months mm_start (NOLOCK) ON mm_start.start_date=r.start_date
			JOIN media_months mm_end   (NOLOCK) ON mm_end.start_date=r.end_date
		GROUP BY
			r.advertiser,
			r.is_overnight,
			r.is_ADU,
			r.adu_for,
			r.daypart,
			r.agency,
			r.cluster,
			r.length
	ELSE
		SELECT
			r.advertiser,
			r.product,
			r.is_overnight,
			mm_start.media_month 'start_month',
			mm_end.media_month 'end_month',
			r.is_ADU,
			r.adu_for,
			r.daypart,
			r.agency,
			r.cluster,
			r.length
		FROM
			#results r
			JOIN media_months mm_start (NOLOCK) ON mm_start.start_date=r.start_date
			JOIN media_months mm_end   (NOLOCK) ON mm_end.start_date=r.end_date
		
	DROP TABLE #results;
END
