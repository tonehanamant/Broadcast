

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/23/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_LookupNetworkMap
	@network VARCHAR(127),
	@map_set VARCHAR(127)
AS
BEGIN
	SELECT
		nm.*
	FROM
		network_maps nm (NOLOCK)
	WHERE
		nm.map_set=@map_set
		AND nm.map_value=@network
END
