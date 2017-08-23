
CREATE PROCEDURE [dbo].[usp_PCS_GetDistinctAudiencesFrom15Proposals]
	@media_month_id int
AS
BEGIN
	select 
		distinct 
			proposal_audiences.audience_id, 
			a.name
	from 
		proposals (NOLOCK)
		join spot_lengths (NOLOCK) on spot_lengths.id = proposals.default_spot_length_id
		join media_months (NOLOCK) on (media_months.start_date between proposals.start_date and proposals.end_date)
				or  (media_months.end_date between proposals.start_date and proposals.end_date)
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id
		JOIN audiences a (NOLOCK) ON a.id=proposal_audiences.audience_id
	where
		media_months.id = @media_month_id
		and	spot_lengths.length = 15
		and proposals.include_on_marriage_report = 1
	order by
		proposal_audiences.audience_id	
END
