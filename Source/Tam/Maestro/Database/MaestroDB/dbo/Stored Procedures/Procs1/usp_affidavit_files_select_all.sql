	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 02/18/2015 01:43:27 PM
	-- Description:	Auto-generated method to select all affidavit_files records.
	-- =============================================
	CREATE PROCEDURE usp_affidavit_files_select_all
	AS
	BEGIN
		SET NOCOUNT ON;

		SELECT
			[affidavit_files].*
		FROM
			[dbo].[affidavit_files] WITH(NOLOCK)
	END
