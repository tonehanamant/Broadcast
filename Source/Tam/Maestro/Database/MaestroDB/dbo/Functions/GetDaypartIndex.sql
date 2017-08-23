-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION dbo.GetDaypartIndex
(
	@air_time INT
)
RETURNS TINYINT
AS
BEGIN
	DECLARE @return TINYINT
	
	SET @return = (
		CASE 
			WHEN @air_time BETWEEN 0	 AND 21600 THEN 4
			WHEN @air_time BETWEEN 21601 AND 43200 THEN 1
			WHEN @air_time BETWEEN 43201 AND 64800 THEN 2
			WHEN @air_time BETWEEN 64801 AND 86400 THEN 3
			ELSE 0
		END
	)
	RETURN @return;
END
