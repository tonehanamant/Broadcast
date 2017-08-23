CREATE PROCEDURE [dbo].[usp_ACS_InsertPostedMediaMonth]
	@media_month_id INT,
	@complete BIT
AS
BEGIN
	INSERT INTO posted_media_months 
	(
		media_month_id,
		complete
	)
	VALUES
	(
		@media_month_id, 
		@complete
	)
END

