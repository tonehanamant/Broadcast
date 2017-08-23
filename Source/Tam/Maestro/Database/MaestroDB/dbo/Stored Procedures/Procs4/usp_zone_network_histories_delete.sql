
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:16 PM
-- Description:	Auto-generated method to delete a single zone_network_histories record.
-- =============================================
CREATE PROCEDURE usp_zone_network_histories_delete
	@zone_id INT,
	@network_id INT,
	@start_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[zone_network_histories]
	WHERE
		[zone_id]=@zone_id
		AND [network_id]=@network_id
		AND [start_date]=@start_date
END
