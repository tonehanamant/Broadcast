
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:08:59 PM
-- Description:	Auto-generated method to delete a single traffic_detail_audiences record.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_audiences_delete
	@traffic_detail_id INT,
	@audience_id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_detail_audiences]
	WHERE
		[traffic_detail_id]=@traffic_detail_id
		AND [audience_id]=@audience_id
END
