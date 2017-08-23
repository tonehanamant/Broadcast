CREATE PROCEDURE [dbo].[usp_affidavit_delivery_runs_select_all]
AS
SELECT
	*
FROM
	dbo.affidavit_delivery_runs WITH(NOLOCK)
