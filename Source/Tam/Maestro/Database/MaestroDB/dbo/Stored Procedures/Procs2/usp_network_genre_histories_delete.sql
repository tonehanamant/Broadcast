-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to delete a single network_genre_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genre_histories_delete]
	@network_id INT,
	@genre_id INT,
	@start_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[network_genre_histories]
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
		AND [start_date]=@start_date
END
