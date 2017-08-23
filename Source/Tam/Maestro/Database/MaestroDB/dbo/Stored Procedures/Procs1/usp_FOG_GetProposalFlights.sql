-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetProposalFlights]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		mw.id,
		pf.*
	FROM
		proposal_flights pf (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON (mw.start_date <= pf.end_date AND mw.end_date >= pf.start_date)
	WHERE
		pf.proposal_id=@proposal_id
END
