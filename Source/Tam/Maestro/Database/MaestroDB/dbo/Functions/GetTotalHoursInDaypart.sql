-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTotalHoursInDaypart]
(
	@daypart_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT

	SET @return = (
		SELECT 
			CASE 
				WHEN end_time<start_time THEN 
					(((end_time+86401)-start_time)*(mon+tue+wed+thu+fri+sat+sun))/3600.0
				ELSE 
					(((end_time+1)-start_time)*(mon+tue+wed+thu+fri+sat+sun))/3600.0
			END 
		FROM 
			vw_ccc_daypart 
		WHERE 
			id=@daypart_id
	)
	
	RETURN @return
END
