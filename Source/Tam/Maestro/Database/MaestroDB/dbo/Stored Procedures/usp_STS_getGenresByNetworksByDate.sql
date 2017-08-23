-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/12/2015
-- Description:	Gets the network genres by network id and effective date
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS_getGenresByNetworksByDate]
	@network_id INT,
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		g.name,
		ng.start_date 'effective_date',
		g.id 
	FROM
		uvw_network_genre_universe ng (NOLOCK)
		JOIN genres g (NOLOCK) ON g.id=ng.genre_id
	where
		ng.network_id=@network_id
		AND (ng.start_date<=@effective_date AND (ng.end_date>=@effective_date OR ng.end_date IS NULL))
END