-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to delete a single network_genres record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genres_delete]
	@network_id INT,
	@genre_id INT
AS
BEGIN
	DELETE FROM
		[dbo].[network_genres]
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
END
