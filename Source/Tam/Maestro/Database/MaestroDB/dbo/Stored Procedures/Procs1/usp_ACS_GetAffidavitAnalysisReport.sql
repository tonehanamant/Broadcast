

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_ACS_GetAffidavitAnalysisReport 70, NULL
CREATE PROCEDURE [dbo].[usp_ACS_GetAffidavitAnalysisReport]
	@media_month_id INT,
	@advertiser_company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		CAST(CASE WHEN prev.company_id IS NULL THEN p.company_id ELSE prev.company_id END AS varchar) [AdvertiserId],
		CASE WHEN prev.name IS NULL THEN p.name ELSE prev.name END [Product],
		CASE WHEN mrev.code IS NULL THEN m.code ELSE mrev.code END [Copy],
		n.code [Network],
		mw.week_number [Week],
		mw.start_date [Start Date],
		mw.end_date [End Date],
		MIN(dbo.GetHour(a.air_time)) [Start Hour],
		MAX(dbo.GetHour(a.air_time)) [End Hour],
		COUNT(*) [total_spots],
		SUM((CAST(a.subscribers AS FLOAT) / ad_hh.universe) * ad_hh.audience_usage) [Total HH Imp]
	FROM
		affidavits a (NOLOCK)
		JOIN materials m (NOLOCK) ON m.id=a.material_id
		LEFT JOIN products p (NOLOCK) ON p.id=m.product_id
		LEFT JOIN material_revisions mr ON mr.original_material_id=a.material_id 
		LEFT JOIN materials mrev (NOLOCK) ON mrev.id=mr.revised_material_id
		LEFT JOIN products prev (NOLOCK) ON prev.id=mrev.product_id
		JOIN uvw_network_universe n (NOLOCK) ON n.network_id=a.network_id AND (n.start_date<=a.air_date AND (n.end_date>=a.air_date OR n.end_date IS NULL))
		JOIN media_weeks mw (NOLOCK) ON a.air_date BETWEEN mw.start_date AND mw.end_date
		LEFT JOIN affidavit_deliveries ad_hh (NOLOCK) ON ad_hh.media_month_id=@media_month_id
			AND ad_hh.affidavit_id=a.id 
			AND ad_hh.audience_id=31
			AND ad_hh.rating_source_id=1
	WHERE
		a.media_month_id=@media_month_id
		AND a.status_id=1
		AND (@advertiser_company_id IS NULL 
			OR (
				(p.company_id IS NOT NULL AND p.company_id=@advertiser_company_id) 
					OR 
				(prev.company_id IS NOT NULL AND prev.company_id=@advertiser_company_id)
			)
		)
	GROUP BY
		p.company_id,
		prev.company_id,
		p.name,
		prev.name,
		m.code,
		mrev.code,
		n.code,
		mw.week_number,
		mw.start_date,
		mw.end_date
	ORDER BY
		advertiserid,
		product,
		copy,
		n.code,
		mw.week_number
END
