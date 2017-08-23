
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to update a cluster_rate_card_details record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_details_update
	@id INT,
	@cluster_rate_card_id INT,
	@daypart_id INT,
	@cluster_id INT
AS
BEGIN
	UPDATE
		[dbo].[cluster_rate_card_details]
	SET
		[cluster_rate_card_id]=@cluster_rate_card_id,
		[daypart_id]=@daypart_id,
		[cluster_id]=@cluster_id
	WHERE
		[id]=@id
END