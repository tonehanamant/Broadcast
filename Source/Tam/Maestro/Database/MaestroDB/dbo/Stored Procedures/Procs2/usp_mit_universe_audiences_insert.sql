-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/22/2013 10:07:01 AM
-- Description:	Auto-generated method to insert a mit_universe_audiences record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_mit_universe_audiences_insert]
	@media_month_id INT,
	@mit_universe_id INT,
	@audience_id INT,
	@universe FLOAT,
	@effective_date DATE
AS
BEGIN
	INSERT INTO [dbo].[mit_universe_audiences]
	(
		[media_month_id],
		[mit_universe_id],
		[audience_id],
		[universe],
		[effective_date]
	)
	VALUES
	(
		@media_month_id,
		@mit_universe_id,
		@audience_id,
		@universe,
		@effective_date
	)
END
