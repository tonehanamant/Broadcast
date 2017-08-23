-- =============================================
-- Author:		CRUD Creator
-- Create date: 02/26/2016 11:22:45 AM
-- Description:	Auto-generated method to select all proposal_inventory_check_details records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_check_details_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_inventory_check_details].*
	FROM
		[dbo].[proposal_inventory_check_details] WITH(NOLOCK)
END