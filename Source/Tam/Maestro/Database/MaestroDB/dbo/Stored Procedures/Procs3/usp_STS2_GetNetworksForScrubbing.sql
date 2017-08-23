	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 7/9/2010
	-- Description:	<Description,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_STS2_GetNetworksForScrubbing]
	AS
	BEGIN
		SET NOCOUNT ON;

		DECLARE @current_date AS DATETIME
		SET @current_date = CONVERT(varchar, GetDate(), 101)
	
		SELECT
			n.network_id,
			n.code,
			n.name,
			n.active,
			n.flag,
			n.start_date,
			n.language_id,
			n.affiliated_network_id,
			n.network_type_id
		FROM
			uvw_network_universe n
		WHERE
			n.start_date<=@current_date AND (n.end_date>=@current_date OR n.end_date IS NULL)
		ORDER BY
			n.code
	END

