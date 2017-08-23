
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTrafficWeeksOverlappingMonth]
(
	@media_month_id INT,
	@traffic_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return AS INT
	
	SET @return = 0
	
	SET @return = (
		SELECT
			COUNT(*)
		FROM
			traffic_flights tf (NOLOCK)
			JOIN media_months mm (NOLOCK) ON mm.start_date <= tf.end_date AND mm.end_date >= tf.start_date
		WHERE 
			mm.id=@media_month_id 
			AND tf.selected=1
			AND tf.traffic_id=@traffic_id
	)
	
	RETURN @return
END

