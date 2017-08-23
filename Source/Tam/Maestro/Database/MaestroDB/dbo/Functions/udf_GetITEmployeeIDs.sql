
-- =============================================
-- Author:		David Sisson
-- Create date: 7/26/2011
-- Description:	User-defined table function returning employees in the IT department
-- =============================================
CREATE FUNCTION [dbo].[udf_GetITEmployeeIDs] 
(	
)
RETURNS TABLE 
AS
RETURN 
(
	select
		id employee_id
	from
		employees
	where
		username in ('tweber', 'dsisson', 'sdefusco', 'jjacobs', 'wfeng', 'rsara', 'jcarsley', 'rgibson')
);

