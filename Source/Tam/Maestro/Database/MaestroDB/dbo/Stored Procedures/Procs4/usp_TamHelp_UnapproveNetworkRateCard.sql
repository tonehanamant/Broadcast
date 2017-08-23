
-- =============================================
-- Author:		jcarsley
-- Create date: 08/15/2013
-- Description:	Unnaproves/Unlocks a network rate card
-- =============================================
CREATE PROCEDURE usp_TamHelp_UnapproveNetworkRateCard 
	@rate_card_name varchar(150) 
AS
BEGIN

update network_rate_card_books
set approved_by_employee_id = null,
      date_approved = null
where name = @rate_card_name

END
