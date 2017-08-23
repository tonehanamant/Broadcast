-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/30/2015 02:31:02 PM
-- Description:	Auto-generated method to select a single system_statements record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_system_statements_select]
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[system_statements].*
	FROM
		[dbo].[system_statements] WITH(NOLOCK)
	WHERE
		[id]=@id
END
