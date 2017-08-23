

 

CREATE PROCEDURE [dbo].[usp_TCS_GetOverlappingProposalsByDates]

( @start_date as datetime,

  @end_date as datetime,

  @spot_length_id as int,

  @proposal_id as int )

 

AS

 

SELECT id FROM PROPOSALS (NOLOCK) WHERE 

((START_DATE between @start_date and @end_date) 

OR 

(END_DATE between @start_date and @end_date))

AND proposals.default_spot_length_id = @spot_length_id

 AND proposals.id <> @proposal_id

 

 

