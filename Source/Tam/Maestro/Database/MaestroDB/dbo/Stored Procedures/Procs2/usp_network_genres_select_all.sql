-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to select all network_genres records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genres_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[network_genres].*
	FROM
		[dbo].[network_genres] WITH(NOLOCK)
END
