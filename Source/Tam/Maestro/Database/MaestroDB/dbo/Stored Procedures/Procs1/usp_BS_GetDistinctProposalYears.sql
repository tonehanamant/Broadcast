
CREATE PROCEDURE usp_BS_GetDistinctProposalYears
AS
SELECT DISTINCT
		[year]
	FROM (
		SELECT DISTINCT
			YEAR(ctf.start_date) 'year'
		FROM
			broadcast_proposal_detail_flights ctf (NOLOCK)

		UNION ALL

		SELECT DISTINCT
			YEAR(ctf.end_date) 'year'
		FROM
			broadcast_proposal_detail_flights ctf (NOLOCK)
	) tmp
	ORDER BY
		[year] DESC
