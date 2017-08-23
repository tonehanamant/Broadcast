CREATE PROCEDURE [dbo].[usp_affidavit_delivery_runs_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.affidavit_delivery_runs WITH(NOLOCK)
WHERE
	id = @id
