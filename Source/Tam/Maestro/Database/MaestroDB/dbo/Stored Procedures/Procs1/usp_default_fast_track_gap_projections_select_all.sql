
CREATE PROCEDURE [dbo].[usp_default_fast_track_gap_projections_select_all]
AS
SELECT
	*
FROM
	default_fast_track_gap_projections WITH(NOLOCK)

