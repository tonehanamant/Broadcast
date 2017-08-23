
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to insert a cluster_rate_card_detail_rates record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_detail_rates_insert
	@cluster_rate_card_detail_id INT,
	@spot_length_id INT,
	@rate MONEY
AS
BEGIN
	INSERT INTO [dbo].[cluster_rate_card_detail_rates]
	(
		[cluster_rate_card_detail_id],
		[spot_length_id],
		[rate]
	)
	VALUES
	(
		@cluster_rate_card_detail_id,
		@spot_length_id,
		@rate
	)
END