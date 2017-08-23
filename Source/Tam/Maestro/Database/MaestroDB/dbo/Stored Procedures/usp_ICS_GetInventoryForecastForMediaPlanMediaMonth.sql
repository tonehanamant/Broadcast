CREATE PROCEDURE [dbo].[usp_ICS_GetInventoryForecastForMediaPlanMediaMonth]
	@inventory_details InventoryRequestTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	DECLARE @start_date DATETIME;
	SELECT @start_date = MIN(mm.start_date) FROM @inventory_details pd JOIN media_months mm ON mm.id=pd.media_month_id;
	
	DECLARE @latest_base_media_month_id SMALLINT= dbo.udf_GetLatestPossibleInventoryForecastBaseMediaMonthId(@start_date);
	SELECT mm.* FROM media_months mm WHERE mm.id=@latest_base_media_month_id;
END