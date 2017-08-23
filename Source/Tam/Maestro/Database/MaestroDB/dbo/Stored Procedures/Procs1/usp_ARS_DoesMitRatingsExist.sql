-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARS_DoesMitRatingsExist
	@media_month_id INT,
	@rating_category_id INT,
	@nielsen_network_id INT,
	@feed_type VARCHAR(15),
	@rating_date DATE,
	@start_time INT,
	@end_time INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		ISNULL(mr.id,-1) 
	FROM 
		mit_ratings mr (NOLOCK) 
	WHERE 
		mr.media_month_id=@media_month_id 
		AND mr.rating_category_id=@rating_category_id 
		AND nielsen_network_id=@nielsen_network_id 
		AND rating_date=@rating_date 
		AND start_time=@start_time
		AND end_time=@end_time
		AND feed_type=@feed_type 
END
