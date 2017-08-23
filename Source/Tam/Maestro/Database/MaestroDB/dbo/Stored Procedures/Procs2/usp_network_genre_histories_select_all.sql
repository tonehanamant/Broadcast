-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to select all network_genre_histories records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genre_histories_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[network_genre_histories].*
	FROM
		[dbo].[network_genre_histories] WITH(NOLOCK)
END
