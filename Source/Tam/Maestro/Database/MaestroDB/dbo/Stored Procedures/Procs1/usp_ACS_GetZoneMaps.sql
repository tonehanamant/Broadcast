	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 12/05/2011
	-- Updated:		03/11/2015
	-- Description:	
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_ACS_GetZoneMaps]
	AS
	BEGIN
		SELECT 
			zm.*
		FROM 
			zone_maps zm (NOLOCK)
		WHERE 
			map_set IN ('ATT','Affidavits')
	END
