-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2014 09:28:41 AM
-- Description:	Auto-generated method to select a single msa_delivery_files record.
-- =============================================
CREATE PROCEDURE usp_msa_delivery_files_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[msa_delivery_files].*
	FROM
		[dbo].[msa_delivery_files] WITH(NOLOCK)
	WHERE
		[id]=@id
END
