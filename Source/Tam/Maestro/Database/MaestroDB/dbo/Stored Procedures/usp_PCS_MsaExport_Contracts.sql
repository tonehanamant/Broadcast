-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "contracts.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_Contracts]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		tp.id,
		p.advertiser_company_id 'adv_id',
		0 'external_con_id',
		0 'external_con_rev_no',
		p.agency_company_id 'agency_id',
		CONVERT(VARCHAR(10), tp.start_date, 120) 'start_date', 
		CONVERT(VARCHAR(10), tp.end_date, 120) 'end_date',
		0 'gross', 
		0 'comm_rate', 
		0 'comm_amt', 
		0 'net',
		p.original_proposal_id 'proposal_id',
		pr.name 'product',
		CAST(p.is_equivalized AS INT) 'equivalized',
		1 'delivery',
		0 'bonus_cap', 
		CASE WHEN p.rating_source_id IN (3,6) THEN 'C3' WHEN p.rating_source_id=1 THEN 'NTI' ELSE 'INVALID' END 'rating_type'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN uvw_display_posts tp ON tp.id=tpp.tam_post_id
		JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
		JOIN products pr ON pr.id=p.product_id
END