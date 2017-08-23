
-- =============================================
-- Author:		Mike Deaven
-- Create date: 8/30/2012
-- Description:	Inserts the delivered ratings
-- =============================================
CREATE PROCEDURE usp_ARSLoader_InsertDeliveredRatings
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @cutoff AS datetime;

	SELECT
		@cutoff = MAX(mm.end_date)
	FROM
		delivered_ratings dr
		JOIN media_months mm
			ON mm.id = dr.media_month_id;
	
	print CONVERT(varchar, @cutoff, 111);
	
	with
	ndr(
		media_month_id,
		network_id,
		rating
	) 
	AS (
		SELECT
			mm.id [media_month_id],
			n.id [network_id],
			dr.delivered_rating [rating]
		FROM
			temp_db_backup.dbo.delivered_ratings_temp dr
			JOIN networks n
				ON n.code = dr.abbr
			JOIN media_months mm
				ON dr.period = mm.media_month
		WHERE
			mm.start_date > @cutoff
			AND dr.daypart = 1
	)
	INSERT INTO
		delivered_ratings(
			[media_month_id],
			[network_id],
			[rating]
		)
		SELECT
			ndr.media_month_id,
			ndr.network_id,
			ndr.rating
		FROM
			ndr
			JOIN networks n 
				ON n.id = ndr.network_id
			JOIN media_months mm
				ON mm.id = ndr.media_month_id
		WHERE
			rating <> 0
		ORDER BY
			ndr.media_month_id,
			ndr.network_id;
END
