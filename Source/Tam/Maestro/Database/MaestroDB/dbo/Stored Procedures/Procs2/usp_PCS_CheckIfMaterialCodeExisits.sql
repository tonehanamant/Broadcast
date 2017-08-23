	
	
	-- =============================================
	-- Author:		Nicholas Kheynis
	-- Create date: 11/07/2014
	-- Description:	<Description,,>
	-- =============================================
	-- usp_PCS_CheckIfMaterialCodeExisits 'XJCF9513'
	CREATE PROCEDURE [dbo].[usp_PCS_CheckIfMaterialCodeExisits]
		@material_code VARCHAR(31)
	AS
	BEGIN
		SELECT
			m.*
		FROM
			materials m
		WHERE 
			m.code = @material_code
	END
	
	
