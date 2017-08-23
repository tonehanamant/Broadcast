
-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2016 04:41:41 PM
-- Description:	Auto-generated method to delete a single traffic_orders record.
-- =============================================
CREATE PROCEDURE usp_traffic_orders_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_orders]
	WHERE
		[id]=@id
END
