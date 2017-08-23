CREATE PROCEDURE usp_STS2_GetZoneTypes
AS
BEGIN
	SET NOCOUNT ON;

    SELECT name
    FROM zone_types WITH (NOLOCK)
    ORDER BY name;
END
