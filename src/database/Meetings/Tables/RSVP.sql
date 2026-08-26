CREATE TABLE [Meetings].[RSVP](
    [RsvpId] [int] IDENTITY(1,1) NOT NULL,
    [EventId] [int] NOT NULL,
    [CircleId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [Status] [nvarchar](32) NOT NULL,
    [Notes] [nvarchar](1000) NULL,
    [OccurrenceDate] [datetime2](7) NULL,
    [RespondedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_RSVP] PRIMARY KEY CLUSTERED ([RsvpId] ASC),
    CONSTRAINT [FK_RSVP_Event] FOREIGN KEY ([EventId], [CircleId]) REFERENCES [Meetings].[Event] ([EventId], [CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RSVP_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]),
    CONSTRAINT [FK_RSVP_CircleMembership] FOREIGN KEY ([CircleId], [UserId]) REFERENCES [Meetings].[CircleMembership] ([CircleId], [UserId])
)
GO

ALTER TABLE [Meetings].[RSVP] ADD CONSTRAINT [DF_RSVP_Status] DEFAULT (N'Pending') FOR [Status]
GO
ALTER TABLE [Meetings].[RSVP] ADD CONSTRAINT [DF_RSVP_RespondedUtc] DEFAULT (getutcdate()) FOR [RespondedUtc]
GO

CREATE UNIQUE INDEX [IX_RSVP_EventId_UserId] ON [Meetings].[RSVP] ([EventId], [UserId])
GO