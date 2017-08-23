
-- =============================================
-- Author:		David Sisson
-- Create date: 4/26/2012
-- Description:	Returns the set of networks on any rate card for a given sales 
--				product (currently defined by sales model and rate-card type 
--				and in the year of and in the year following a given date.
-- =============================================
CREATE FUNCTION udf_GetRateCardNetworksForProductAsOf 
(	
	-- Add the parameters for the function here
	@nameSalesModel varchar(63), 
	@nameRateCardType varchar(63),
	@dateAsOf datetime
)
RETURNS TABLE 
AS
RETURN 
(
	with
	ratecard_books(
		network_rate_card_book_id,
		rate_card_book_year,
		rate_card_book_version,
		max_version
	) as (
		select
			nrb.id,
			nrb.year,
			nrb.version,
			max(nrb.version) over(partition by nrb.sales_model_id, nrb.year, nrb.quarter, nrb.media_month_id) max_version
		from
			dbo.network_rate_card_books nrb
			join dbo.sales_models sm on
				sm.id = nrb.sales_model_id
		where
			nrb.date_approved is not null
			and
			(
				sm.name = @nameSalesModel
				or
				@nameSalesModel is null
			)
	),
	latest_ratecard_books(
		network_rate_card_book_id,
		year
	) as (
		select
			rb.network_rate_card_book_id,
			rb.rate_card_book_year
		from
			ratecard_books rb
		where
			rb.rate_card_book_version = rb.max_version
	)
	select distinct
		nrd.network_id
	from
		latest_ratecard_books lrb
		join dbo.network_rate_cards nr on
			lrb.network_rate_card_book_id = nr.network_rate_card_book_id
		join rate_card_types rt on
			rt.id = nr.rate_card_type_id
		join dbo.network_rate_card_details nrd on
			nrd.network_rate_card_id = nr.id
	where
		(
			lrb.year = datepart(year, @dateAsOf)
			or
			lrb.year = datepart(year, @dateAsOf) + 1
			or
			@dateAsOf is null
		)
		and
		(
			rt.name = @nameRateCardType
			or
			@nameRateCardType is null
		)
);
