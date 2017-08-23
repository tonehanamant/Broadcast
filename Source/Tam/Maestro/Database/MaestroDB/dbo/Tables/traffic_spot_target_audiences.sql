CREATE TABLE [dbo].[traffic_spot_target_audiences] (
    [traffic_spot_target_id] INT        NOT NULL,
    [audience_id]            INT        NOT NULL,
    [impressions_per_spot]   FLOAT (53) NOT NULL,
    [proposal_rating]        FLOAT (53) NOT NULL,
    [traffic_rating]         FLOAT (53) NOT NULL,
    [scaling_factor]         FLOAT (53) NOT NULL,
    [coverage_universe]      FLOAT (53) NOT NULL,
    CONSTRAINT [PK_traffic_spot_target_audiencess] PRIMARY KEY CLUSTERED ([traffic_spot_target_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_spot_target_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_traffic_spot_target_audiences_traffic_spot_targets] FOREIGN KEY ([traffic_spot_target_id]) REFERENCES [dbo].[traffic_spot_targets] ([id])
);

