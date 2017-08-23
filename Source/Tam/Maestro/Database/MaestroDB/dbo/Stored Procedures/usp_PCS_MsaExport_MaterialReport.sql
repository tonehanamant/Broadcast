-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/17/2015
-- Description:	Used to populate MSA Export file "spots_YYYY-MM.txt"
-- =============================================
-- EXEC usp_PCS_MsaExport_MaterialReport 401
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_MaterialReport]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  
	
	SELECT * FROM media_weeks mw WHERE mw.media_month_id=@media_month_id ORDER BY mw.start_date

	SELECT
		tp.post_setup_advertiser,
		tp.post_setup_product,
		tpp.tam_post_id,
		mw.start_date,
		mmsa.code,
		mmsa.tape_log,
		slmsa.length,
		SUM(tparinw.total_spots) 'total_spots'
	FROM
		proposals p
		JOIN tam_post_proposals tpp ON tpp.posting_plan_proposal_id=p.id
			AND tpp.post_source_code=2
		JOIN tam_post_analysis_reports_isci_network_weeks tparinw ON tparinw.tam_post_proposal_id=tpp.id
			AND tparinw.audience_id=31
		JOIN tam_posts tp ON tp.id=tpp.tam_post_id
		JOIN media_weeks mw ON mw.id=tparinw.media_week_id
		-- this reprsents the posted copy (the :15 for example, this should never be a married ISCI)
		JOIN materials m ON m.id=tparinw.material_id
		-- this reprsents client ISCI (if any)
		LEFT JOIN materials mc ON mc.id=CASE WHEN m.real_material_id IS NOT NULL THEN m.real_material_id ELSE m.id END
		-- check to see if there is an HD to SD substitution defined
		LEFT JOIN materials msd ON msd.code=CASE WHEN mc.code IS NOT NULL THEN SUBSTRING(mc.code, 1, LEN(mc.code)-1) ELSE SUBSTRING(m.code, 1, LEN(m.code)-1) END
		-- final msa determination
		LEFT JOIN materials mmsa ON mmsa.id=CASE WHEN msd.id IS NOT NULL THEN msd.id WHEN mc.id IS NOT NULL THEN mc.id ELSE m.id END
		LEFT JOIN spot_lengths slmsa ON slmsa.id=mmsa.spot_length_id
	WHERE
		p.posting_media_month_id=@media_month_id
	GROUP BY
		tp.post_setup_advertiser,
		tp.post_setup_product,
		tpp.tam_post_id,
		mw.start_date,
		mmsa.code,
		mmsa.tape_log,
		slmsa.length
	ORDER BY
		tp.post_setup_advertiser,
		tp.post_setup_product,
		tpp.tam_post_id,
		mw.start_date
END