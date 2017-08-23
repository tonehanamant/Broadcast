
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/9/2010
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[udf_GetOrderedProposalsForMediaMonth]
(	
	@media_month VARCHAR(4)
)
RETURNS TABLE 
AS
RETURN (
	select
		proposal.*
	from
		media_months (nolock) media_month 
		join proposals (nolock) proposal on
			proposal.start_date < media_month.end_date
			and
			media_month.start_date < proposal.end_date
	where
		@media_month = media_month.media_month
		and
		proposal.proposal_status_id = 4
);

