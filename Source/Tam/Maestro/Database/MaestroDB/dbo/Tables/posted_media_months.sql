CREATE TABLE [dbo].[posted_media_months] (
    [media_month_id] INT NOT NULL,
    [complete]       BIT NOT NULL,
    CONSTRAINT [PK_posted_media_months] PRIMARY KEY CLUSTERED ([media_month_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_posted_media_months_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);










GO


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months', @level2type = N'COLUMN', @level2name = N'complete';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'posted_media_months', @level2type = N'COLUMN', @level2name = N'complete';


GO
CREATE TRIGGER dbo.post_email_notice
	ON maestro.dbo.posted_media_months
AFTER INSERT, UPDATE
AS

DECLARE @Message varchar(500)

select @Message=cast(case mm.[month] 
			when 1 then 'January'
			when 2 then 'February'
			when 3 then 'March'
			when 4 then 'April'
			when 5 then 'May'
			when 6 then 'June'
			when 7 then 'July'
			when 8 then 'August'
			when 9 then 'September'
			when 10 then 'October'
			when 11 then 'November'
			when 12 then 'December'
			else 'error' 
	  end as varchar) + ' '
		+ cast(mm.[year] as varchar)+ N' has been marked as final.'
from inserted pmm
join maestro.dbo.media_months mm
on mm.id=pmm.media_month_id
and pmm.complete = 1

select @Message=cast(case mm.[month] 
			when 1 then 'January'
			when 2 then 'February'
			when 3 then 'March'
			when 4 then 'April'
			when 5 then 'May'
			when 6 then 'June'
			when 7 then 'July'
			when 8 then 'August'
			when 9 then 'September'
			when 10 then 'October'
			when 11 then 'November'
			when 12 then 'December'
			else 'error' 
	  end as varchar) + ' '
		+ cast(mm.[year] as varchar)+ N' has been changed to no longer be final.'
from inserted pmm
join maestro.dbo.media_months mm
on mm.id=pmm.media_month_id
and pmm.complete = 0

EXECUTE msdb.dbo.sp_notify_operator @name=N'postalerts',
	@subject=N'Post Change Notification',
	@body=@Message;