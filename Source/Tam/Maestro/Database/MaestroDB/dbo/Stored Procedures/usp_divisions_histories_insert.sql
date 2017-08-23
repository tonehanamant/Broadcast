-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to insert a divisions_histories record.
-- =============================================
create PROCEDURE dbo.usp_divisions_histories_insert
	@id INT OUTPUT,
	@zone_id INT,
	@code VARCHAR(15),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[divisions_histories]
	(
		[zone_id],
		[code],
		[start_date],
		[end_date]
	)
	VALUES
	(
		@zone_id,
		@code,
		@start_date,
		@end_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END