CREATE TABLE [dbo].[traffic_details_proposal_details_map] (
    [id]                 INT        IDENTITY (1, 1) NOT NULL,
    [traffic_detail_id]  INT        NOT NULL,
    [proposal_detail_id] INT        NOT NULL,
    [proposal_rate]      MONEY      NOT NULL,
    [proposal_spots]     FLOAT (53) CONSTRAINT [DF_traffic_details_proposal_details_map_proposal_spots] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_traffic_details_proposal_details_map_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_2] FOREIGN KEY ([traffic_detail_id]) REFERENCES [dbo].[traffic_details] ([id]),
    CONSTRAINT [FK_3] FOREIGN KEY ([proposal_detail_id]) REFERENCES [dbo].[proposal_details] ([id]),
    CONSTRAINT [IX_traffic_details_proposal_details_map] UNIQUE NONCLUSTERED ([proposal_detail_id] ASC, [traffic_detail_id] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_details_proposal_details_map_traffic_detail_id]
    ON [dbo].[traffic_details_proposal_details_map]([traffic_detail_id] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_details_proposal_details_map_proposal_detail_id]
    ON [dbo].[traffic_details_proposal_details_map]([proposal_detail_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_details_proposal_details_map', @level2type = N'COLUMN', @level2name = N'proposal_spots';

