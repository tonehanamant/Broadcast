-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to update a network_genre_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genre_histories_update]
	@network_id INT,
	@genre_id INT,
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[network_genre_histories]
	SET
		[end_date]=@end_date
	WHERE
		[network_id]=@network_id
		AND [genre_id]=@genre_id
		AND [start_date]=@start_date
END
