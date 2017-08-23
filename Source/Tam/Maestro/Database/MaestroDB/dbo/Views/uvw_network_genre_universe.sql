CREATE VIEW [dbo].[uvw_network_genre_universe]
AS
	SELECT     
		network_id, genre_id, effective_date AS start_date, NULL AS end_date
	FROM
		dbo.network_genres (NOLOCK)
	UNION ALL
	SELECT
		network_id, genre_id, start_date, end_date
	FROM
		dbo.network_genre_histories (NOLOCK)