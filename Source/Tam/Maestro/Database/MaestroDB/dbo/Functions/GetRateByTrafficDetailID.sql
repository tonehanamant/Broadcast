CREATE FUNCTION [dbo].[GetRateByTrafficDetailID]
(
      @traffic_detail_id INT,
      @topography_id INT
)
RETURNS FLOAT
AS
BEGIN
      DECLARE @return FLOAT;
      DECLARE @traffic_id INT;
      DECLARE @sales_model_id INT;
      DECLARE @is_married BIT;
      DECLARE @is_fixed INT;
      
      DECLARE @proposal_rate FLOAT;
      DECLARE @proposal_universe FLOAT;
      DECLARE @traffic_universe FLOAT;
      DECLARE @traffic_rating FLOAT;
      DECLARE @proposal_rating FLOAT;

      DECLARE @net1 FLOAT, @net2 FLOAT, @net3 FLOAT;

      SET @traffic_id = dbo.udf_GetTrafficIdFromTrafficDetailId(@traffic_detail_id);
      SET @sales_model_id = dbo.udf_GetSalesModelFromTrafficId(@traffic_id);
      SET @is_married = dbo.udf_IsTrafficMarried(@traffic_id);
      SET @is_fixed = dbo.udf_IsFixed(@traffic_id,@topography_id);
      
      SELECT @net1 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_AGENCY';
      SELECT @net2 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_VIG';
      SELECT @net3 = (1.0 - smtv.value) FROM sales_model_traffic_vigs smtv (NOLOCK) WHERE smtv.sales_model_id=@sales_model_id AND smtv.map_name='RATE_WITHHOLDING';

      IF @is_married = 1
            BEGIN
				SELECT 
					@return = mt.combinedrate
				FROM 
					dbo.udf_TCS_GetMarriedTrafficRateTableByTrafficDetailId(@traffic_detail_id) mt
				WHERE
					mt.topography_id=@topography_id
            END
      ELSE
            BEGIN
                  IF @is_fixed = 1
                        BEGIN 
                              SELECT DISTINCT 
                                    @return = tdt.rate
                              FROM
                                      traffic_details td (NOLOCK)
                                      JOIN traffic_detail_weeks tdw (NOLOCK) ON td.id = tdw.traffic_detail_id
                                      JOIN traffic_detail_topographies tdt (NOLOCK) ON tdt.traffic_detail_week_id = tdw.id
                              WHERE
                                      td.id = @traffic_detail_id 
                                      AND tdt.topography_id = @topography_id;
                        END
                  ELSE
                        BEGIN
                              SELECT
                                      @proposal_rate = tdpd.proposal_rate,
                                      @proposal_universe = pda.us_universe * pd.universal_scaling_factor,
                                      @traffic_universe = dbo.GetTrafficDetailCoverageUniverse(td.id, t.audience_id, @topography_id),
                                      @traffic_rating = CASE WHEN tda.traffic_rating < pda.rating THEN tda.traffic_rating ELSE pda.rating END,
                                      @proposal_rating = pda.rating
                              FROM
                                    traffic_details td (NOLOCK) 
                                    JOIN traffic t (NOLOCK) ON t.id = td.traffic_id 
                                    JOIN traffic_details_proposal_details_map tdpd (NOLOCK) ON tdpd.traffic_detail_id = td.id
                                    JOIN proposal_details pd (NOLOCK) ON pd.id = tdpd.proposal_detail_id
                                    JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id = pd.proposal_id 
                                          AND t.audience_id = pa.audience_id
                                    JOIN proposal_detail_audiences pda (NOLOCK) ON pda.proposal_detail_id = pd.id 
                                          AND pda.audience_id = pa.audience_id 
                                    JOIN traffic_detail_audiences tda (NOLOCK) ON tda.traffic_detail_id = td.id 
                                          AND tda.audience_id = t.audience_id
                              WHERE
                                    td.id = @traffic_detail_id;
                                    
                              SELECT @return = 
                                    CASE WHEN @proposal_universe > 0 AND @proposal_rating > 0 THEN            
                                          ((@traffic_rating * @traffic_universe) / 1000.0) * ((@proposal_rate / ((@proposal_rating * @proposal_universe) / 1000.0)) * @net1 * @net2 * @net3) 
                                    ELSE
                                          0.0
                                    END;
                        END
            END
         
      RETURN ROUND(@return, 2)
END
