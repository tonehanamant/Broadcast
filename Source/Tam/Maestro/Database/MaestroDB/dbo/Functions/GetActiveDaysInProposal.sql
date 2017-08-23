-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/7/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetActiveDaysInProposal
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	SET @return = (
		SELECT
			SUM(DATEDIFF(day,pf.start_date,pf.end_date) + 1)
		FROM
			proposal_flights pf (NOLOCK)
		WHERE
			pf.selected = 1
			AND pf.proposal_id = @proposal_id
	)
		
	RETURN @return;
END
