CREATE PROCEDURE ups_REL_GetHouseIsciiForMaterial
(
	@year int,
	@material_id int
)
AS
BEGIN
	
	SELECT mhm.houseiscii from materials_houseiscii_map mhm with (NOLOCK) where
		mhm.material_id = @material_id;
	
END
