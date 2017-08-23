
--IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND 
--       name = 'usp_SRS_GetProductByMediaMonthRelease')
--   DROP  Procedure  usp_SRS_GetProductByMediaMonthRelease
--GO

CREATE PROCEDURE [dbo].[usp_SRS_GetProductByMediaMonthRelease]
(
      @media_month_id varchar(4), @release_id Int
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	select DISTINCT 
		tr.product_description_id pid,
		Case When tr.display_name = ' ' Then tr.name + ', ProdId: ' + cast(tr.product_description_id as varchar) 
			 Else						tr.display_name + ', ProdId: ' + cast(tr.product_description_id as varchar) 
			 End [Product] 
	from
		releases r
		join traffic tr on	
			tr.release_id = r.id
		join traffic_details trd on 
			tr.id = trd.traffic_id
		join traffic_orders tro on
			trd.id = tro.traffic_detail_id
	where
		r.id = @release_id
		and tro.on_financial_reports = 1
		and tro.active = 1
	order by
		[Product] 

END
