-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/04/2016 03:27:10 PM
-- Description:	Auto-generated method to select all proposal_inventory_checks records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_checks_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_inventory_checks].*
	FROM
		[dbo].[proposal_inventory_checks] WITH(NOLOCK)
END