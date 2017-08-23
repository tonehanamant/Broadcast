-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to update a network_genres record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genres_update]
	@network_id INT,
	@genre_id INT,
	@effective_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[network_genres]
	SET
		[effective_date]=@effective_date
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
END
