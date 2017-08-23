	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 2/3/2015
	-- Description:	Get's the previouis media month given today's date.
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_MAS_GetPreviousMediaMonth]
	AS
	BEGIN
		SET NOCOUNT ON;
	
		DECLARE @media_month_id INT
		SELECT
			@media_month_id = mm.id
		FROM 
			media_months mm
		WHERE
			CAST(GETDATE() AS DATE) BETWEEN mm.start_date AND mm.end_date

		SELECT 
			mm.* 
		FROM 
			media_months mm (NOLOCK) 
		WHERE 
			mm.id=dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -1)
	END
