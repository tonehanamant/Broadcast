-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to select all divisions records.
-- =============================================
create PROCEDURE dbo.usp_divisions_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[divisions].*
	FROM
		[dbo].[divisions] WITH(NOLOCK)
END