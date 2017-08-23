-- =============================================
-- Author:		Stephen DeFusco
-- Created:		8/12/2015
-- Description:	Gets the network genres histories entity by network id
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetNetworkGenreHistoriesByNetworkId]
	@network_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		ngh.*
	FROM
		network_genre_histories ngh (NOLOCK)
	WHERE
		ngh.network_id=@network_id
END