	CREATE Procedure [dbo].[usp_PCS_GetContractedRatingsFromProposal]
		@proposal_id int
	AS
	BEGIN
		SELECT 
			pd.network_id, 
			pda.audience_id, 
			pda.rating 
		FROM 
			proposal_detail_audiences pda WITH (NOLOCK) 
			JOIN proposal_details pd WITH (NOLOCK) on pd.id = pda.proposal_detail_id 
		WHERE
			pd.proposal_id = @proposal_id;
	END
