-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/20/2014
-- Description:	<Description,,>
-- =============================================
-- usp_BS_GetBroadcastPostResults 212
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastPostResults] 
	@broadcast_affidavit_file_id INT
AS
BEGIN
	SET NOCOUNT ON;

    CREATE TABLE #audiences (audience_id INT, audience_code VARCHAR(31))
	INSERT INTO #audiences
		SELECT bpd.audience_id,a.code FROM broadcast_post_details bpd (NOLOCK) JOIN audiences a (NOLOCK) ON a.id=bpd.audience_id WHERE bpd.broadcast_affidavit_file_id=@broadcast_affidavit_file_id AND bpd.audience_id=31 GROUP BY bpd.audience_id,a.code;
	INSERT INTO #audiences
		SELECT bpd.audience_id,a.code FROM broadcast_post_details bpd (NOLOCK) JOIN audiences a (NOLOCK) ON a.id=bpd.audience_id WHERE bpd.broadcast_affidavit_file_id=@broadcast_affidavit_file_id AND bpd.audience_id<>31 GROUP BY bpd.audience_id,a.code;
	
	DECLARE @cols NVARCHAR(MAX)
	SELECT  @cols = STUFF
	(
		(
			SELECT DISTINCT TOP 100 PERCENT 
				'],[' + t2.audience_code
			FROM
				#audiences AS t2
			ORDER BY 
				'],[' + t2.audience_code
			FOR XML PATH('')
		), 1, 2, ''
	) + ']'
	SET @cols = REPLACE(@cols,'&amp;','&')

	DECLARE @query NVARCHAR(MAX)
	SET @query = 
	'SELECT market [Market],station [Station],network_affilates [Network Affilate],air_date [Date],dbo.GetExportableTime(air_time) [Time],program [Program],length [Length],isci [ISCI],advertiser [Advertiser],product [Product],phone_number [Phone #],campaign [Campaign],invoice_number [Invoice #],' + @cols + ' FROM 
	(
		SELECT 
			market,station,network_affilates,air_date,air_time,program,length,isci,advertiser,product,phone_number,campaign,invoice_number,a.audience_code,SUM(bpd.delivery) [viewers] 
		FROM
			broadcast_post_details bpd (NOLOCK)
			JOIN broadcast_affidavits ba (NOLOCK) ON ba.media_month_id=bpd.media_month_id 
				AND ba.id=bpd.broadcast_affidavit_id
				AND ba.broadcast_affidavit_file_id=' + CAST(@broadcast_affidavit_file_id AS VARCHAR) + '
			LEFT JOIN spot_lengths sl (NOLOCK) ON sl.id=ba.spot_length_id
			JOIN #audiences a ON a.audience_id=bpd.audience_id
		GROUP BY
			ba.market,
			ba.station,
			ba.air_date,
			ba.air_time,
			ba.program,
			sl.length,
			ba.isci,
			ba.advertiser,
			ba.invoice_number,
			ba.phone_number,
			ba.product,
			ba.network_affilates,
			ba.campaign,
			a.audience_code
	) data
	PIVOT
	(
		SUM(data.viewers)
		FOR audience_code IN (' + @cols + ')
	) pvt ORDER BY market,station,air_date,air_time,program,length,isci,advertiser'
	
	EXECUTE(@query)
	
	DROP TABLE #audiences;
END
