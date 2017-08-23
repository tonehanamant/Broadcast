

 

CREATE Procedure [dbo].[usp_TCS_GetNetworkClusterDetails]

      (

            @id Int

      )

AS

 

select network_clusters.network_id from network_clusters (NOLOCK) where 

network_clusters.cluster_id = @id
