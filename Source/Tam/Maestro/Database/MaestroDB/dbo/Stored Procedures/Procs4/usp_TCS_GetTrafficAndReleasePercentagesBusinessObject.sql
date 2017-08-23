-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	
-- =============================================
CREATE PROCEDURE usp_TCS_GetTrafficAndReleasePercentagesBusinessObject
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		p.id 'property_id',
		CAST(p.value AS FLOAT) 'value'
	FROM 
		properties p (NOLOCK) 
	WHERE 
		p.name='proposal_discount_factor_for_traffic_overage'

	SELECT 
		smtv.id,
		smtv.sales_model_id,
		sl.name,
		smtv.map_name,
		smtv.value
	FROM 
		sales_model_traffic_vigs smtv (NOLOCK)
		JOIN sales_models sl (NOLOCK) ON sl.id=smtv.sales_model_id
	WHERE
		smtv.map_name IN ('RATE_AGENCY','RATE_VIG','RATE_WITHHOLDING')

	SELECT 
		slm.id,
		slm.map_set,
		CAST(slm.map_value AS FLOAT) 'value',
		slm.effective_date,
		sl.*
	FROM 
		spot_length_maps slm (NOLOCK) 
		JOIN spot_lengths sl (NOLOCK) ON sl.id=slm.spot_length_id
	WHERE
		slm.map_set IN ('release_clearance_factor','traffic_clearance_factor')
	ORDER BY 
		slm.spot_length_id,
		slm.map_set,
		slm.effective_date

	-- spot lengths with no clearance factors
	SELECT
		sl.*
	FROM
		spot_lengths sl (NOLOCK)
	WHERE
		sl.id NOT IN (
			SELECT slm.spot_length_id FROM spot_length_maps slm (NOLOCK) 
		)
	ORDER BY
		sl.order_by
END
