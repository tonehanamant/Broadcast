
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/15/2016 03:09:18 PM
-- Description:	Auto-generated method to select a single traffic_materials record.
-- =============================================
CREATE PROCEDURE usp_traffic_materials_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_materials].*
	FROM
		[dbo].[traffic_materials] WITH(NOLOCK)
	WHERE
		[id]=@id
END
