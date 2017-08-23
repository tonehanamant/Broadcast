-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to delete or potentionally disable a network_genres record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genres_select]
	@network_id INT,
	@genre_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[network_genres].*
	FROM
		[dbo].[network_genres] WITH(NOLOCK)
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
END
