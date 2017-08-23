-- =============================================
-- Author:		Brenton L Reeder
-- Create date: 7/15/2014
-- Description:	Gets the proposal category id from the category name
-- =============================================
CREATE PROCEDURE usp_PCS_GetProposalCategoryIdFromCategory
	@name varchar(15)
AS
BEGIN
	SET NOCOUNT ON;

    select 
		id
	from categories
		where 
			category_set = 'Proposal' and 
			name = @name
	order by name asc
END
