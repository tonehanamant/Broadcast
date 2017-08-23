-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to insert a regions_histories record.
-- =============================================
create PROCEDURE dbo.usp_regions_histories_insert
	@id INT OUTPUT,
	@division_id INT,
	@code VARCHAR(15),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[regions_histories]
	(
		[division_id],
		[code],
		[start_date],
		[end_date]
	)
	VALUES
	(
		@division_id,
		@code,
		@start_date,
		@end_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END