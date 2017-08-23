CREATE PROCEDURE [dbo].[usp_TCS_audiences_select_all_formatted_name]
AS
BEGIN
	SELECT 
		a.id, 	
		a.name
	FROM
		audiences a (NOLOCK)
END
