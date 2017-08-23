	
	-- =============================================
	-- Author:        Nicholas Kheynis
	-- Create date: 12/17/2014
	-- Description:   Get subscriber count by zone/network/effective date
	-- =============================================
	CREATE PROCEDURE usp_STS2_GetZoneNameByCode
	      @zone_code VARCHAR(15)
	AS
	BEGIN
	      SET NOCOUNT ON;
	
	    SELECT
	            z.name
	      FROM
	            dbo.zones z
	      WHERE
	            z.code = @zone_code
	END
	
