-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to select a single divisions record.
-- =============================================
create PROCEDURE dbo.usp_divisions_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[divisions].*
	FROM
		[dbo].[divisions] WITH(NOLOCK)
	WHERE
		[id]=@id
END