
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to select all cluster_rate_card_detail_rates records.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_detail_rates_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[cluster_rate_card_detail_rates].*
	FROM
		[dbo].[cluster_rate_card_detail_rates] WITH(NOLOCK)
END