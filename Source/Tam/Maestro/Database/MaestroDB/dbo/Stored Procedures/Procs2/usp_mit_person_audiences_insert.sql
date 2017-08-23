-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/22/2013 10:06:56 AM
-- Description:	Auto-generated method to insert a mit_person_audiences record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_mit_person_audiences_insert]
	@media_month_id INT,
	@mit_rating_id INT,
	@audience_id INT,
	@type VARCHAR(15),
	@usage FLOAT,
	@effective_date DATE
AS
BEGIN
	INSERT INTO [dbo].[mit_person_audiences]
	(
		[media_month_id],
		[mit_rating_id],
		[audience_id],
		[type],
		[usage],
		[effective_date]
	)
	VALUES
	(
		@media_month_id,
		@mit_rating_id,
		@audience_id,
		@type,
		@usage,
		@effective_date
	)
END
