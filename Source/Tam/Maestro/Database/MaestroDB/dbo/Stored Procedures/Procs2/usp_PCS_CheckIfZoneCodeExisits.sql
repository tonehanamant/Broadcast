	
	-- =============================================
	-- Author:		Nicholas Kheynis
	-- Create date: 11/07/2014
	-- Description:	<Description,,>
	-- =============================================
	-- usp_PCS_CheckIfZoneCodeExisits 'AHCN'
	CREATE PROCEDURE [dbo].[usp_PCS_CheckIfZoneCodeExisits]
		@zone_code VARCHAR(31)
	AS
	BEGIN
		SELECT
			z.*
		FROM
			zones z
		WHERE 
			z.code = @zone_code
	END
	
