
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:45 AM
-- Description:	Auto-generated method to select all cluster_rate_cards records.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_cards_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[cluster_rate_cards].*
	FROM
		[dbo].[cluster_rate_cards] WITH(NOLOCK)
END