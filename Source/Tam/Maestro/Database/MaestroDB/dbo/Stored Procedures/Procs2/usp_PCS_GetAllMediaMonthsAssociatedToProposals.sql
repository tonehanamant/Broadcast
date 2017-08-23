CREATE PROCEDURE [dbo].[usp_PCS_GetAllMediaMonthsAssociatedToProposals]

AS

select 
	distinct media_months.id, 
	media_months.year, 
	media_months.month, 
	media_months.media_month, 
	media_months.start_date, 
	media_months.end_date 
from
	proposals WITH (NOLOCK)
	join
	media_months WITH (NOLOCK) on 
		(proposals.start_date between media_months.start_date and media_months.end_date) OR
		(proposals.end_date between media_months.start_date and media_months.end_date)
order by
	media_months.id

