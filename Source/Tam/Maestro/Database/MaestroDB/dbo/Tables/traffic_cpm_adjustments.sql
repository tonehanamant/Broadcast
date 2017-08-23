CREATE TABLE [dbo].[traffic_cpm_adjustments] (
    [id]         INT IDENTITY (1, 1) NOT NULL,
    [traffic_id] INT NOT NULL,
    [percentage] INT NOT NULL,
    CONSTRAINT [PK_traffic_cpm_adjustments] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_cpm_adjustments_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

