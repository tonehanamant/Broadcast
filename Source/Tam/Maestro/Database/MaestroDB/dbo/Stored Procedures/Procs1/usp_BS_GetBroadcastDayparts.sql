CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastDayparts]

AS
SELECT 
	m.id,
	m.name,
	m.description
FROM
	broadcast_dayparts m WITH (NOLOCK)
ORDER BY
	m.id
		
