-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to update a regions record.
-- =============================================
create PROCEDURE dbo.usp_regions_update
	@id INT,
	@division_id INT,
	@code VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[regions]
	SET
		[division_id]=@division_id,
		[code]=@code,
		[effective_date]=@effective_date
	WHERE
		[id]=@id
END