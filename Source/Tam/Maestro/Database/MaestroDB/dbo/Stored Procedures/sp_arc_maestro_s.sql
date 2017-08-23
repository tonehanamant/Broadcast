--exec [dbo].[sp_arc_maestro_s] 'posted_affidavits', 'MediaMonthIDRangePFN', '0514', '0714'

CREATE PROCEDURE [dbo].[sp_arc_maestro_s]
@tablename varchar(50), @pfunction VARCHAR(50),  @min_media_month VARCHAR(4), @max_media_month VARCHAR(4)
as

	declare @string_ varchar(2000), @pmin_id INT,  @pmax_id INT, @p_id INT,  @min_id INT, @max_id int

BEGIN TRY
--get media_month_id, and min partition, and max partion numbers
	SET  @string_='select id, $partition.[' + @pfunction + '](id) as p_id into temp_db_backup.dbo.temp_pid
			from dbo.media_months'
	EXEC(@string_)
	
	SELECT @pmin_id=MIN(p_id), @pmax_id=MAX(p_id) 
		FROM temp_db_backup.dbo.temp_pid
		WHERE id IN (SELECT id FROM dbo.media_months WHERE media_month in (@min_media_month, @max_media_month))

	--move partion data
	set @p_id=@pmin_id

	while @p_id<= @pmax_id
	BEGIN
	--switch table	
		set @string_='alter table dbo.'+@tablename + '
			SWITCH PARTITION '+cast(@p_id as varchar(3)) +
			' to dbo.' +@tablename + '_out PARTITION '+ cast(@p_id as varchar(3))
	--execute data move
		exec(@string_)
	--go for next
		set @p_id=@p_id+1
	END

	DROP table  temp_db_backup.dbo.temp_pid

END TRY
BEGIN CATCH
    DECLARE @ErrorNumber INT, @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure NVARCHAR(200), @ErrorLine INT, @ErrorMessage NVARCHAR(4000);
	SELECT 
		@ErrorNumber = ERROR_NUMBER(),
		@ErrorSeverity = ERROR_SEVERITY(),
		@ErrorState = ERROR_STATE(),
		@ErrorProcedure = isnull(ERROR_PROCEDURE(), 'N/A'),
		@ErrorLine = ERROR_LINE(),
		@ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 
            'Message: '+ ERROR_MESSAGE();
    DROP table  temp_db_backup.dbo.temp_pid
	
	RAISERROR(
		@ErrorMessage, 
		@ErrorSeverity, 
		1,               
		@ErrorNumber,    -- parameter: original error number.
		@ErrorSeverity,  -- parameter: original error severity.
		@ErrorState,     -- parameter: original error state.
		@ErrorProcedure, -- parameter: original error procedure name.
		@ErrorLine       -- parameter: original error line number.
	);
END CATCH;