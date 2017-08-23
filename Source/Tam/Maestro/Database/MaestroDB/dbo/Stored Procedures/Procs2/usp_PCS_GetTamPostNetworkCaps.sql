-- =============================================
-- Author:		Brenton Reeder
-- Create date: 4/26/2014
-- Description:	Used to pull tam_post_network_caps for a given tam_post_id
-- =============================================
CREATE PROCEDURE usp_PCS_GetTamPostNetworkCaps
	@tam_post_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		tpnc.*
	FROM
		dbo.tam_post_network_caps tpnc (NOLOCK)
	WHERE
		tpnc.tam_post_id = @tam_post_id
END
