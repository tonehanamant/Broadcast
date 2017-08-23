-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/19/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetRemainingHiatusDaysInProposal
(
	@proposal_id INT,
	@effective_date DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	SET @return = (
		SELECT
			SUM(DATEDIFF(day,CASE WHEN @effective_date BETWEEN pf.start_date AND pf.end_date THEN @effective_date ELSE pf.start_date END,pf.end_date))
		FROM
			proposal_flights pf (NOLOCK)
		WHERE
			pf.proposal_id=@proposal_id
			AND pf.selected=0
			AND @effective_date < pf.end_date
	)
		
	RETURN @return;
END
