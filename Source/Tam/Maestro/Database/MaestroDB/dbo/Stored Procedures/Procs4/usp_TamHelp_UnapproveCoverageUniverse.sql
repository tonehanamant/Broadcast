-- =============================================
-- Author:		jcarsley
-- Create date: 08/28/2013
-- Description:	Unnaproves/Unlocks a coverage universe
-- =============================================
CREATE PROCEDURE [dbo].[usp_TamHelp_UnapproveCoverageUniverse] 
	@media_month varchar(150),
	@sales_model_id int
AS
BEGIN

update coverage_universes
set approved_by_employee_id = null,
      date_approved = null
from media_months mm
     join coverage_universes cu on mm.id = cu.base_media_month_id
where
	mm.media_month = @media_month
	and sales_model_id = @sales_model_id

END
