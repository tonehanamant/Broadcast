-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/10/2016
-- Description:	Returns true if the network_map exists, false otherwise.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_DoesNetworkMapExist]
	@map_value VARCHAR(127),
	@map_set VARCHAR(127),
	@network_id INT
AS
BEGIN
	SELECT
		nm.*
	FROM
		network_maps nm (NOLOCK)
	WHERE
		nm.map_set=@map_set
		AND nm.map_value=@map_value
		AND nm.network_id=@network_id
END