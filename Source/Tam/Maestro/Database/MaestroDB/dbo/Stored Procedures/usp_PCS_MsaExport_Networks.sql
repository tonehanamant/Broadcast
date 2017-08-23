-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "networks.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_Networks]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @media_month_start_date DATETIME;
	DECLARE @network_ids AS TABLE (network_id INT);
	
	-- should always be in the context of 1 media month (but just in case we use MIN with current date fallback)
	SELECT
		@media_month_start_date = ISNULL(MIN(mm.start_date),CAST(GETDATE() AS DATE))
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tppm ON tppm.id=mttp.id
		JOIN proposals p ON p.id=tppm.posting_plan_proposal_id
		JOIN media_months mm ON mm.id=p.posting_media_month_id
			
	SELECT
		n.network_id 'id',
		n.code 'abbr',
		n.name 'name',
		1 'active'
	FROM
		uvw_network_universe n
	WHERE
		n.start_date<=@media_month_start_date AND (n.end_date>=@media_month_start_date OR n.end_date IS NULL)
END