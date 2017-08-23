
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/15/2016 03:09:18 PM
-- Description:	Auto-generated method to insert a traffic_materials record.
-- =============================================
CREATE PROCEDURE usp_traffic_materials_insert
	@id INT OUTPUT,
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
	INSERT INTO [dbo].[traffic_materials]
	(
		[traffic_id],
		[material_id],
		[start_date],
		[end_date],
		[rotation],
		[disposition_id],
		[scheduling],
		[comment],
		[dr_phone],
		[internal_note_id],
		[external_note_id],
		[topography_id],
		[traffic_alert_spot_location],
		[sort_order],
		[reel_material_id],
		[hd_traffic_material_id]
	)
	VALUES
	(
		@traffic_id,
		@material_id,
		@start_date,
		@end_date,
		@rotation,
		@disposition_id,
		@scheduling,
		@comment,
		@dr_phone,
		@internal_note_id,
		@external_note_id,
		@topography_id,
		@traffic_alert_spot_location,
		@sort_order,
		@reel_material_id,
		@hd_traffic_material_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END

