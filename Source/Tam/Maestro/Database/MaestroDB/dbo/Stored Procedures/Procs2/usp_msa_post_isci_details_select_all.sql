CREATE PROCEDURE [dbo].[usp_msa_post_isci_details_select_all]
AS
SELECT
	*
FROM
	dbo.msa_post_isci_details WITH(NOLOCK)
