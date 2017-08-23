CREATE FUNCTION [dbo].[GetTotalGuaranteeImpressionsByProposalNetworkAudience]          
(        
	@proposal_id int        
	,@audience_id int        
	,@network_id int        
)        
RETURNS FLOAT          
AS          
BEGIN          
	DECLARE @return AS FLOAT          
	 
	SET @return = (         
		SELECT           
			sum(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, @audience_id)) AS total_impressions        
		FROM          
			proposals p (NOLOCK)          
		INNER JOIN proposal_details pd (NOLOCK) on pd.proposal_id = p.id          
		INNER JOIN proposal_audiences pa (NOLOCK) on pa.proposal_id = p.id         
		INNER JOIN proposal_detail_audiences pda (NOLOCK) on pd.id = pda.proposal_detail_id         
			AND pa.audience_id = pda.audience_id          
		INNER JOIN audiences a (NOLOCK) on a.id = pa.audience_id          
			AND a.id = @audience_id        
		INNER JOIN uvw_network_universe n (NOLOCK) on n.network_id = pd.network_id         
			AND (n.start_date<=pd.start_date         
			AND (n.end_date>=pd.start_date OR n.end_date IS NULL))          
			AND n.network_id = @network_id        
		WHERE          
			p.id = @proposal_id         
			AND pd.include = 1          
		GROUP BY        
			p.id        
			,a.id        
			,n.network_id          
	)        
	RETURN @return        
END
