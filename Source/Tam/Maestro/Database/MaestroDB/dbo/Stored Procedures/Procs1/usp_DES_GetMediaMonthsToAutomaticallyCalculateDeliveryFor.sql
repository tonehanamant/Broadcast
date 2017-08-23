-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/3/2012
-- Description:	
-- =============================================
-- usp_DES_GetMediaMonthsToAutomaticallyCalculateDeliveryFor
CREATE PROCEDURE [dbo].[usp_DES_GetMediaMonthsToAutomaticallyCalculateDeliveryFor]
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @media_month_id INT
	DECLARE @start_date DATETIME
	DECLARE @current_date DATETIME

	SET @current_date = GETDATE();
	SET @start_date = (SELECT mm.start_date FROM media_months mm (NOLOCK) WHERE @current_date BETWEEN mm.start_date AND mm.end_date);

	DECLARE @in_week_3 BIT
	SET @in_week_3 = (SELECT COUNT(*) FROM media_weeks mw (NOLOCK) WHERE @current_date BETWEEN mw.start_date AND mw.end_date AND mw.week_number>=3);

	IF @in_week_3 = 0
		BEGIN
			SET @start_date = (SELECT mm.start_date FROM media_months mm (NOLOCK) WHERE DATEADD(day,-40,@start_date) BETWEEN mm.start_date AND mm.end_date);
			SET @media_month_id = (SELECT mm.id FROM media_months mm (NOLOCK) WHERE @start_date BETWEEN mm.start_date AND mm.end_date);
		END
	ELSE
		BEGIN
			SET @media_month_id = (SELECT mm.id FROM media_months mm (NOLOCK) WHERE DATEADD(day,-1,@start_date) BETWEEN mm.start_date AND mm.end_date);
		END;

	SELECT
		mm.*
	FROM
		dbo.media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id

	SET NOCOUNT OFF;
END
