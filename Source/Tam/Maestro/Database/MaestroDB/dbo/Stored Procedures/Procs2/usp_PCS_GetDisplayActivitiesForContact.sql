-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayActivitiesForContact]
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		activities.id,
		activity_types.name,
		activities.activity_type_id, 
		activities.employee_id,
		employees.firstname,
		employees.lastname,
		activities.comment,
		activities.date_created, 
		activities.date_last_modified 
	FROM 
		activities (NOLOCK) 
		JOIN activity_types (NOLOCK) ON activity_types.id=activities.activity_type_id 
		LEFT JOIN employees (NOLOCK) ON employees.id=activities.employee_id 
	WHERE 
		contact_id=@contact_id 
	ORDER BY 
		activities.date_created DESC
END
