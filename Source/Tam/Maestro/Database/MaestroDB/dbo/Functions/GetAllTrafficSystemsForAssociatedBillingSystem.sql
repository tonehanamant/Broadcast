

-- =============================================
-- Author:		Joseph Jacobs
-- Create date: 10/6/2009
-- Description:	
-- =============================================
-- SELECT * FROM dbo.GetAllTrafficSystemsForAssociatedBillingSystem(1, '9/15/2009')
CREATE FUNCTION [dbo].[GetAllTrafficSystemsForAssociatedBillingSystem]
(	
	@system_id INT
)
RETURNS @systems TABLE
(
	system_id INT
)
AS
BEGIN
	INSERT INTO @systems
-- gives you all systems associated with the BILLING system's zones
    SELECT 
          DISTINCT system_id
    FROM
                  system_zones (NOLOCK)
            WHERE
                  system_zones.zone_id IN (
                        -- gives you all zones related to the BILLING system
                        SELECT
                              system_zones.zone_id
                        FROM
                              system_zones (NOLOCK)
                        WHERE
                              system_zones.system_id=@system_id
                  )		

	RETURN;
END

