/* =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>

-- mod id: 1.1
-- modification: 09/18/2012  
-- Eric Wenger
-- Description: per requirements from the business, 
	--exclude upfront plans
	--exclude Hispanic networks
	
	other changes:
	--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED in place of table hints
	--put reserved words used as column names in brackets
	--explicitly specify the DB object schema

--to execute:
--	EXEC [usp_PCS_ExportRevenueByNetworkReport] 2, 2011
-- =============================================
-- usp_PCS_ExportRevenueByNetworkReport 3, 2009  */
CREATE PROCEDURE [dbo].[usp_PCS_ExportRevenueByNetworkReport]
	@quarter INT,
	@year INT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED -- +1.1 sql2K8 may disregard table level hints

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 'Revenue by network for ' + CASE @quarter WHEN 1 THEN '1st' WHEN 2 THEN '2nd' WHEN 3 THEN '3rd' WHEN 4 THEN '4th' END + ' Quarter, ' + CAST(@year AS VARCHAR(5))

	SELECT
		n.code,
		SUM(pdw.units * pd.proposal_rate) 'gross_revenue'
	FROM
		dbo.proposal_detail_worksheets pdw -- +1.1 specify object schema
		JOIN dbo.proposal_details pd ON pd.id=pdw.proposal_detail_id  -- +1.1 specify object schema
		JOIN dbo.proposals p ON p.id=pd.proposal_id  -- +1.1 specify object schema
		JOIN dbo.networks n ON n.id=pd.network_id and n.code not like '%-H%' -- +1.1 exclude Hispanic networks; specify object schema
		JOIN dbo.media_weeks mw ON mw.id=pdw.media_week_id -- +1.1 specify object schema
		JOIN dbo.media_months mm ON mm.id=mw.media_month_id -- +1.1 specify object schema
	WHERE
		p.proposal_status_id=4
		AND CASE mm.[month] -- +1.1 bracket reserved words
				WHEN 1 THEN 1 
				WHEN 2 THEN 1
				WHEN 3 THEN 1
				WHEN 4 THEN 2
				WHEN 5 THEN 2
				WHEN 6 THEN 2
				WHEN 7 THEN 3
				WHEN 8 THEN 3
				WHEN 9 THEN 3
				WHEN 10 THEN 4
				WHEN 11 THEN 4
				WHEN 12 THEN 4
			END = @quarter
		AND mm.[year]=@year -- +1.1 bracket reserved words
		AND pdw.units>0
		AND pd.proposal_rate>0
		AND p.is_upfront =0 /* +1.1 exclude upfronts; query already returns ordered plans, which is the most reliable source for this flag value */
	GROUP BY
		n.code
	ORDER BY
		n.code
END
