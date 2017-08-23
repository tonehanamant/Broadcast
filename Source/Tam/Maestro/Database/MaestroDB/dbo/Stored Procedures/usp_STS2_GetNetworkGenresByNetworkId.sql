-- =============================================
-- Author:		Stephen DeFusco
-- Created:		8/12/2015
-- Description:	Gets the network genres entity by network id
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetNetworkGenresByNetworkId]
	@network_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		ng.*
	FROM
		network_genres ng (NOLOCK)
	WHERE
		ng.network_id=@network_id
END