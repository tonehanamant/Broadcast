-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2014 09:28:41 AM
-- Description:	Auto-generated method to delete a single msa_delivery_files record.
-- =============================================
CREATE PROCEDURE usp_msa_delivery_files_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[msa_delivery_files]
	WHERE
		[id]=@id
END
