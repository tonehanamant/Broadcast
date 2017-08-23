
CREATE PROCEDURE usp_BS_GetBroadcastProposalsForAdvertiser 
(
	@advertiser_id int
)
AS

	CREATE TABLE #temp_versions(proposal_id int, revision int);
	
	INSERT INTO #temp_versions(proposal_id, revision) 
	SELECT 
		bp.original_proposal_id, 
		MAX(bp.version_number) 
	FROM broadcast_proposals bp with (NOLOCK) 
	WHERE 
		bp.advertiser_company_id = @advertiser_id
	GROUP BY
		bp.original_proposal_id;

	SELECT
		bp.advertiser_company_id,
		bp.agency_company_id,
		p.name,
		bp.*		
	from broadcast_proposals bp with (NOLOCK)
	join #temp_versions on #temp_versions.proposal_id = bp.original_proposal_id and #temp_versions.revision = bp.version_number
	left join products p with (NOLOCK) on p.id = bp.product_id
	ORDER BY
		bp.original_proposal_id
		
	DROP TABLE #temp_versions;
