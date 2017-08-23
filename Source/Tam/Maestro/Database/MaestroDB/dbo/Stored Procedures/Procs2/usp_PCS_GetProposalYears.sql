-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/10/2010
-- Description:	Returns all years based on flight of proposals.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalYears]
AS
BEGIN
	SELECT DISTINCT
		[year]
	FROM (
		SELECT DISTINCT
			YEAR(pf.start_date) 'year'
		FROM
			proposal_flights pf (NOLOCK)

		UNION ALL

		SELECT DISTINCT
			YEAR(pf.end_date) 'year'
		FROM
			proposal_flights pf (NOLOCK)
	) tmp
	ORDER BY
		[year] DESC
END
