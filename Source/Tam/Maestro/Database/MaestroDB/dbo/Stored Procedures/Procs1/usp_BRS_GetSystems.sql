CREATE PROCEDURE [dbo].[usp_BRS_GetSystems]

AS
BEGIN

	SET NOCOUNT ON;

	SELECT 
		s.id,
		s.code + ' (' + s.name + ')'
	FROM
		systems s (NOLOCK)
		JOIN system_maps sm (NOLOCK) on sm.system_id = s.id
	WHERE
		sm.map_set = 'cmw_traffic'
	ORDER BY
		s.code
END