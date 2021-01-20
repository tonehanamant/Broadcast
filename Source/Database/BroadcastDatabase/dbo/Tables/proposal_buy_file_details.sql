CREATE TABLE [dbo].[proposal_buy_file_details] (
    [id]                   INT             IDENTITY (1, 1) NOT NULL,
    [proposal_buy_file_id] INT             NOT NULL,
    [total_spots]          INT             NOT NULL,
    [spot_cost]            DECIMAL (10, 2) NOT NULL,
    [total_cost]           DECIMAL (10, 2) NOT NULL,
    [spot_length_id]       INT             NOT NULL,
    [daypart_id]           INT             NOT NULL,
    [station_id]           INT             NOT NULL,
    CONSTRAINT [PK_proposal_buy_file_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_buy_file_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_proposal_buy_file_details_proposal_buy_files] FOREIGN KEY ([proposal_buy_file_id]) REFERENCES [dbo].[proposal_buy_files] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_buy_file_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_proposal_buy_file_details_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_details_stations]
    ON [dbo].[proposal_buy_file_details]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_details_spot_lengths]
    ON [dbo].[proposal_buy_file_details]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_details_proposal_buy_files]
    ON [dbo].[proposal_buy_file_details]([proposal_buy_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_details_dayparts]
    ON [dbo].[proposal_buy_file_details]([daypart_id] ASC);

