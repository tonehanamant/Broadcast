
CREATE Procedure [dbo].[usp_TCS_GetProposalFlights]
	(
		@id Int
	)
AS
SELECT proposal_id, start_date, end_date, selected 
from 
proposal_flights (NOLOCK) 
where proposal_id = @id
order by start_date

