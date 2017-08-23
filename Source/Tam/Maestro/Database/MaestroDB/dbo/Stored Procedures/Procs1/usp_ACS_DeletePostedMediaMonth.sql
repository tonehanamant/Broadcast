CREATE PROCEDURE [dbo].[usp_ACS_DeletePostedMediaMonth]
(
	@media_month_id INT
)
AS
BEGIN
	DELETE FROM posted_media_months WHERE media_month_id=@media_month_id
END


