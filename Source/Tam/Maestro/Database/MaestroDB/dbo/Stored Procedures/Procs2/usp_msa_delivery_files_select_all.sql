-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2014 09:28:41 AM
-- Description:	Auto-generated method to select all msa_delivery_files records.
-- =============================================
CREATE PROCEDURE usp_msa_delivery_files_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[msa_delivery_files].*
	FROM
		[dbo].[msa_delivery_files] WITH(NOLOCK)
END
