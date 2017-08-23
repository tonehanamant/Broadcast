CREATE procedure [dbo].[usp_ARSLoader_GetMitUniverseAudiencesForMitUniverseId]
	@MitUniverseId int
AS
BEGIN
	SELECT  media_month_id ,
	        mit_universe_id ,
	        audience_id ,
	        universe ,
	        effective_date
	FROM dbo.mit_universe_audiences mua (NOLOCK)
	WHERE mua.mit_universe_id = @MitUniverseId
END

