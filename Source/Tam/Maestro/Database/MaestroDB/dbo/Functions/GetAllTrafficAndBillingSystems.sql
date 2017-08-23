


-- =============================================
-- Author:		Joseph Jacobs
-- Create date: 10/6/2009
-- Description:	
-- =============================================
-- SELECT * FROM dbo.[GetAllTrafficAndBillingSystems]
CREATE FUNCTION [dbo].[GetAllTrafficAndBillingSystems]
()
RETURNS @systems TABLE
(
	billing_system_id INT,
	traffic_system_id INT
)
AS
BEGIN
	INSERT INTO @systems
-- gives you all systems associated with the BILLING system's zones
    SELECT 
          DISTINCT szBilling.system_id, szTraffic.system_id
    FROM
                  system_zones (NOLOCK) szTraffic
				  join system_zones (NOLOCK) szBilling on szTraffic.zone_id = szBilling.zone_id

	RETURN;
END


