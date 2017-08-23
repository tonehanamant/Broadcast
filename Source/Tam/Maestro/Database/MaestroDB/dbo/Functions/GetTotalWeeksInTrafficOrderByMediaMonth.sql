-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTotalWeeksInTrafficOrderByMediaMonth]
(
	@traffic_id INT,
	@media_month_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT

	SET @return = (
		CAST
		(
			(
				SELECT COUNT(*) FROM traffic_flights tf (NOLOCK) WHERE 
					tf.traffic_id=@traffic_id 
					AND selected=1 
					AND (SELECT start_date FROM media_months WHERE id=@media_month_id) <= tf.start_date 
					AND (SELECT end_date   FROM media_months WHERE id=@media_month_id) >= tf.start_date
			) AS FLOAT
		)
	)
	
	RETURN @return
END
