
-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to select a single regions record.
-- =============================================
create PROCEDURE dbo.usp_regions_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[regions].*
	FROM
		[dbo].[regions] WITH(NOLOCK)
	WHERE
		[id]=@id
END