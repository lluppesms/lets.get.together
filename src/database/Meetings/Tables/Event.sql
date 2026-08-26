CREATE TABLE [Meetings].[Event](
    [EventId] [int] IDENTITY(1,1) NOT NULL,
    [CircleId] [int] NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Details] [nvarchar](2000) NULL,
    [StartsUtc] [datetime2](7) NOT NULL,
    [EndsUtc] [datetime2](7) NULL,
    [IsRecurring] [bit] NOT NULL,
    [RsvpMode] [int] NOT NULL,
    [RecurrenceRule] [nvarchar](200) NULL,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    [CancelledUtc] [datetime2](7) NULL,
    CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED ([EventId] ASC),
    CONSTRAINT [AK_Event_EventId_CircleId] UNIQUE ([EventId], [CircleId]),
    CONSTRAINT [FK_Event_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Event_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId])
)
GO

ALTER TABLE [Meetings].[Event] ADD CONSTRAINT [DF_Event_IsRecurring] DEFAULT ((0)) FOR [IsRecurring]
GO
ALTER TABLE [Meetings].[Event] ADD CONSTRAINT [DF_Event_RsvpMode] DEFAULT ((0)) FOR [RsvpMode]
GO
ALTER TABLE [Meetings].[Event] ADD CONSTRAINT [DF_Event_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE INDEX [IX_Event_CircleId_StartsUtc] ON [Meetings].[Event] ([CircleId], [StartsUtc])
GO