-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/7/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetRemainingActiveDaysInProposal]
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT
	DECLARE @current_date AS DATETIME
	SET @current_date = CONVERT(varchar, GetDate(), 101)

	SET @return = (
		SELECT
			SUM(DATEDIFF(day,CASE WHEN pf.start_date < @current_date THEN @current_date ELSE pf.start_date END,pf.end_date) + 1)
		FROM
			proposal_flights pf (NOLOCK)
		WHERE
			pf.selected = 1
			AND pf.proposal_id = @proposal_id
			AND (pf.start_date >= @current_date OR pf.end_date >= @current_date)
	)

	IF @return IS NULL
		SET @return = 0
		
	RETURN @return;
END
