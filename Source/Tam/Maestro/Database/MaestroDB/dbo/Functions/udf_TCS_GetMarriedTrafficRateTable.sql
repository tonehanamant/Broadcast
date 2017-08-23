
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created SP.Get traffic rate
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.Added original_proposal_cpm1 & 2. 
*****************************************************************************************************/
CREATE FUNCTION [dbo].[udf_TCS_GetMarriedTrafficRateTable]
(
      @traffic_id INT
)
RETURNS @rates TABLE
(
    traffic_detail_id INT,
    network_id INT,
    proposal_id1 INT,
    proposal_id2 INT,
	proposal_cpm1 MONEY,
    proposal_cpm2 MONEY,
    original_proposal_cpm1 MONEY,
    original_proposal_cpm2 MONEY,
    topography_id INT,
    delivery1 FLOAT,
    delivery2 FLOAT,
    rate1 MONEY,
    rate2 MONEY,
    combinedrate MONEY
)
AS
BEGIN
      DECLARE @sales_model_id INT;
      DECLARE @net1 FLOAT, @net2 FLOAT, @net3 FLOAT;

      SET @sales_model_id = dbo.udf_GetSalesModelFromTrafficId(@traffic_id);

      SELECT @net1 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_AGENCY';
      SELECT @net2 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_VIG';
      SELECT @net3 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_WITHHOLDING';
      
      WITH married_cpms
      (
            network_id,
            proposal_id1,
            proposal_id2,
            proposal1_audience_id,
            proposal2_audience_id,
            proposal1_rating,
            proposal2_rating,
            proposal_cpm1,
            proposal_cpm2,
            traffic_cpm
      )
      AS
      (
            SELECT 
                  pd1.network_id 'network_id',
                  p1.id 'proposal_id1',
                  p2.id 'proposal_id2',
                  pa1.audience_id 'proposal1_audience_id',
                  pa2.audience_id 'proposal2_audience_id',
                  pda1.rating 'proposal1_rating',
                  pda2.rating 'proposal2_rating',
                  dbo.GetProposalDetailCPMUnEquivalized(pd1.id, pa1.audience_id) * (rc1.weighting_factor) 'proposal_cpm1',
                  dbo.GetProposalDetailCPMUnEquivalized(pd2.id, pa2.audience_id) * (rc2.weighting_factor)'proposal_cpm2',
                  dbo.GetProposalDetailCPMUnEquivalized(pd1.id, pa1.audience_id) * (rc1.weighting_factor) + dbo.GetProposalDetailCPMUnEquivalized(pd2.id, pa2.audience_id) * (rc2.weighting_factor) 'traffic_cpm'
            FROM
                  release_cpmlink rc1 (NOLOCK)
                  JOIN release_cpmlink rc2 (NOLOCK) ON rc1.traffic_id = rc2.traffic_id 
                        AND rc1.proposal_id <> rc2.proposal_id 
                        AND rc1.id < rc2.id
                  JOIN proposal_details pd1 (NOLOCK) ON pd1.proposal_id = rc1.proposal_id
                  JOIN proposal_details pd2 (NOLOCK) ON pd2.proposal_id = rc2.proposal_id 
                        AND pd1.network_id = pd2.network_id
                  JOIN proposals p1 (NOLOCK) ON p1.id = pd1.proposal_id
                  JOIN proposals p2 (NOLOCK) ON p2.id = pd2.proposal_id
                  JOIN proposal_audiences pa1 (NOLOCK) ON pa1.proposal_id = p1.id 
                        AND pa1.ordinal = p1.guarantee_type
                  JOIN proposal_audiences pa2 (NOLOCK) ON pa2.proposal_id = p2.id 
                        AND pa2.ordinal = p2.guarantee_type
                  JOIN proposal_detail_audiences pda1 (NOLOCK) ON pda1.proposal_detail_id = pd1.id 
                        AND pda1.audience_id = pa1.audience_id
                  JOIN proposal_detail_audiences pda2 (NOLOCK) ON pda2.proposal_detail_id = pd2.id 
                        AND pda2.audience_id = pa2.audience_id
            WHERE
                  rc1.traffic_id = @traffic_id
      )
      INSERT INTO @rates
            SELECT DISTINCT
                  td.id,
                  td.network_id,
                  mc.proposal_id1,
                  mc.proposal_id2,
			      ROUND((mc.proposal_cpm1 * @net1 * @net2 * @net3), 2) 'proposal_cpm1',
                  ROUND((mc.proposal_cpm2 * @net1 * @net2 * @net3), 2) 'proposal_cpm2',
				  ROUND(mc.proposal_cpm1,2) 'original_proposal_cpm1',
				  ROUND(mc.proposal_cpm2,2) 'original_proposal_cpm2',
                  tdt.topography_id,
                  CASE WHEN pda.rating < tda1.traffic_rating THEN pda.rating ELSE tda1.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda1.audience_id, tdt.topography_id) / 1000.0) 'delivery1',
                  CASE WHEN mc.proposal2_rating < tda2.traffic_rating THEN mc.proposal2_rating ELSE tda2.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda2.audience_id, tdt.topography_id) / 1000.0) 'delivery2',
                  ROUND(ROUND((mc.proposal_cpm1 * @net1 * @net2 * @net3), 2) * (CASE WHEN pda.rating < tda1.traffic_rating THEN pda.rating ELSE tda1.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda1.audience_id, tdt.topography_id) / 1000.0)), 2) 'Rate1',
                  ROUND(ROUND((mc.proposal_cpm2 * @net1 * @net2 * @net3), 2) * (CASE WHEN mc.proposal2_rating < tda2.traffic_rating THEN mc.proposal2_rating ELSE tda2.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda2.audience_id, tdt.topography_id) / 1000.0)), 2) 'Rate2',
                  CASE WHEN tm.map_value = 'var' THEN
                        ROUND((ROUND((mc.proposal_cpm1 * @net1 * @net2 * @net3), 2) * (CASE WHEN pda.rating < tda1.traffic_rating THEN pda.rating ELSE tda1.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda1.audience_id, tdt.topography_id) / 1000.0))) + (ROUND((mc.proposal_cpm2 * @net1 * @net2 * @net3), 2) * (CASE WHEN mc.proposal2_rating < tda2.traffic_rating THEN mc.proposal2_rating ELSE tda2.traffic_rating END * (dbo.GetTrafficDetailCoverageUniverse(td.id, tda2.audience_id, tdt.topography_id) / 1000.0))), 2) 
                  ELSE
                        tdt.rate
                  END
            FROM
                  married_cpms mc 
                  JOIN traffic_details td (NOLOCK) ON td.network_id = mc.network_id
                  JOIN traffic t (NOLOCK) ON t.id = td.traffic_id 
                  JOIN traffic_detail_audiences tda1 (NOLOCK) ON tda1.traffic_detail_id = td.id 
                        AND tda1.audience_id = t.audience_id
                  JOIN traffic_detail_audiences tda2 (NOLOCK) ON tda2.traffic_detail_id = td.id 
                        AND tda2.audience_id = mc.proposal2_audience_id
                  JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id = td.id
                  JOIN traffic_detail_topographies tdt (NOLOCK) ON tdt.traffic_detail_week_id = tdw.id
                  JOIN topography_maps tm (NOLOCK) ON tm.topography_id = tdt.topography_id 
                        AND tm.map_set = 'rate_type'
                  JOIN traffic_proposals tp (NOLOCK) ON tp.traffic_id = t.id
                  JOIN proposals p (NOLOCK) ON p.id = tp.proposal_id
                  JOIN proposal_details pd (NOLOCK) ON pd.proposal_id = p.id 
                        AND pd.network_id = td.network_id
                  JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id = p.id 
                        AND  p.guarantee_type = pa.ordinal
                  JOIN proposal_detail_audiences pda (NOLOCK) ON pda.proposal_detail_id = pd.id 
                        AND pda.audience_id = pa.audience_id
            WHERE
                  td.traffic_id = @traffic_id;
                  
      RETURN;
END
