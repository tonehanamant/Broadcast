

 

 

CREATE Procedure [dbo].[usp_TCS_GetNetworkClusters]

      (

            @topography_id Int

      )

AS

 

 

select clusters.id, clusters.name from clusters (NOLOCK), cluster_topography_map (NOLOCK) where 

clusters.id = cluster_topography_map.cluster_id and 

cluster_topography_map.topography_id = @topography_id

