CREATE TABLE [Meetings].[Circle](
    [CircleId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [Description] [nvarchar](1000) NULL,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    [IsArchived] [bit] NOT NULL,
    CONSTRAINT [PK_Circle] PRIMARY KEY CLUSTERED ([CircleId] ASC),
    CONSTRAINT [FK_Circle_User_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId])
)
GO

ALTER TABLE [Meetings].[Circle] ADD CONSTRAINT [DF_Circle_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO
ALTER TABLE [Meetings].[Circle] ADD CONSTRAINT [DF_Circle_IsArchived] DEFAULT ((0)) FOR [IsArchived]
GO