
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/15/2016 03:09:18 PM
-- Description:	Auto-generated method to delete a single traffic_materials record.
-- =============================================
CREATE PROCEDURE usp_traffic_materials_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_materials]
	WHERE
		[id]=@id
END
