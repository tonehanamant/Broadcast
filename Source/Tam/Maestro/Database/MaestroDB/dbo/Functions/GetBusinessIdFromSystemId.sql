-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetBusinessIdFromSystemId
(
	@system_id INT,
	@effective_date DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	SET @return = (
		SELECT 
			TOP 1 business_id 
		FROM 
			uvw_systemzone_universe sz 
			JOIN uvw_zonebusiness_universe zb (NOLOCK) ON zb.zone_id=sz.zone_id AND (zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL)) AND zb.type='MANAGEDBY'
		WHERE
			sz.system_id=@system_id
			AND (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)) 
			AND sz.type='BILLING'
	)

	RETURN @return;
END
