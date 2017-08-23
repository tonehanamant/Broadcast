
CREATE FUNCTION [dbo].[GetProposalGeneratedName]
(
	-- Add the parameters for the function here
	@proposal_id int,
	@audience_id int = null,
	@show_category bit = 0
)
RETURNS @proposalname TABLE
(
	product_id INT,
	partial_name varchar(MAX)
)
AS
BEGIN
	DECLARE @result varchar(MAX);
	DECLARE @daypart_text VARCHAR(MAX);
	DECLARE @spot_length_text VARCHAR(MAX);

	DECLARE @table TABLE (
		product_id int,
		dayparts_name varchar(max),
		spot_lengths varchar(max),
		category varchar(max),
		audiences_text varchar(max),
		adu varchar(max),
		msa varchar(max),
		overnight varchar(max)
	);
	
	IF dbo.IsMultiDaypartPlan(@proposal_id) = 1
		SET @daypart_text = 'MD';
	ELSE
		SELECT 
			@daypart_text = d.daypart_text 
		FROM
			proposals p (NOLOCK) 
			JOIN dayparts d (NOLOCK) ON d.id=p.primary_daypart_id
		WHERE 
			p.id=@proposal_id;
	
	IF @daypart_text IS NOT NULL AND LEN(@daypart_text)>0
		SET @daypart_text = @daypart_text + ' '

	-- get spot length text (supports multiple spot lengths)
	SELECT
		@spot_length_text = COALESCE(@spot_length_text + ' ', '') + ':' + CAST(t.length AS VARCHAR)
	FROM (
		SELECT DISTINCT
			sl.length
		FROM
			proposal_details pd (NOLOCK)
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pd.proposal_id=@proposal_id
	) t
	ORDER BY
		t.length
	IF @spot_length_text IS NOT NULL AND LEN(@spot_length_text)>0
		SET @spot_length_text = @spot_length_text + ' '	
	
	-- insert the values exactly as needed with spaces at the end since no concat() func available
	insert into @table
		select
			proposals.product_id,
			@daypart_text,
			@spot_length_text,
			case when c.name is not null then
				c.name  + ' '
			else 
				''
			end,
			case when a.code is not null then
				'(Alt Demo ' + a.code  + ') '
			else
				''
			end,
			case when (proposals.is_audience_deficiency_unit_schedule = 1) then
				'(ADU' + (case when proposals.audience_deficiency_unit_for is not null then ' for ' + proposals.audience_deficiency_unit_for else '' end) + ') '
			else
				''
			end,
			case when(proposals.is_msa = 1) then
				'(MSA) '
			else
				''
			end,
			case when proposals.is_overnight=1 then
				'(ON) '
			else
				''
			end
		from 
			proposals with (nolock)
			join spot_lengths with (nolock) on spot_lengths.id = proposals.default_spot_length_id
			left join proposal_audiences pa with (nolock) on pa.proposal_id=proposals.id
				and pa.ordinal>1
				and pa.audience_id= @audience_id
			left join audiences a (nolock) on a.id=pa.audience_id
			left join categories c with (nolock) on c.id=proposals.category_id
		where 
			proposals.id = @proposal_id

		insert into @proposalname select 
			T.product_id, 
			ISNULL(T.dayparts_name, '') + 
			ISNULL(T.spot_lengths, '') + 
			CASE @show_category WHEN 1 THEN ISNULL(T.category, '') ELSE '' END + 
			ISNULL(T.audiences_text, '') + 
			ISNULL(T.adu, '') + 
			ISNULL(T.msa, '') + 
			ISNULL(T.overnight, '') 
		from 
			@table T

		return;
END