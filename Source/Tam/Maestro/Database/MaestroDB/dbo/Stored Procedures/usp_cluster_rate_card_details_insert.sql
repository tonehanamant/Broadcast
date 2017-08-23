
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to insert a cluster_rate_card_details record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_details_insert
	@id INT OUTPUT,
	@cluster_rate_card_id INT,
	@daypart_id INT,
	@cluster_id INT
AS
BEGIN
	INSERT INTO [dbo].[cluster_rate_card_details]
	(
		[cluster_rate_card_id],
		[daypart_id],
		[cluster_id]
	)
	VALUES
	(
		@cluster_rate_card_id,
		@daypart_id,
		@cluster_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END