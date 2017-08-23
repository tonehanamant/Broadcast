-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to insert a network_genre_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genre_histories_insert]
	@network_id INT,
	@genre_id INT,
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[network_genre_histories]
	(
		[network_id],
		[genre_id],
		[start_date],
		[end_date]
	)
	VALUES
	(
		@network_id,
		@genre_id,
		@start_date,
		@end_date
	)
END
