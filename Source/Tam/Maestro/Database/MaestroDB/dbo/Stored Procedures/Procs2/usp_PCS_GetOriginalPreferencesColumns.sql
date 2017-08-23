-- =============================================
-- Author:		<Kheynis,Nick>
-- Create date: <12/16/2013>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetOriginalPreferencesColumns]
		@employee_id INT,
		@application_code TINYINT,
		@object_class_code TINYINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		pc.* 
	FROM 
		preferences_columns pc (NOLOCK)
	WHERE
		pc.employee_id = @employee_id and
		pc.application_code = @application_code and
		pc.object_class_code = @object_class_code
	ORDER BY
		pc.column_order
END
