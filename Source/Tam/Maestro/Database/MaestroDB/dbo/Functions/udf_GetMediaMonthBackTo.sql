-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/19/2014
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[udf_GetMediaMonthBackTo]
(
	@media_month_id INT,
	@num_months_to_go_back INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT;
	
	SELECT @return=id FROM (
		SELECT	
			mm.id, 
			ROW_NUMBER() OVER(ORDER BY start_date DESC) rownumber 
		FROM 
			media_months mm 
		WHERE 
			mm.start_date <= (SELECT start_date FROM media_months WHERE id=@media_month_id)
	) tmp
	WHERE 
		tmp.rownumber=@num_months_to_go_back;
		
	RETURN @return;
END
