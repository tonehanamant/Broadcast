CREATE PROCEDURE [dbo].[usp_mit_person_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.mit_person_audiences WITH(NOLOCK)
