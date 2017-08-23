-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetProposalActiveWeeksInFlight(23245)
CREATE FUNCTION [dbo].[GetProposalActiveWeeksInFlight]
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return AS INT
	
	SET @return = 0
	
	SET @return = (
		SELECT
			COUNT(*)
		FROM
			proposal_flights pf (NOLOCK) 
			JOIN media_months mm (NOLOCK) ON mm.start_date <= pf.end_date AND mm.end_date >= pf.start_date
		WHERE 
			pf.selected=1
			AND pf.proposal_id=@proposal_id
	)
	
	RETURN @return
END
