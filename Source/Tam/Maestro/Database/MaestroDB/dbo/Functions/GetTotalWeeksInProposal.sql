-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTotalWeeksInProposal]
(
	@proposal_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT

	SET @return = (
		CAST
		(
			(
				SELECT 
					COUNT(*) 
				FROM 
					proposal_flights pf (NOLOCK) 
				WHERE 
					pf.proposal_id=@proposal_id 
					AND selected=1
			) AS FLOAT
		)
	)
	
	RETURN @return
END
