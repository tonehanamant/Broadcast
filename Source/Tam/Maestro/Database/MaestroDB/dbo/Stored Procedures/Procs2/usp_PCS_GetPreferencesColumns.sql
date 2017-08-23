-- =============================================
-- Author:		<Kheynis,Nick>
-- Create date: <12/16/2013>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPreferencesColumns]
		@employee_id INT,
		@application_code TINYINT,
		@object_class_code TINYINT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @count INT
	SELECT 
		@count = COUNT(1) 
	FROM 
		preferences_columns pc (NOLOCK)
	WHERE
		pc.employee_id = @employee_id and
		pc.application_code = @application_code and
		pc.object_class_code = @object_class_code
		
	IF @count > 0
	BEGIN
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
	ELSE
	BEGIN
		SELECT 
			@employee_id,
			dc.* 
		FROM 
			default_columns dc (NOLOCK)
		WHERE
			dc.application_code = @application_code and
			dc.object_class_code = @object_class_code
		ORDER BY
			dc.column_order
	END
END
