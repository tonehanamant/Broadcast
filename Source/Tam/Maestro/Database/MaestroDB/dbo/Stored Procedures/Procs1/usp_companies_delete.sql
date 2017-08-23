-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/11/2014 10:31:07 AM
-- Description:	Auto-generated method to delete a single companies record.
-- =============================================
CREATE PROCEDURE usp_companies_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[companies]
	WHERE
		[id]=@id
END

