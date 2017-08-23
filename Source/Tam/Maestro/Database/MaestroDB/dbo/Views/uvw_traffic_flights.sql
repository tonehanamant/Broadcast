
CREATE VIEW [dbo].[uvw_traffic_flights]
AS
SELECT        TOP (100) PERCENT tf.traffic_id, tf.start_date, tf.end_date, mw.id
FROM            dbo.traffic_flights AS tf WITH (NOLOCK) INNER JOIN
                         dbo.media_weeks AS mw WITH (NOLOCK) ON tf.start_date >= mw.start_date AND tf.end_date <= mw.end_date
ORDER BY tf.traffic_id