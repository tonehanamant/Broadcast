-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to insert a divisions record.
-- =============================================
create PROCEDURE dbo.usp_divisions_insert
	@id INT OUTPUT,
	@zone_id INT,
	@code VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[divisions]
	(
		[zone_id],
		[code],
		[effective_date]
	)
	VALUES
	(
		@zone_id,
		@code,
		@effective_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END