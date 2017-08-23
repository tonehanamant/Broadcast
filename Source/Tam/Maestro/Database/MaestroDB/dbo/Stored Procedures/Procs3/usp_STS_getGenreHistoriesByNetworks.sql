-- =============================================
-- Author:		Brenton L Reeder
-- Create date: 6/11/2014
-- Modified:	8/12/2015 - Stephen DeFusco - Missing no locks, specified fields in select list.
-- Description:	Gets the network genres by network id
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS_getGenreHistoriesByNetworks]
	@network_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		g.name,
		ngh.start_date,
		ngh.end_date,
		ngh.network_id,
		ngh.genre_id 
	FROM 
		network_genre_histories	ngh (NOLOCK)
		JOIN genres g (NOLOCK) on g.id=ngh.genre_id
	WHERE
		ngh.network_id=@network_id
END
