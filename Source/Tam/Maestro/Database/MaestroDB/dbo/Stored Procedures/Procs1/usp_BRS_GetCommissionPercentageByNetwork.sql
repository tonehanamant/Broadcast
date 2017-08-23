-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetCommissionPercentageByNetwork
	@network_id INT
AS
BEGIN
	DECLARE @commission DECIMAL(18,2)
	
	SELECT 
		@commission = commission 
	FROM 
		cmw_network_commissions cnc (NOLOCK) 
	WHERE 
		network_id=@network_id
	
	IF @commission IS NULL
		SET @commission = 0.15
		
	SELECT @commission
END
