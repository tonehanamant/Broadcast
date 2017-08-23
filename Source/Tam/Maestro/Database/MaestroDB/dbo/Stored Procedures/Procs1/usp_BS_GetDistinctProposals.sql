

CREATE PROCEDURE usp_BS_GetDistinctProposals 
(
	@proposal_status_id int
)
AS

declare @query varchar(max);

	CREATE TABLE #temp_versions(proposal_id int, revision int);
	
	INSERT INTO #temp_versions(proposal_id, revision) 
	SELECT 
		bp.original_proposal_id, 
		MAX(bp.version_number) 
	FROM broadcast_proposals bp with (NOLOCK) 
	GROUP BY
		bp.original_proposal_id;

SELECT
		bp.advertiser_company_id,
		bp.agency_company_id,
		p.name,
		bp.*		
	from broadcast_proposals bp with (NOLOCK)
	join #temp_versions on #temp_versions.proposal_id = bp.original_proposal_id and #temp_versions.revision = bp.version_number
	left join products p with (NOLOCK) on p.id = bp.product_id WHERE (@proposal_status_id is null or @proposal_status_id = bp.proposal_status_id)
		
	DROP TABLE #temp_versions;
