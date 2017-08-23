
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:17 PM
-- Description:	Auto-generated method to delete a single zone_networks record.
-- =============================================
CREATE PROCEDURE usp_zone_networks_delete
	@zone_id INT,
	@network_id INT
AS
BEGIN
	DELETE FROM
		[dbo].[zone_networks]
	WHERE
		[zone_id]=@zone_id
		AND [network_id]=@network_id
END
