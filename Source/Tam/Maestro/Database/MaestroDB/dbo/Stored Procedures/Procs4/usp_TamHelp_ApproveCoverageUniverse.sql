
-- =============================================
-- Author:		jcarsley
-- Create date: 08/28/2013
-- Description:	Unnaproves/Unlocks a coverage universe
-- =============================================
CREATE PROCEDURE [dbo].[usp_TamHelp_ApproveCoverageUniverse] 
	@media_month varchar(150),
	@sales_model_id int,
	@windows_user_name varchar(50)
AS
BEGIN

declare @employee_id int

Select @employee_id = id
from employees emp
where emp.username = @windows_user_name

update coverage_universes
set approved_by_employee_id =  @employee_id,
      date_approved = getdate()
from media_months mm
     join coverage_universes cu on mm.id = cu.base_media_month_id
where
	mm.media_month = @media_month
	and sales_model_id = @sales_model_id

END

