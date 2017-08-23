
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:17 PM
-- Description:	Auto-generated method to delete or potentionally disable a zone_networks record.
-- =============================================
CREATE PROCEDURE usp_zone_networks_select
	@zone_id INT,
	@network_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zone_networks].*
	FROM
		[dbo].[zone_networks] WITH(NOLOCK)
	WHERE
		[zone_id]=@zone_id
		AND [network_id]=@network_id
END
