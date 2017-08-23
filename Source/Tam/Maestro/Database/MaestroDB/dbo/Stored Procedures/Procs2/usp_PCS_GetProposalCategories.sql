-- =============================================
-- Author:		Brenton L Reeder
-- Create date: 7/15/2014
-- Description:	Gets the proposal categories and IDs for UI
-- =============================================
CREATE PROCEDURE usp_PCS_GetProposalCategories
AS
BEGIN
	SET NOCOUNT ON;

    select 
		*
	from categories
		where 
			category_set = 'Proposal'
	order by name asc
END
