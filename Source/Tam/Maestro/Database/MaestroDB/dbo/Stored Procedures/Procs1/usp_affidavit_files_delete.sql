	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 02/18/2015 01:43:29 PM
	-- Description:	Auto-generated method to delete a single affidavit_files record.
	-- =============================================
	CREATE PROCEDURE usp_affidavit_files_delete
		@id INT
	AS
	BEGIN
		DELETE FROM
			[dbo].[affidavit_files]
		WHERE
			[id]=@id
	END
