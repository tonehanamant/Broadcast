-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to update a divisions record.
-- =============================================
create PROCEDURE dbo.usp_divisions_update
	@id INT,
	@zone_id INT,
	@code VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[divisions]
	SET
		[zone_id]=@zone_id,
		[code]=@code,
		[effective_date]=@effective_date
	WHERE
		[id]=@id
END