-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/15/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostDayparts]
	@tam_post_id INT
AS
BEGIN
	SELECT
		tpd.*
	FROM
		tam_post_dayparts tpd (NOLOCK)
	WHERE
		tpd.tam_post_id=@tam_post_id
END
