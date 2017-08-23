
-- =============================================
-- Author:		David Sisson
-- Create date: 5/20/09
-- Description:	Finds the set of actual ratings data, universe, TV usage and 
--				viewership, by rating category, component audience, and feed 
--				type for a network, on a date and at a time.
-- =============================================

CREATE PROCEDURE [dbo].[usp_ARS_GetComponentAudienceRatingsByNetworkDateAndTime] 
	@idNetwork int,
	@date as datetime,
	@time as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare
		@idNielsenNetwork as int,
		@codeNielsenID as int,
		@stringDate as varchar(63),
		@stringMapSet as varchar(15);

	BEGIN TRY
		-- Validate input parameters
		IF NOT EXISTS (SELECT * FROM networks WHERE id = @idNetwork)
			BEGIN
			RAISERROR(
				'Requested network, ID #%d, is not in the networks table.', 
				15, 
				1, 
				@idNetwork);
			END;

		IF @time not between 0 and 86399 
			BEGIN
			RAISERROR(
				'Requested time, %d, is not in the range of valid times, 0 - 86399.', 
				15, 
				1, 
				@time);
			END;

		-- Set and validate local variables
		set @stringDate = convert(varchar,@date,111);
		set @stringMapSet = 'Nielsen';
	
		set @idNetwork = isnull(dbo.GetNationalPackageNetwork(@idNetwork, @date), @idNetwork);

		with
		nielsen_network_map(
			nielsen_id
		) as (
			select
				cast(map_value as int) nielsen_id
			from
				network_maps
			where
				1 = active
				and
				@stringMapSet = map_set
				and
				@idNetwork = network_id
				and
				@date >= effective_date
			union
			select
				cast(map_value as int) nielsen_id
			from
				network_map_histories
			where
				1 = active
				and
				@stringMapSet = map_set
				and
				@idNetwork = network_id
				and
				@date between start_date and end_date
		)
		select
			@codeNielsenID = nielsen_id
		from
			nielsen_network_map;
		IF @codeNielsenID IS NULL
			BEGIN
			RAISERROR(
				'Requested network id, %d, does not map to a Nielsen ID on %s.', 
				15, 
				1, 
				@idNetwork,
				@stringDate);
			END;

		with
			nielsen_network(
				nielsen_network_id
			) as (
				select
					id 
				from
					nielsen_networks nn
				where
					@codeNielsenID = nielsen_id
					and
					@date >= effective_date
				union
				select
					nielsen_network_id 
				from
					nielsen_network_histories nn
				where
					@codeNielsenID = nielsen_id
					and
					@date between start_date and end_date
			)
		select	
			@idNielsenNetwork = nielsen_network_id
		from
			nielsen_network;
		IF @idNielsenNetwork IS NULL
			BEGIN
			RAISERROR(
				'Requested network, ID %d and Nielsen ID %d, does not have a corresponding Nielsen Network on %s.', 
				15, 
				1, 
				@idNetwork,
				@codeNielsenID,
				@stringDate);
			END;

		-- Create output dataset
		with
		nnn(
			network_id,
			nielsen_network_id
		) as (
			select
				n.id network_id,
				nn.id nielsen_network_id
			from
				networks n
				join network_maps nm on
					n.id = nm.network_id
					and
					'Nielsen' = nm.map_set
				join nielsen_networks nn on
					nn.nielsen_id = cast(nm.map_value as int)
		),
		nsa(
			network_id,
			substitution_category_id,
			substitute_network_id,
			weight,
			start_date,
			end_date
		) as (
			select
				network_id,
				substitution_category_id,
				substitute_network_id,
				weight,
				effective_date start_date,
				'12/31/2032' end_date
			from
				network_substitutions ns

			union

			select
				network_id,
				substitution_category_id,
				substitute_network_id,
				weight,
				start_date,
				end_date
			from
				network_substitution_histories ns
		),
		ns(
				nielsen_network_id,
				substitution_category,
				substitute_nielsen_network_id,
				weight,
				start_date,
				end_date
		) as (
			select
				net_nn.nielsen_network_id nielsen_network_id,
				sc.name substitution_category,
				sub_nn.nielsen_network_id substitute_nielsen_network_id,
				ns.weight,
				ns.start_date,
				ns.end_date
			from
				nsa ns
				join dbo.substitution_categories sc on
					sc.id = ns.substitution_category_id
				join nnn net_nn on
					ns.network_id = net_nn.network_id
				join nnn sub_nn on
					ns.substitute_network_id = sub_nn.network_id
		)
		select
			mr.rating_category_id,
			mpa.audience_id,
			mr.feed_type,
			mua.universe universe,
			mta.usage tv_usage,
			mpa.usage viewership
		from
			mit_ratings mr
			join mit_person_audiences mpa on
				mr.id = mpa.mit_rating_id
			join mit_tv_audiences mta on
				mr.id = mta.mit_rating_id
				and
				mpa.audience_id = mta.audience_id
			join mit_universes mu on
				mr.nielsen_network_id = mu.nielsen_network_id
				and
				mr.rating_date between mu.start_date and mu.end_date
				and
				mr.rating_category_id = mu.rating_category_id
			join mit_universe_audiences mua on
				mu.id = mua.mit_universe_id
				and
				mua.audience_id = mpa.audience_id
			left join ns on
				ns.nielsen_network_id = mr.nielsen_network_id
				and
				'Delivery' = ns.substitution_category
				and
				@date between ns.start_date and ns.end_date
		where
			ns.nielsen_network_id is null
			and
			@idNielsenNetwork = mr.nielsen_network_id
			and
			@date = mr.rating_date
			and
			@time between mr.start_time and mr.end_time
		union
		select
			mr.rating_category_id,
			mpa.audience_id,
			mr.feed_type,
			mua.universe universe,
			mta.usage tv_usage,
			mpa.usage viewership
		from
			mit_ratings mr
			join ns nsr on
				@idNielsenNetwork = nsr.nielsen_network_id
				and
				'Delivery' = nsr.substitution_category
				and
				@date between nsr.start_date and nsr.end_date
				and
				nsr.substitute_nielsen_network_id = mr.nielsen_network_id
			join mit_person_audiences mpa on
				mr.id = mpa.mit_rating_id
			join mit_tv_audiences mta on
				mr.id = mta.mit_rating_id
				and
				mpa.audience_id = mta.audience_id
			join ns nsu on
				@idNielsenNetwork = nsu.nielsen_network_id
				and
				'Universe' = nsu.substitution_category
				and
				@date between nsu.start_date and nsu.end_date
			join mit_universes mu on
				nsu.substitute_nielsen_network_id = mu.nielsen_network_id
				and
				mr.rating_date between mu.start_date and mu.end_date
				and
				mr.rating_category_id = mu.rating_category_id
			join mit_universe_audiences mua on
				mu.id = mua.mit_universe_id
				and
				mua.audience_id = mpa.audience_id
		where
			@date = mr.rating_date
			and
			@time between mr.start_time and mr.end_time
		order by
			mpa.audience_id,
			mr.rating_category_id,
			mr.feed_type;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		-- Use RAISERROR inside the CATCH block to return error
		-- information about the original error that caused
		-- execution to jump to the CATCH block.
		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH;
END
