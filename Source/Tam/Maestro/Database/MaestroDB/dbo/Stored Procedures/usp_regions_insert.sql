-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to insert a regions record.
-- =============================================
create PROCEDURE dbo.usp_regions_insert
	@id INT OUTPUT,
	@division_id INT,
	@code VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[regions]
	(
		[division_id],
		[code],
		[effective_date]
	)
	VALUES
	(
		@division_id,
		@code,
		@effective_date
	)

	SELECT
		@id = SCOPE_IDENTITY()
END