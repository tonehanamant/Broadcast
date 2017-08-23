-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/22/2013 10:07:03 AM
-- Description:	Auto-generated method to insert a mit_universes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_mit_universes_insert]
	@media_month_id INT,
	@rating_category_id INT,
	@nielsen_network_id INT,
	@start_date DATE,
	@end_date DATE,
	@id INT OUTPUT
AS
BEGIN
	INSERT INTO [dbo].[mit_universes]
	(
		[media_month_id],
		[rating_category_id],
		[nielsen_network_id],
		[start_date],
		[end_date]
	)
	VALUES
	(
		@media_month_id,
		@rating_category_id,
		@nielsen_network_id,
		@start_date,
		@end_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
