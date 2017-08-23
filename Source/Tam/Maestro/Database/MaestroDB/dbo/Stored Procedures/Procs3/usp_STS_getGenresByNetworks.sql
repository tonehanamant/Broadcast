-- =============================================
-- Author:		Brenton L Reeder
-- Create date: 6/11/2014
-- Modified:	8/12/2015 - Stephen DeFusco - Added NOLOCK
-- Description:	Gets the network genres by network id
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS_getGenresByNetworks]
	@network_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		g.name,
		ng.effective_date,
		g.id
	FROM
		network_genres ng (NOLOCK)
		JOIN genres g (NOLOCK) ON g.id=ng.genre_id
	WHERE
		ng.network_id=@network_id
END
