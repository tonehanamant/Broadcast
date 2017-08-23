-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to select a single regions_histories record.
-- =============================================
create PROCEDURE dbo.usp_regions_histories_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[regions_histories].*
	FROM
		[dbo].[regions_histories] WITH(NOLOCK)
	WHERE
		[id]=@id
END