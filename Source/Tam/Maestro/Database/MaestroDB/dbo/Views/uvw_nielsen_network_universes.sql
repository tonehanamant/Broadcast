CREATE VIEW [dbo].[uvw_nielsen_network_universes]
AS
SELECT    id as nielsen_network_id, network_rating_category_id, nielsen_id, code, [name], active, 
effective_date AS [start_date], NULL AS end_date
FROM     dbo.nielsen_networks   with (NOLOCK)
UNION ALL
SELECT    nielsen_network_id,  network_rating_category_id, nielsen_id, code, [name], active, 
[start_date], end_date 
FROM     dbo.nielsen_network_histories  with (NOLOCK)