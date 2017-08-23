CREATE TABLE [dbo].[traffic_details] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [traffic_id]         INT           NOT NULL,
    [network_id]         INT           NOT NULL,
    [daypart_id]         INT           NOT NULL,
    [spot_length_id]     INT           NOT NULL,
    [comment]            VARCHAR (127) NOT NULL,
    [internal_note_id]   INT           NULL,
    [external_note_id]   INT           NULL,
    [traffic_amount]     MONEY         CONSTRAINT [DF_traffic_details_traffic_amount] DEFAULT ((0)) NOT NULL,
    [release_amount]     MONEY         NULL,
    [CPM1]               MONEY         NULL,
    [CPM2]               MONEY         NULL,
    [traffic_amount1]    MONEY         NULL,
    [traffic_amount2]    MONEY         NULL,
    [release_amount1]    MONEY         NULL,
    [release_amount2]    MONEY         NULL,
    [proposal_detail_id] INT           NULL,
    [traffic_rating]     FLOAT (53)    NULL,
    CONSTRAINT [PK_traffic_details] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_details_external_note] FOREIGN KEY ([external_note_id]) REFERENCES [dbo].[notes] ([id]),
    CONSTRAINT [FK_traffic_details_internal_note] FOREIGN KEY ([internal_note_id]) REFERENCES [dbo].[notes] ([id]),
    CONSTRAINT [FK_traffic_details_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_details_proposal_details] FOREIGN KEY ([proposal_detail_id]) REFERENCES [dbo].[proposal_details] ([id]),
    CONSTRAINT [FK_traffic_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_traffic_details_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);






GO
CREATE NONCLUSTERED INDEX [IX_traffic_details_traffic_id]
    ON [dbo].[traffic_details]([traffic_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The corresponding traffic record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The nationally sold network record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The nationally sold daypart record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The spot length record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Free text comment.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'internal_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Internal note/comment for internal reporting.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'internal_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'internal_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'external_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Externally facing note/comment for inclusion on outgoing insertion orders.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'external_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details', @level2type = N'COLUMN', @level2name = N'external_note_id';

