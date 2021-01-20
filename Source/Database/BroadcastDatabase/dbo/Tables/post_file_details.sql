CREATE TABLE [dbo].[post_file_details] (
    [id]                            INT           IDENTITY (1, 1) NOT NULL,
    [post_file_id]                  INT           NOT NULL,
    [rank]                          INT           NULL,
    [market]                        VARCHAR (63)  NULL,
    [station]                       VARCHAR (15)  NOT NULL,
    [affiliate]                     VARCHAR (15)  NOT NULL,
    [weekstart]                     DATETIME      NOT NULL,
    [day]                           VARCHAR (3)   NOT NULL,
    [date]                          DATETIME      NOT NULL,
    [time_aired]                    INT           NOT NULL,
    [program_name]                  VARCHAR (255) NOT NULL,
    [spot_length]                   INT           NOT NULL,
    [spot_length_id]                INT           NOT NULL,
    [house_isci]                    VARCHAR (63)  NULL,
    [client_isci]                   VARCHAR (63)  NOT NULL,
    [advertiser]                    VARCHAR (63)  NOT NULL,
    [inventory_source]              VARCHAR (255) NOT NULL,
    [inventory_source_daypart]      VARCHAR (255) NOT NULL,
    [inventory_out_of_spec_reason]  VARCHAR (255) NOT NULL,
    [estimate_id]                   INT           NOT NULL,
    [detected_via]                  VARCHAR (255) NULL,
    [spot]                          INT           NOT NULL,
    [advertiser_out_of_spec_reason] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_post_file_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_post_file_details_post_files] FOREIGN KEY ([post_file_id]) REFERENCES [dbo].[post_files] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_post_file_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_post_file_details_spot_lengths]
    ON [dbo].[post_file_details]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_post_file_details_post_files]
    ON [dbo].[post_file_details]([post_file_id] ASC);

