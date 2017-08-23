-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/2/2012
-- Description:	Used to create/query the Operations Calendar.
-- =============================================
-- SELECT * FROM dbo.udf_CreateOperationsCalendar(374)
CREATE FUNCTION udf_CreateOperationsCalendar
(	
	@media_month_id INT
)
RETURNS TABLE 
AS
RETURN 
(
	WITH ordered_months (
		media_month_id, 
		end_date, 
		row
	) AS (
		SELECT
			mm.id 'media_month_id',
			mm.end_date,
			ROW_NUMBER() OVER(ORDER BY mm.start_date) 'row'
		FROM
			media_months mm (NOLOCK)
	)
	SELECT
		m.end_date 'Last Day Of Media Month',
		DATEADD(day, 1, mw_lwor.start_date) 'Last Week of Ratings',
		DATEADD(day, 4, mw_aco.start_date) 'Affidavit Cut Off',
		DATEADD(day, 3, mw_ftp.start_date) 'Fast Track Post',
		mw_p.start_date 'Post',
		CASE
			WHEN DATEPART(day, mw_msap.start_date)>=22	THEN mw_msap.start_date
			WHEN DATEPART(day, mw_msap.start_date)=18	THEN DATEADD(day, 4, mw_msap.start_date)
			WHEN DATEPART(day, mw_msap.start_date)=19	THEN DATEADD(day, 3, mw_msap.start_date)
			WHEN DATEPART(day, mw_msap.start_date)=20	THEN DATEADD(day, 2, mw_msap.start_date)
			WHEN DATEPART(day, mw_msap.start_date)=21	THEN DATEADD(day, 1, mw_msap.start_date)
			ELSE NULL
		END 'MSA Post'
	FROM
		ordered_months m (NOLOCK)
		JOIN ordered_months m_plus1 ON m_plus1.row=m.row+1
		JOIN ordered_months m_plus2 ON m_plus2.row=m.row+2
		JOIN media_weeks mw_lwor	(NOLOCK) ON mw_lwor.media_month_id=m_plus1.media_month_id	AND mw_lwor.week_number=3
		JOIN media_weeks mw_aco		(NOLOCK) ON mw_aco.media_month_id=m_plus1.media_month_id	AND mw_aco.week_number=2
		JOIN media_weeks mw_ftp		(NOLOCK) ON mw_ftp.media_month_id=m_plus1.media_month_id	AND mw_ftp.week_number=3
		JOIN media_weeks mw_p		(NOLOCK) ON mw_p.media_month_id=m_plus2.media_month_id		AND mw_p.week_number=4
		JOIN media_weeks mw_msap	(NOLOCK) ON mw_msap.media_month_id=m_plus2.media_month_id	AND mw_msap.week_number=(SELECT MAX(week_number) FROM media_weeks (NOLOCK) WHERE media_month_id=m_plus2.media_month_id)
	WHERE
		m.media_month_id=@media_month_id
)
