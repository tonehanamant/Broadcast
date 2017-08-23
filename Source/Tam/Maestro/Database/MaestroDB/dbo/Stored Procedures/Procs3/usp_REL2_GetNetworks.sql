-- =============================================
-- Author:        John Carsley
-- Create date: 05/06/2014
-- Description:   Gets networks records by a table of network ids
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL2_GetNetworks]
(
		@network_ids AS UniqueIdTable READONLY
)
AS
BEGIN
		SELECT  n.id ,
				n.code ,
				n.name ,
				n.active ,
				n.flag ,
				n.effective_date,
				n.language_id,
				n.affiliated_network_id,
				n.network_type_id
		FROM dbo.networks AS n
		JOIN @network_ids AS idTable ON n.id = idTable.id
END
