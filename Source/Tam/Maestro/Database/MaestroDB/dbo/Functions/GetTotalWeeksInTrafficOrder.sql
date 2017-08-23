
CREATE FUNCTION GetTotalWeeksInTrafficOrder
(
	@traffic_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return AS FLOAT

	SET @return = (
		CAST
		(
			(
				SELECT 
					COUNT(*) 
				FROM 
					traffic_flights tf (NOLOCK) 
				WHERE 
					tf.traffic_id=@traffic_id 
					AND selected=1
			) AS FLOAT
		)
	)
	
	RETURN @return
END
