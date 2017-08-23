
-- =============================================
-- Author:		jcarsley
-- Create date: 08/15/2013
-- Description:	Approves/Locks a network rate card
-- =============================================
CREATE PROCEDURE usp_TamHelp_ApproveNetworkRateCard 
	@rate_card_name varchar(150),
	@windows_user_name varchar(50)
AS
BEGIN

declare @employee_id int

Select @employee_id = id
from employees emp
where emp.username = @windows_user_name

update network_rate_card_books
set approved_by_employee_id = @employee_id,
      date_approved = getdate()
where name = @rate_card_name

END
