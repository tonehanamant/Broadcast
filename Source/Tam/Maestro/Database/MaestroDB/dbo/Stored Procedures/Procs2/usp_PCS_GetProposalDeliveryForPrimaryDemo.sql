
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDeliveryForPrimaryDemo]
     @proposal_id int
AS
BEGIN
     select 
          p.name,  
          p.agency_company_id, 
          p.advertiser_company_id, 
          pr.name, 
          pa.audience_id, 
          a.name, 
          sum(dbo.GetProposalDetailTotalDeliveryUnEQ(pd.id, pa.audience_id) ),
          p.is_equivalized,
          pp.ordinal
     from
          proposals p WITH (NOLOCK) 
          join proposal_details pd WITH (NOLOCK) on pd.proposal_id = p.id
          join proposal_audiences pa WITH (NOLOCK) on pa.proposal_id = p.id and p.guarantee_type = pa.ordinal
          left join products pr WITH (NOLOCK) on pr.id = p.product_id
          join audiences a (NOLOCK) ON a.id=pa.audience_id
          LEFT OUTER JOIN dbo.proposal_proposals pp ON pp.child_proposal_id = p.id
     where
          p.id = @proposal_id
     group by
          p.name, 
          p.agency_company_id, 
          p.advertiser_company_id, 
          pr.name, 
          pa.audience_id, 
          a.name, 
          p.is_equivalized,
          pp.ordinal
END
