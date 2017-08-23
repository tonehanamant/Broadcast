

-- =============================================
-- Author:		Joseph Jacobs
-- Create date: 10/6/2009
-- Description:	
-- =============================================
-- SELECT * FROM dbo.[GetAllZonesForAssociatedSystem](1, '9/15/2009')
CREATE FUNCTION [dbo].[GetAllZonesForAssociatedSystem]
(	
	@system_id int,
	@effective_date DATETIME
)
RETURNS @zones TABLE
(
	zone_id INT
)
AS
BEGIN
	INSERT INTO @zones
	select distinct primary_zone_id from zone_zones (NOLOCK) where primary_zone_id in 
		(select zone_id from system_zones (NOLOCK) where system_id = @system_id) 
		or secondary_zone_id in 
		(select zone_id from system_zones (NOLOCK) where system_id = @system_id)
		 and effective_date <= @effective_date
	UNION
	select distinct secondary_zone_id from zone_zones (NOLOCK) where primary_zone_id in 
		(select zone_id from system_zones (NOLOCK) where system_id = @system_id) 
		or secondary_zone_id in 
		(select zone_id from system_zones (NOLOCK) where system_id = @system_id)
	 and effective_date <= @effective_date

	RETURN;
END
