-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectOwnedByZoneBusinessByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		zone_id,
		business_id,
		type,
		effective_date
	FROM
		zone_businesses
	WHERE
		zone_id=@zone_id
		AND type='OWNEDBY'
END
