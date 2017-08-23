-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostsByAdvertiser
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostsByAdvertiser]
	@advertiser_company_id INT
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.advertiser_company_id = @advertiser_company_id
END
