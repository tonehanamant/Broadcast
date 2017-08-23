CREATE PROCEDURE usp_TCS_GetMarryCandidateContractDelivery 
(
	@proposal_id int,
	@media_month_id int
)
AS
BEGIN
	-- set @proposal_id = 35091;
	-- set @media_month_id = 362;
	
	CREATE TABLE #deliv_temp (proposal_id int, delivery float);
	
	INSERT INTO #deliv_temp (proposal_id, delivery)
	select 
		p.id, ISNULL( SUM(CAST(pdw.units AS FLOAT)), 0) * dbo.GetProposalDetailDeliveryUnEQ(pd.id, pa.audience_id) 
	from
		proposals p WITH (NOLOCK)
		join proposal_details pd WITH (NOLOCK) on pd.proposal_id = p.id
		join proposal_audiences pa WITH (NOLOCK) on pa.ordinal = p.guarantee_type and pa.proposal_id = p.id
		join proposal_marry_candidates pmc WITH (NOLOCK) on pmc.proposal_id = p.id and pmc.media_month_id = @media_month_id
		join proposal_marry_candidate_flights pmcf WITH (NOLOCK) ON  pmc.id = pmcf.proposal_marry_candidate_id
		join media_weeks mw WITH (NOLOCK) on mw.start_date <= pmcf.end_date and mw.end_date >= pmcf.start_date
		join proposal_detail_worksheets pdw WITH (NOLOCK) on pdw.media_week_id = mw.id and pdw.proposal_detail_id = pd.id
	where
		pmcf.selected = 1
		and 
		p.id = @proposal_id
	GROUP BY
		p.id,
		pd.id, 
		pa.audience_id,
		pd.network_id
	ORDER BY
		pd.network_id
	
	SELECT SUM(delivery) from 
		#deliv_temp
	WHERE
		proposal_id = @proposal_id;
		
	DROP TABLE #deliv_temp;
END
