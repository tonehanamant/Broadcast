
CREATE PROCEDURE [dbo].[usp_BS_GetDistinctProposalsByQuarter]
	@theyear INT,
	@quarter INT
AS
BEGIN
	CREATE TABLE #temp_versions(proposal_id int, revision int);
	
	INSERT INTO #temp_versions(proposal_id, revision) 
	SELECT 
		bp.original_proposal_id, 
		MAX(bp.version_number) 
	FROM broadcast_proposals bp with (NOLOCK) 
	JOIN media_months mm (NOLOCK) ON (mm.start_date <= bp.end_date AND mm.end_date >= bp.start_date)
	WHERE 
		mm.year = @theyear
		AND (@quarter IS NULL OR CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter)
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
END
