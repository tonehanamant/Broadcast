
-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/16/2016 09:55:44 AM
-- Description:	Auto-generated method to select a single cluster_rate_card_details record.
-- =============================================
CREATE PROCEDURE usp_cluster_rate_card_details_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[cluster_rate_card_details].*
	FROM
		[dbo].[cluster_rate_card_details] WITH(NOLOCK)
	WHERE
		[id]=@id
END