-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetTrafficStartDate
(
	@traffic_id INT
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @return DATETIME

	SET @return = (
		SELECT MIN(start_date) FROM traffic_flights WHERE traffic_id=@traffic_id
	)
	
	RETURN @return
END
