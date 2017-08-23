-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/13/2014 05:19:04 PM
-- Description:	Auto-generated method to select all msa_deliveries records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_deliveries_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[msa_deliveries].*
	FROM
		[dbo].[msa_deliveries] WITH(NOLOCK)
END
