-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to delete or potentionally disable a network_genre_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genre_histories_select]
	@network_id INT,
	@genre_id INT,
	@start_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[network_genre_histories].*
	FROM
		[dbo].[network_genre_histories] WITH(NOLOCK)
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
		AND [start_date]=@start_date
END
