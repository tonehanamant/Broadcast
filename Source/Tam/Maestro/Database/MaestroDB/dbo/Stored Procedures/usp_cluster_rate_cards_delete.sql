
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:45 AM
-- Description:	Auto-generated method to delete a single cluster_rate_cards record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_cards_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[cluster_rate_cards]
	WHERE
		[id]=@id
END