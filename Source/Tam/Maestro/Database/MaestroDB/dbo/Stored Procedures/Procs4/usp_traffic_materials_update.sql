
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/15/2016 03:09:18 PM
-- Description:	Auto-generated method to update a traffic_materials record.
-- =============================================
CREATE PROCEDURE usp_traffic_materials_update
	@id INT,
	@traffic_id INT,
	@material_id INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@rotation INT,
	@disposition_id INT,
	@scheduling VARCHAR(63),
	@comment VARCHAR(255),
	@dr_phone VARCHAR(15),
	@internal_note_id INT,
	@external_note_id INT,
	@topography_id INT,
	@traffic_alert_spot_location VARCHAR(63),
	@sort_order INT,
	@reel_material_id INT,
	@hd_traffic_material_id INT
AS
BEGIN
	UPDATE
		[dbo].[traffic_materials]
	SET
		[traffic_id]=@traffic_id,
		[material_id]=@material_id,
		[start_date]=@start_date,
		[end_date]=@end_date,
		[rotation]=@rotation,
		[disposition_id]=@disposition_id,
		[scheduling]=@scheduling,
		[comment]=@comment,
		[dr_phone]=@dr_phone,
		[internal_note_id]=@internal_note_id,
		[external_note_id]=@external_note_id,
		[topography_id]=@topography_id,
		[traffic_alert_spot_location]=@traffic_alert_spot_location,
		[sort_order]=@sort_order,
		[reel_material_id]=@reel_material_id,
		[hd_traffic_material_id]=@hd_traffic_material_id
	WHERE
		[id]=@id
END

