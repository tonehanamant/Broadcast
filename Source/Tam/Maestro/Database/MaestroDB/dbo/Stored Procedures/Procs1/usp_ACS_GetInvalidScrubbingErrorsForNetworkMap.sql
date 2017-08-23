-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetInvalidScrubbingErrorsForNetworkMap]
	@network_id INT,
	@map_value VARCHAR(63)
AS
BEGIN
	SELECT
		mm.id,
		mm.media_month,
		COUNT(a.id)
	FROM
		affidavits a			(NOLOCK)
		JOIN media_months mm	(NOLOCK) ON mm.id=a.media_month_id
	WHERE
		a.network_id=@network_id
		AND a.affidavit_net=@map_value
	GROUP BY
		mm.id,
		mm.media_month
	ORDER BY
		mm.id DESC
END
