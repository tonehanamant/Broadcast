-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaMonthsOfProposal]
    @proposal_id INT
AS
BEGIN
	SELECT DISTINCT	
		mm.*
	FROM
		proposal_flights pf (NOLOCK) 
		JOIN media_months mm (NOLOCK) ON mm.start_date <= pf.end_date AND mm.end_date >= pf.start_date
	WHERE 
		pf.proposal_id=@proposal_id
	ORDER BY 
		mm.start_date
END
