------------------------------------------------------------------------------------------------------------------------
-- Get Together Sample Data
-- Populates the [Meetings] schema with a small, realistic sample dataset: Users, Circles,
-- CircleMemberships, InvitationCodes, Events, and RSVPs. 
------------------------------------------------------------------------------------------------------------------------
DECLARE @RemovePreviousData varchar(1) = 'Y'

IF @RemovePreviousData = 'Y'
BEGIN
  PRINT ''
  PRINT 'Removing previous sample data...'
  DELETE FROM [Meetings].[ReminderLog]
  DELETE FROM [Meetings].[RSVP]
  DELETE FROM [Meetings].[InvitationCode]
  DELETE FROM [Meetings].[CircleMembership]
  DELETE FROM [Meetings].[Event]
  DELETE FROM [Meetings].[Circle]
  DELETE FROM [Meetings].[User]
  BEGIN TRY
    PRINT 'Reseeding tables...'
    DBCC CHECKIDENT('[Meetings].[ReminderLog]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[RSVP]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[InvitationCode]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[CircleMembership]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[Event]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[Circle]', RESEED, 0)
    DBCC CHECKIDENT('[Meetings].[User]', RESEED, 0)
  END TRY
  BEGIN CATCH
    -- Import should continue even when reseed cannot be performed.
    PRINT 'Warning: identity reseed skipped: ' + ERROR_MESSAGE();
  END CATCH
END

------------------------------------------------------------------------------------------------------------------------
-- Users
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample users...'
INSERT INTO [Meetings].[User] (ExternalId, DisplayName, EmailAddress, IsActive, CreatedUtc)
SELECT v.ExternalId, v.DisplayName, v.EmailAddress, 1, getutcdate()
FROM (VALUES
  ('ext-user-001', 'Ava Chen', 'ava.chen@example.com'),
  ('ext-user-002', 'Marcus Lee', 'marcus.lee@example.com'),
  ('ext-user-003', 'Priya Patel', 'priya.patel@example.com'),
  ('ext-user-004', 'Jordan Smith', 'jordan.smith@example.com'),
  ('ext-user-005', 'Sofia Rodriguez', 'sofia.rodriguez@example.com'),
  ('ext-user-006', 'Ben Okafor', 'ben.okafor@example.com'),
  ('ext-user-007', 'Grace Kim', 'grace.kim@example.com')
) AS v(ExternalId, DisplayName, EmailAddress)
WHERE v.ExternalId NOT IN (SELECT ExternalId FROM [Meetings].[User])

------------------------------------------------------------------------------------------------------------------------
-- Circles
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample circles...'
INSERT INTO [Meetings].[Circle] (Name, Description, CreatedByUserId, CreatedUtc, IsArchived)
SELECT v.Name, v.Description, u.UserId, getutcdate(), 0
FROM (VALUES
  ('Neighborhood Book Club', 'Monthly book discussion for neighbors on Elm Street.', 'ext-user-001'),
  ('Weekend Hikers', 'Casual weekend hiking group for local trails.', 'ext-user-002'),
  ('Family Game Night', 'Recurring family-friendly board and card game gathering.', 'ext-user-003')
) AS v(Name, Description, CreatedByExternalId)
INNER JOIN [Meetings].[User] u ON u.ExternalId = v.CreatedByExternalId
WHERE v.Name NOT IN (SELECT Name FROM [Meetings].[Circle])

------------------------------------------------------------------------------------------------------------------------
-- Circle Memberships
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample circle memberships...'
INSERT INTO [Meetings].[CircleMembership] (CircleId, UserId, Role, JoinedUtc, LeftUtc)
SELECT c.CircleId, u.UserId, v.Role, getutcdate(), NULL
FROM (VALUES
  ('Neighborhood Book Club', 'ext-user-001', 'Owner'),
  ('Neighborhood Book Club', 'ext-user-002', 'Member'),
  ('Neighborhood Book Club', 'ext-user-003', 'Member'),
  ('Neighborhood Book Club', 'ext-user-005', 'Member'),
  ('Weekend Hikers', 'ext-user-002', 'Owner'),
  ('Weekend Hikers', 'ext-user-004', 'Member'),
  ('Weekend Hikers', 'ext-user-006', 'Member'),
  ('Weekend Hikers', 'ext-user-007', 'Member'),
  ('Family Game Night', 'ext-user-003', 'Owner'),
  ('Family Game Night', 'ext-user-001', 'Member'),
  ('Family Game Night', 'ext-user-004', 'Member'),
  ('Family Game Night', 'ext-user-006', 'Member')
) AS v(CircleName, UserExternalId, Role)
INNER JOIN [Meetings].[Circle] c ON c.Name = v.CircleName
INNER JOIN [Meetings].[User] u ON u.ExternalId = v.UserExternalId
WHERE NOT EXISTS (
  SELECT 1 FROM [Meetings].[CircleMembership] cm WHERE cm.CircleId = c.CircleId AND cm.UserId = u.UserId
)

------------------------------------------------------------------------------------------------------------------------
-- Invitation Codes
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample invitation codes...'
INSERT INTO [Meetings].[InvitationCode] (CircleId, Code, CreatedByUserId, CreatedUtc, ExpiresUtc)
SELECT c.CircleId, v.Code, u.UserId, getutcdate(), DATEADD(day, 30, getutcdate())
FROM (VALUES
  ('Neighborhood Book Club', 'BOOK-CLUB-24X7', 'ext-user-001'),
  ('Weekend Hikers', 'HIKE-CREW-88ZQ', 'ext-user-002'),
  ('Family Game Night', 'GAME-NIGHT-5F2K', 'ext-user-003')
) AS v(CircleName, Code, CreatedByExternalId)
INNER JOIN [Meetings].[Circle] c ON c.Name = v.CircleName
INNER JOIN [Meetings].[User] u ON u.ExternalId = v.CreatedByExternalId
WHERE v.Code NOT IN (SELECT Code FROM [Meetings].[InvitationCode])

------------------------------------------------------------------------------------------------------------------------
-- Events
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample events...'
INSERT INTO [Meetings].[Event] (CircleId, Title, Details, StartsUtc, EndsUtc, IsRecurring, RsvpMode, RecurrenceRule, CreatedByUserId, CreatedUtc)
SELECT c.CircleId, v.Title, v.Details, v.StartsUtc, v.EndsUtc, v.IsRecurring, v.RsvpMode, v.RecurrenceRule, u.UserId, getutcdate()
FROM (VALUES
  ('Neighborhood Book Club', 'September Book Discussion', 'Discussing "The Midnight Library" over coffee.', DATEADD(day, 7, getutcdate()), DATEADD(minute, 90, DATEADD(day, 7, getutcdate())), 0, 0, NULL, 'ext-user-001'),
  ('Neighborhood Book Club', 'October Book Discussion', 'Book TBD - vote in the group chat.', DATEADD(day, 37, getutcdate()), DATEADD(minute, 90, DATEADD(day, 37, getutcdate())), 0, 0, NULL, 'ext-user-001'),
  ('Weekend Hikers', 'Ridge Trail Hike', 'Moderate 6-mile loop, bring water.', DATEADD(day, 3, getutcdate()), DATEADD(hour, 3, DATEADD(day, 3, getutcdate())), 0, 0, NULL, 'ext-user-002'),
  ('Weekend Hikers', 'Sunset Ridge Series', 'Weekly sunset hikes all summer.', DATEADD(day, 10, getutcdate()), DATEADD(hour, 2, DATEADD(day, 10, getutcdate())), 1, 1, 'FREQ=WEEKLY;BYDAY=SA', 'ext-user-002'),
  ('Weekend Hikers', 'Waterfall Trailhead Meetup', 'Short scenic hike with a picnic after.', DATEADD(day, 21, getutcdate()), DATEADD(hour, 4, DATEADD(day, 21, getutcdate())), 0, 0, NULL, 'ext-user-006'),
  ('Family Game Night', 'Board Game Bonanza', 'Bring your favorite board game to share.', DATEADD(day, 5, getutcdate()), DATEADD(hour, 3, DATEADD(day, 5, getutcdate())), 0, 0, NULL, 'ext-user-003'),
  ('Family Game Night', 'Monthly Card Night', 'Recurring monthly card games.', DATEADD(day, 14, getutcdate()), DATEADD(hour, 3, DATEADD(day, 14, getutcdate())), 1, 0, 'FREQ=MONTHLY;BYDAY=2FR', 'ext-user-003'),
  ('Family Game Night', 'Kids vs Parents Trivia', 'Trivia night with prizes for the kids.', DATEADD(day, 28, getutcdate()), DATEADD(hour, 2, DATEADD(day, 28, getutcdate())), 0, 0, NULL, 'ext-user-004')
) AS v(CircleName, Title, Details, StartsUtc, EndsUtc, IsRecurring, RsvpMode, RecurrenceRule, CreatedByExternalId)
INNER JOIN [Meetings].[Circle] c ON c.Name = v.CircleName
INNER JOIN [Meetings].[User] u ON u.ExternalId = v.CreatedByExternalId
WHERE v.Title NOT IN (SELECT Title FROM [Meetings].[Event])

------------------------------------------------------------------------------------------------------------------------
-- RSVPs
------------------------------------------------------------------------------------------------------------------------
PRINT ''
PRINT 'Inserting sample RSVPs...'
INSERT INTO [Meetings].[RSVP] (EventId, CircleId, UserId, Status, Notes, OccurrenceDate, RespondedUtc)
SELECT e.EventId, e.CircleId, u.UserId, v.Status, v.Notes, NULL, getutcdate()
FROM (VALUES
  ('September Book Discussion', 'ext-user-001', 'Accepted', 'Bringing snacks.'),
  ('September Book Discussion', 'ext-user-002', 'Accepted', NULL),
  ('September Book Discussion', 'ext-user-003', 'Declined', 'Out of town that week.'),
  ('September Book Discussion', 'ext-user-005', 'Pending', NULL),
  ('Ridge Trail Hike', 'ext-user-002', 'Accepted', NULL),
  ('Ridge Trail Hike', 'ext-user-004', 'Accepted', 'Bringing trekking poles.'),
  ('Ridge Trail Hike', 'ext-user-006', 'Declined', 'Recovering from a cold.'),
  ('Board Game Bonanza', 'ext-user-003', 'Accepted', NULL),
  ('Board Game Bonanza', 'ext-user-001', 'Accepted', 'Bringing Catan.'),
  ('Board Game Bonanza', 'ext-user-004', 'Pending', NULL)
) AS v(EventTitle, UserExternalId, Status, Notes)
INNER JOIN [Meetings].[Event] e ON e.Title = v.EventTitle
INNER JOIN [Meetings].[User] u ON u.ExternalId = v.UserExternalId
WHERE NOT EXISTS (
  SELECT 1 FROM [Meetings].[RSVP] r WHERE r.EventId = e.EventId AND r.UserId = u.UserId
)

PRINT ''
PRINT 'Sample data load complete.'
