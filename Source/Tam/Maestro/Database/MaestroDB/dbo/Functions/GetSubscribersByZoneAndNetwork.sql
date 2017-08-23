-- =============================================
-- Author:		Stephen DeFusco
-- Create date: ?
-- Modified:	2/17/2017
-- Description:	Get's sub count for a zone/network on a specific date while using a special case for regional sports nets in which it takes the max sub count of any regional sports net in the zone.
-- =============================================
CREATE FUNCTION [dbo].[GetSubscribersByZoneAndNetwork]
(
	@zone_id INT,
	@network_id INT,
	@effective_date DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	DECLARE @regional_networks TABLE (substitute_network_id INT NOT NULL);
	INSERT INTO @regional_networks
		SELECT CAST(nm.map_value AS INT) FROM uvw_networkmap_universe nm (NOLOCK) WHERE (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL)) AND nm.map_set='PostReplace' AND nm.network_id=@network_id

	IF (SELECT COUNT(1) FROM @regional_networks n) > 0
		BEGIN
			SET @return = (
				SELECT
					MAX(subscribers) AS subs
				FROM
					dbo.uvw_zonenetwork_universe zn (NOLOCK)
					INNER JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id
				WHERE
					zn.zone_id=@zone_id
					AND ((@effective_date IS NULL AND zn.end_date IS NULL)	OR (zn.start_date<=@effective_date	AND (zn.end_date>=@effective_date	OR zn.end_date	IS NULL)))
					AND ((@effective_date IS NULL AND z.end_date IS NULL)	OR (z.start_date<=@effective_date	AND (z.end_date>=@effective_date	OR z.end_date	IS NULL)))
					AND (
						network_id IN (SELECT substitute_network_id FROM @regional_networks) 
						OR network_id=@network_id
					)
				)
		END
	ELSE
		BEGIN
			SET @return = (
				SELECT
					MAX(subscribers) AS subs
				FROM
					dbo.uvw_zonenetwork_universe zn (NOLOCK)
					INNER JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id
				WHERE
					zn.zone_id=@zone_id
					AND network_id=@network_id
					AND ((@effective_date IS NULL AND zn.end_date IS NULL)	OR (zn.start_date<=@effective_date	AND (zn.end_date>=@effective_date	OR zn.end_date	IS NULL)))
					AND ((@effective_date IS NULL AND z.end_date IS NULL)	OR (z.start_date<=@effective_date	AND (z.end_date>=@effective_date	OR z.end_date	IS NULL)))
				)
		END

	RETURN ISNULL(@return,0)
END
