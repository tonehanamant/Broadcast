-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetYears]
AS
BEGIN
	(
		SELECT
			DISTINCT YEAR(m.date_received) 'year'
		FROM
			dbo.materials m (NOLOCK)
		WHERE
			m.date_received IS NOT NULL
		
		UNION

		SELECT
			DISTINCT YEAR(m.date_created) 'year'
		FROM
			dbo.materials m (NOLOCK)
	)
	ORDER BY
		year DESC
END
