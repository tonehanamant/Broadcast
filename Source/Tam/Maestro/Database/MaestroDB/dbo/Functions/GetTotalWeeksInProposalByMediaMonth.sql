-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetTotalWeeksInProposalByMediaMonth
(
	@proposal_id INT,
	@media_month_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT

	SET @return = (
		CAST
		(
			(
				SELECT COUNT(*) FROM proposal_flights pf (NOLOCK) WHERE 
					pf.proposal_id=@proposal_id 
					AND selected=1 
					AND (SELECT start_date FROM media_months WHERE id=@media_month_id) <= pf.start_date 
					AND (SELECT end_date   FROM media_months WHERE id=@media_month_id) >= pf.start_date
			) AS FLOAT
		)
	)
	
	RETURN @return
END
