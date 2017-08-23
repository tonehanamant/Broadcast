-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/17/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARSLoader_GetNielsenNetworkLookup
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		nn.id,
		nn.code,
		nn.nielsen_id
	FROM 
		nielsen_networks nn (NOLOCK)
END
