CREATE PROCEDURE usp_tam_post_excluded_affidavit_systems_insert
(
	@tam_post_excluded_affidavit_id		Int,
	@system_id		Int
)
AS
INSERT INTO tam_post_excluded_affidavit_systems
(
	tam_post_excluded_affidavit_id,
	system_id
)
VALUES
(
	@tam_post_excluded_affidavit_id,
	@system_id
)

