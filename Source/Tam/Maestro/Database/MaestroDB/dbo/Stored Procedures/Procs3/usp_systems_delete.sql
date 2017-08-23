
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:53 AM
-- Description:	Auto-generated method to delete a single systems record.
-- =============================================
CREATE PROCEDURE usp_systems_delete
	@id INT,
	@effective_date DATETIME
AS
BEGIN
	UPDATE
		dbo.systems
	SET
		[systems].[active]=0,
		[systems].[effective_date]=@effective_date
	WHERE
		[id]=@id
END
