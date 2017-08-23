	-- =============================================
	-- Author:        Stephen DeFusco
	-- Create date: 11/21/2014
	-- Description:   Get subscriber count by zone/network/effective date
	-- =============================================
	CREATE PROCEDURE usp_STS2_lookupZoneNetworkSubscribers
		  @zone_id INT,
		  @network_id INT,
		  @effective_date DATETIME
	AS
	BEGIN
		  SET NOCOUNT ON;

		SELECT
				zn.subscribers
		  FROM
				dbo.uvw_zonenetwork_universe zn
		  WHERE
				zn.zone_id=@zone_id
				AND zn.network_id=@network_id
				AND zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL) 
	END

