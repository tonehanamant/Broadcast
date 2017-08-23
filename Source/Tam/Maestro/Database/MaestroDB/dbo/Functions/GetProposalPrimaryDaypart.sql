-- SELECT dbo.GetProposalPrimaryDaypart(22979)
CREATE FUNCTION [dbo].[GetProposalPrimaryDaypart]
(
	@proposal_id INT
)
RETURNS VARCHAR(63)
AS
BEGIN
	DECLARE @return VARCHAR(63);

	WITH tmp (daypart_text,total) AS
	(
		SELECT TOP 1
			dayparts.daypart_text,
			COUNT(*) 'total'
		FROM
			proposal_details (NOLOCK)
			JOIN dayparts (NOLOCK) ON dayparts.id=proposal_details.daypart_id
		WHERE
			proposal_details.proposal_id=@proposal_id
		GROUP BY
			dayparts.daypart_text
		ORDER BY
			COUNT(*) DESC
	)
	SELECT TOP 1 @return = daypart_text FROM tmp

	RETURN @return
END
