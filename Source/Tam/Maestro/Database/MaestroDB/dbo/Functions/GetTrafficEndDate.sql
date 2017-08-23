-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetTrafficEndDate
(
	@traffic_id INT
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @return DATETIME

	SET @return = (
		SELECT MAX(end_date) FROM traffic_flights WHERE traffic_id=@traffic_id
	)
	
	RETURN @return
END
