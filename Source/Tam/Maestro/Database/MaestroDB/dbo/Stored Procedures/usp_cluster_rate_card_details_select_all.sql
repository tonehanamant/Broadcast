
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to select all cluster_rate_card_details records.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_details_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[cluster_rate_card_details].*
	FROM
		[dbo].[cluster_rate_card_details] WITH(NOLOCK)
END