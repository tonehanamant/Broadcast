CREATE PROCEDURE usp_tam_post_excluded_affidavit_systems_delete
(
	@tam_post_excluded_affidavit_id		Int,
	@system_id		Int)
AS
DELETE FROM
	tam_post_excluded_affidavit_systems
WHERE
	tam_post_excluded_affidavit_id = @tam_post_excluded_affidavit_id
 AND
	system_id = @system_id
