CREATE PROCEDURE [dbo].[usp_ACS_UnscrubCopy]
	@material_id INT,
	@media_month_id INT,
	@affidavit_copy VARCHAR(63)
AS
BEGIN
	UPDATE
		a
	SET
		a.material_id=NULL,
		a.hash=NULL,
		a.status_id=2
	FROM
		affidavits a
	WHERE
		a.media_month_id=@media_month_id
		AND a.material_id=@material_id
		AND a.affidavit_copy=@affidavit_copy

	DELETE FROM material_map_histories WHERE material_id=@material_id AND map_value=@affidavit_copy
	DELETE FROM material_maps WHERE material_id=@material_id AND map_value=@affidavit_copy
END
