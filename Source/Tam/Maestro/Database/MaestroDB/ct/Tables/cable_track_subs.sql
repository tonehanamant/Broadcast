CREATE TABLE [ct].[cable_track_subs] (
    [tam_media_month]   VARCHAR (4)    NULL,
    [syscode]           NVARCHAR (255) NULL,
    [sys_aiue]          FLOAT (53)     NULL,
    [network_code]      NVARCHAR (255) NULL,
    [strata_network_id] INT            NULL,
    [tam_net_code]      VARCHAR (15)   NOT NULL,
    [net_ue]            FLOAT (53)     NULL,
    [tam_subs]          FLOAT (53)     NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Backfill]
    ON [ct].[cable_track_subs]([syscode] ASC, [network_code] ASC)
    INCLUDE([tam_media_month], [tam_subs]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_network_code]
    ON [ct].[cable_track_subs]([network_code] ASC)
    INCLUDE([tam_media_month], [syscode], [tam_subs]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'sys_aiue';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'sys_aiue';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'network_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'network_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'strata_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'strata_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_net_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_net_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'net_ue';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'net_ue';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_subs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'ct', @level1type = N'TABLE', @level1name = N'cable_track_subs', @level2type = N'COLUMN', @level2name = N'tam_subs';

