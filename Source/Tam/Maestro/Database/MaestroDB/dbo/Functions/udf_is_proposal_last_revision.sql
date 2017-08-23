CREATE FUNCTION [dbo].[udf_is_proposal_last_revision](
	@proposal_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT
	
	SET @return = (
			-- check if this is an original proposal with revisions
			SELECT top 1 CAST(0 AS BIT) AS has_revisions
			FROM proposals
			WHERE original_proposal_id = @proposal_id
			AND proposal_status_id IN (1,3,4,5,8)) --working, sent to client, ordered, previously ordered, internal
	
	
	IF (@return IS NULL)
	BEGIN
		-- it passed the first check
		SET @return = (
				-- check if this proposal is a revision that has revisions after it
				SELECT top 1 CAST(0 AS BIT) AS has_revisions
				FROM proposals p
				INNER JOIN proposals p2 ON p.original_proposal_id = p2.original_proposal_id
				WHERE p.id = @proposal_id
				AND p.version_number < p2.version_number)
				
		IF (@return IS NULL)
		BEGIN
			-- it passed the second check
			SET @return = 1
		END
	END
	
	RETURN @return
END

