	
	
	-- =============================================
	-- Author:		Nicholas Kheynis
	-- Create date: 11/07/2014
	-- Description:	<Description,,>
	-- =============================================
	-- usp_PCS_CheckIfNetworkCodeExisits 'FXSP1'
	CREATE PROCEDURE [dbo].[usp_PCS_CheckIfNetworkCodeExisits]
		@network_code VARCHAR(31)
	AS
	BEGIN
		SELECT
			n.*
		FROM
			networks n
		WHERE 
			n.code = @network_code
	END
