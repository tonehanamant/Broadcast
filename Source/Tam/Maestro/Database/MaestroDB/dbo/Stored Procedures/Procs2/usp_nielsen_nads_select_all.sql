-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 09:10:36 AM
-- Description:	Auto-generated method to select all nielsen_nads records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_nielsen_nads_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[nielsen_nads].*
	FROM
		[dbo].[nielsen_nads] WITH(NOLOCK)
END
