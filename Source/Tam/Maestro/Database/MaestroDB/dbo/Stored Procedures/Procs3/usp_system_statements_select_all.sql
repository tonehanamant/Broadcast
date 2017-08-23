-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/30/2015 02:31:01 PM
-- Description:	Auto-generated method to select all system_statements records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_system_statements_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[system_statements].*
	FROM
		[dbo].[system_statements] WITH(NOLOCK)
END
