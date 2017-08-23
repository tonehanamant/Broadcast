-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/12/2014
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.udf_GetTrafficClearanceFactorByProposal(30012,GETDATE())
CREATE FUNCTION [dbo].[udf_GetTrafficClearanceFactorByProposal] 
(
	@proposal_id INT,
	@effective_date DATETIME
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT;
	DECLARE @spot_length_id INT;
	
	-- get spot_length_id from proposal
	SET @spot_length_id = (
		SELECT TOP 1 WITH ties 
			pd.spot_length_id
		FROM 
			proposal_details pd (NOLOCK)
		WHERE 
			pd.spot_length_id IS NOT NULL
			AND pd.proposal_id = @proposal_id
		GROUP BY 
			pd.spot_length_id
		ORDER  BY 
			COUNT(1) DESC
	);
	
	-- get release clearance factor
	SELECT
		@return = ISNULL(CAST(slm.map_value AS FLOAT), 1.0)
	FROM
		uvw_spot_length_maps slm
	WHERE                 
		slm.spot_length_id=@spot_length_id
		AND slm.map_set='traffic_clearance_factor'
		AND slm.start_date<=@effective_date AND (slm.end_date>=@effective_date OR slm.end_date IS NULL);

	RETURN @return;
END
