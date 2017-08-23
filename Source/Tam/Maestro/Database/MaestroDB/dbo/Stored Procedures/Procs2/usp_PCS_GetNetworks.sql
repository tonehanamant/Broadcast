
-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 4/7/2014
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetNetworks]
AS
BEGIN
	SELECT
		*
	FROM
		networks (NOLOCK)
	ORDER BY
		name
END
