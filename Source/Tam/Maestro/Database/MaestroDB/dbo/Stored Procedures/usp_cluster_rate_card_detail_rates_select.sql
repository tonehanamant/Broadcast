
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to delete or potentionally disable a cluster_rate_card_detail_rates record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_detail_rates_select
	@cluster_rate_card_detail_id INT,
	@spot_length_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[cluster_rate_card_detail_rates].*
	FROM
		[dbo].[cluster_rate_card_detail_rates] WITH(NOLOCK)
	WHERE
		[cluster_rate_card_detail_id]=@cluster_rate_card_detail_id
		AND [spot_length_id]=@spot_length_id
END