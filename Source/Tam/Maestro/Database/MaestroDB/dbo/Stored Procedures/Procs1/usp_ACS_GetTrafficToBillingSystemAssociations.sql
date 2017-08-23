-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
-- usp_ACS_GetTrafficToBillingSystemAssociations '6/15/2011','7/5/2011'
CREATE PROCEDURE usp_ACS_GetTrafficToBillingSystemAssociations
	@effective_date DATETIME,
	@current_date DATETIME
AS
BEGIN	
	SELECT 
		billing_system_id,
		traffic_system_id 
	FROM 
		dbo.udf_TrafficToBillingSystems(@effective_date) 
		
	UNION 
	
	SELECT 
		billing_system_id,
		traffic_system_id 
	FROM 
		dbo.udf_TrafficToBillingSystems(@current_date)
END
