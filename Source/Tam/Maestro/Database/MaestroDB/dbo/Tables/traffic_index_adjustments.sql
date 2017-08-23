CREATE TABLE [dbo].[traffic_index_adjustments] (
    [id]                       INT IDENTITY (1, 1) NOT NULL,
    [traffic_id]               INT NOT NULL,
    [threshold]                INT NOT NULL,
    [percentage]               INT NOT NULL,
    [is_fixed_rate_adjustment] BIT NOT NULL,
    CONSTRAINT [PK_traffic_index_adjustments] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_index_adjustments_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

