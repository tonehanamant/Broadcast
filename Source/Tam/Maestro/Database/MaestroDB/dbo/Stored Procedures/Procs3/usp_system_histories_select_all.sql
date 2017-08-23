
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:52 AM
-- Description:	Auto-generated method to select all system_histories records.
-- =============================================
CREATE PROCEDURE usp_system_histories_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[system_histories].*
	FROM
		[dbo].[system_histories] WITH(NOLOCK)
END
