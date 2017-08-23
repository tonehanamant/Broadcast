-- =============================================
-- Author:		Stephen DeFusco and Nicholas Kheynis
-- Create date: 5-22-14
-- Description:	<Description,,>
-- =============================================
-- SELECT * FROM dbo.GetRotationReport('0711')
CREATE FUNCTION [dbo].[GetSalesModelsForEmployee]
(	
	@employee_id INT
)
RETURNS @return TABLE
(
	sales_model_id INT
)
AS
BEGIN
	-- CABLE
	IF (SELECT COUNT(1) FROM dbo.employee_roles er WHERE er.employee_id = @employee_id AND er.role_id = 57) > 0
	
	BEGIN
	INSERT INTO @return SELECT 1 
	INSERT INTO @return SELECT 6 
	END
	
	-- HISPANIC
	IF (SELECT COUNT(1) FROM dbo.employee_roles er WHERE er.employee_id = @employee_id AND er.role_id = 58) > 0
	
	BEGIN
	INSERT INTO @return SELECT 2 
	INSERT INTO @return SELECT 3 
	END
	
	-- NATIONAL DR
	IF (SELECT COUNT(1) FROM dbo.employee_roles er WHERE er.employee_id = @employee_id AND er.role_id = 59) > 0
	
	BEGIN
	INSERT INTO @return SELECT 4 
	END
	
	-- IMW
	IF (SELECT COUNT(1) FROM dbo.employee_roles er WHERE er.employee_id = @employee_id AND er.role_id = 60) > 0
	
	BEGIN
	INSERT INTO @return SELECT 5 
	END
	
	RETURN;
END
