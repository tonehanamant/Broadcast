	

	-- =============================================
	-- Author:        Nicholas Kheynis
	-- Create date: 11/24/2014
	-- Description:   Get subscriber count by zone/network/effective date
	-- =============================================
	CREATE PROCEDURE usp_STS2_GetNetworkIdByCode
		  @network_code VARCHAR(15)
	AS
	BEGIN
		  SET NOCOUNT ON;

		SELECT
				n.id
		  FROM
				dbo.networks n
		  WHERE
				n.code = @network_code
	END

