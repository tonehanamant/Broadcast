
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTrafficDetailHHUniverse]
(
	@traffic_detail_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = 0.0
	
	SET @return = (
		select 
			tda2.us_universe 
		from 
			traffic_detail_audiences (NOLOCK) as tda2 
			join traffic_details (NOLOCK) as td2 
				on td2.id = tda2.traffic_detail_id and tda2.audience_id = 31
		WHERE
			td2.id = @traffic_detail_id
	)
	
	RETURN @return
END
