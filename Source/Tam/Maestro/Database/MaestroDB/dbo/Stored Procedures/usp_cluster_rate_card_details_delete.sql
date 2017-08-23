
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to delete a single cluster_rate_card_details record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_details_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[cluster_rate_card_details]
	WHERE
		[id]=@id
END