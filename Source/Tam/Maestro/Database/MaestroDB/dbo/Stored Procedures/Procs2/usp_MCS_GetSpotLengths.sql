-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetSpotLengths]
AS
BEGIN
	SELECT
		sl.length,
		sl.id
	FROM
		dbo.spot_lengths sl (NOLOCK)
END
