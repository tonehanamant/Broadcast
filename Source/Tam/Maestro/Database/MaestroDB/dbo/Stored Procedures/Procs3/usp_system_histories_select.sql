
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:52 AM
-- Description:	Auto-generated method to delete or potentionally disable a system_histories record.
-- =============================================
CREATE PROCEDURE usp_system_histories_select
	@system_id INT,
	@start_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[system_histories].*
	FROM
		[dbo].[system_histories] WITH(NOLOCK)
	WHERE
		[system_id]=@system_id
		AND [start_date]=@start_date
END

