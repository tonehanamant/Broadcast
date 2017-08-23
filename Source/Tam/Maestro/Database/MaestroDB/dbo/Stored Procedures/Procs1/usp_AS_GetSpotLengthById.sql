	
	-- =============================================
	-- Author:		Nicholas Kheynis
	-- Create date: 12/23/14
	-- Description:	<Description,,>
	-- =============================================
	--usp_AS_GetSpotLengthById 1
	CREATE PROCEDURE [dbo].[usp_AS_GetSpotLengthById]
	     @spot_length_id INT
	AS
	BEGIN
	    SELECT 
			s.*
		FROM
			dbo.spot_lengths s
		WHERE
			@spot_length_id = s.id
	END
	
