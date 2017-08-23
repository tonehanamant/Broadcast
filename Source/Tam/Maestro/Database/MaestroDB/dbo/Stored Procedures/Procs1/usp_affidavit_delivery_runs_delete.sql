CREATE PROCEDURE [dbo].[usp_affidavit_delivery_runs_delete]
(
	@id Int
)
AS
DELETE FROM dbo.affidavit_delivery_runs WHERE id=@id
