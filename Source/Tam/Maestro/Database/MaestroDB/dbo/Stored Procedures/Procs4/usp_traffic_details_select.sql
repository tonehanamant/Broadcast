
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2015 12:15:41 AM
-- Description:	Auto-generated method to select a single traffic_details record.
-- =============================================
CREATE PROCEDURE usp_traffic_details_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_details].*
	FROM
		[dbo].[traffic_details] WITH(NOLOCK)
	WHERE
		[id]=@id
END
