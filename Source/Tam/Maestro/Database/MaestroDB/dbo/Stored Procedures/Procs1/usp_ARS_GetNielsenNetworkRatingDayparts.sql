


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetNielsenNetworkRatingDayparts]
	@nielsen_network_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		nielsen_network_rating_dayparts.nielsen_network_id,
		nielsen_network_rating_dayparts.daypart_id,
		nielsen_network_rating_dayparts.effective_date
	FROM
		nielsen_network_rating_dayparts
	WHERE
		nielsen_network_rating_dayparts.nielsen_network_id=@nielsen_network_id
END



