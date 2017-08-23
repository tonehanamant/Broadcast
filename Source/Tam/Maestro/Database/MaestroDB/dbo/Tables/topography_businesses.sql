CREATE TABLE [dbo].[topography_businesses] (
    [topography_id]  INT      NOT NULL,
    [business_id]    INT      NOT NULL,
    [include]        BIT      NOT NULL,
    [effective_date] DATETIME NOT NULL,
    CONSTRAINT [PK_topography_businesses] PRIMARY KEY CLUSTERED ([topography_id] ASC, [business_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_topography_businesses_businesses] FOREIGN KEY ([business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_topography_businesses_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
CREATE TRIGGER tr_topography_businesses 
	ON dbo.topography_businesses 
	AFTER INSERT 
AS 

SET NOCOUNT ON 

--insert trigger event 
INSERT INTO trigger_status (id, table_name, trigger_name, trigger_start_time, trigger_flag) 
VALUES(12, 'topography_businesses', 'tr_topography_businesses', GETDATE(), 1)  --es

GO
DISABLE TRIGGER [dbo].[tr_topography_businesses]
    ON [dbo].[topography_businesses];


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_businesses', @level2type = N'COLUMN', @level2name = N'effective_date';

