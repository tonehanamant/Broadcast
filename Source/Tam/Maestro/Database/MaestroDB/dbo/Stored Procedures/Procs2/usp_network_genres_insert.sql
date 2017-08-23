-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/12/2015 01:25:58 PM
-- Description:	Auto-generated method to insert a network_genres record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_genres_insert]
	@network_id INT,
	@genre_id INT,
	@effective_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[network_genres]
	(
		[network_id],
		[genre_id],
		[effective_date]
	)
	VALUES
	(
		@network_id,
		@genre_id,
		@effective_date
	)
END
