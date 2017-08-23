	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 02/18/2015 01:43:29 PM
	-- Description:	Auto-generated method to select a single affidavit_files record.
	-- =============================================
	CREATE PROCEDURE usp_affidavit_files_select
		@id Int
	AS
	BEGIN
		SET NOCOUNT ON;

		SELECT
			[affidavit_files].*
		FROM
			[dbo].[affidavit_files] WITH(NOLOCK)
		WHERE
			[id]=@id
	END
