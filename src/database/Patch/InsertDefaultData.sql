------------------------------------------------------------------------------------------------------------------------
-- Get Together Sample Data
-- Populates the [Meetings] schema with a small, realistic clean-slate dataset.
------------------------------------------------------------------------------------------------------------------------
DECLARE @RemovePreviousData varchar(1) = 'Y';

IF @RemovePreviousData = 'Y'
BEGIN
    PRINT '';
    PRINT 'Removing previous sample data...';
    DELETE FROM [Meetings].[EmailVerificationToken];
    DELETE FROM [Meetings].[ReminderLog];
    DELETE FROM [Meetings].[RSVP];
    DELETE FROM [Meetings].[InvitationCode];
    DELETE FROM [Meetings].[CircleMembership];
    DELETE FROM [Meetings].[Event];
    DELETE FROM [Meetings].[Circle];
    DELETE FROM [Meetings].[UserIdentity];
    DELETE FROM [Meetings].[UserEmailAlias];
    DELETE FROM [Meetings].[User];

    BEGIN TRY
        PRINT 'Reseeding tables...';
        DBCC CHECKIDENT ('[Meetings].[EmailVerificationToken]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[ReminderLog]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[RSVP]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[InvitationCode]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[CircleMembership]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[Event]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[Circle]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[UserIdentity]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[UserEmailAlias]', RESEED, 0);
        DBCC CHECKIDENT ('[Meetings].[User]', RESEED, 0);
    END TRY
    BEGIN CATCH
        PRINT 'Warning: identity reseed skipped: ' + ERROR_MESSAGE();
    END CATCH;
END;

PRINT '';
PRINT 'Inserting sample users, identities, and aliases...';
INSERT INTO [Meetings].[User] (DisplayName, IsActive, CreatedUtc)
VALUES
    ('Sammy Jordon', 1, GETUTCDATE()), 
    ('Marcus Lee', 1, GETUTCDATE()), 
    ('Priya Patel', 1, GETUTCDATE()),
    ('Jordan Smith', 1, GETUTCDATE()), 
    ('Sofia Rodriguez', 1, GETUTCDATE()), 
    ('Ben Okafor', 1, GETUTCDATE()),
    ('Grace Kim', 1, GETUTCDATE());

INSERT INTO [Meetings].[UserIdentity] (UserId, Provider, Issuer, Subject, CreatedUtc)
SELECT u.UserId, v.Provider, v.Issuer, v.Subject, GETUTCDATE()
FROM (VALUES
    ('Sammy Jordon', 1, 'https://login.microsoftonline.com/sample-tenant/v2.0', 'sample-entra-sammy-jordon'),
    ('Marcus Lee', 2, 'https://accounts.google.com', 'sample-google-marcus-lee'),
    ('Priya Patel', 3, 'https://www.facebook.com', 'sample-facebook-priya-patel'),
    ('Jordan Smith', 1, 'https://login.microsoftonline.com/sample-tenant/v2.0', 'sample-entra-jordan-smith'),
    ('Sofia Rodriguez', 2, 'https://accounts.google.com', 'sample-google-sofia-rodriguez'),
    ('Ben Okafor', 3, 'https://www.facebook.com', 'sample-facebook-ben-okafor'),
    ('Grace Kim', 1, 'https://login.microsoftonline.com/sample-tenant/v2.0', 'sample-entra-grace-kim')
) AS v(DisplayName, Provider, Issuer, Subject)
INNER JOIN [Meetings].[User] AS u ON u.DisplayName = v.DisplayName;

INSERT INTO [Meetings].[UserEmailAlias] (UserId, EmailAddress, NormalizedEmailAddress, IsVerified, VerifiedUtc, IsPrimary, CreatedUtc)
SELECT u.UserId, v.EmailAddress, UPPER(v.EmailAddress), 1, GETUTCDATE(), 1, GETUTCDATE()
FROM (VALUES
    ('Sammy Jordon', 'sammy.jordon@example.com'), 
    ('Marcus Lee', 'marcus.lee@example.com'), 
    ('Priya Patel', 'priya.patel@example.com'),
    ('Jordan Smith', 'jordan.smith@example.com'), 
    ('Sofia Rodriguez', 'sofia.rodriguez@example.com'), 
    ('Ben Okafor', 'ben.okafor@example.com'),
    ('Grace Kim', 'grace.kim@example.com')
) AS v(DisplayName, EmailAddress)
INNER JOIN [Meetings].[User] AS u ON u.DisplayName = v.DisplayName;

PRINT '';
PRINT 'Inserting sample circles and memberships...';
INSERT INTO [Meetings].[Circle] (Name, Description, CreatedByUserId, CreatedUtc, IsArchived)
SELECT v.Name, v.Description, i.UserId, GETUTCDATE(), 0
FROM (VALUES
    ('Neighborhood Book Club', 'Monthly book discussion for neighbors on Elm Street.', 'sample-entra-sammy-jordon'),
    ('Weekend Hikers', 'Casual weekend hiking group for local trails.', 'sample-google-marcus-lee'),
    ('Family Game Night', 'Recurring family-friendly board and card game gathering.', 'sample-facebook-priya-patel')
) AS v(Name, Description, CreatedBySubject)
INNER JOIN [Meetings].[UserIdentity] AS i ON i.Subject = v.CreatedBySubject;

INSERT INTO [Meetings].[CircleMembership] (CircleId, UserId, Role, JoinedUtc, LeftUtc)
SELECT c.CircleId, i.UserId, v.Role, GETUTCDATE(), NULL
FROM (VALUES
    ('Neighborhood Book Club', 'sample-entra-sammy-jordon', 'Owner'), 
    ('Neighborhood Book Club', 'sample-google-marcus-lee', 'Member'),
    ('Neighborhood Book Club', 'sample-facebook-priya-patel', 'Member'), 
    ('Neighborhood Book Club', 'sample-google-sofia-rodriguez', 'Member'),
    ('Weekend Hikers', 'sample-google-marcus-lee', 'Owner'), 
    ('Weekend Hikers', 'sample-entra-jordan-smith', 'Member'),
    ('Weekend Hikers', 'sample-facebook-ben-okafor', 'Member'), 
    ('Weekend Hikers', 'sample-entra-grace-kim', 'Member'),
    ('Family Game Night', 'sample-facebook-priya-patel', 'Owner'),
    ('Family Game Night', 'sample-entra-sammy-jordon', 'Member'),
    ('Family Game Night', 'sample-entra-jordan-smith', 'Member'), 
    ('Family Game Night', 'sample-facebook-ben-okafor', 'Member')
) AS v(CircleName, UserSubject, Role)
INNER JOIN [Meetings].[Circle] AS c ON c.Name = v.CircleName
INNER JOIN [Meetings].[UserIdentity] AS i ON i.Subject = v.UserSubject;

PRINT '';
PRINT 'Inserting recipient-bound sample invitations and verification challenges...';
INSERT INTO [Meetings].[InvitationCode] (CircleId, Code, CreatedByUserId, RecipientEmailAddress, NormalizedRecipientEmailAddress, CreatedUtc, ExpiresUtc)
SELECT c.CircleId, v.Code, i.UserId, v.RecipientEmailAddress, UPPER(v.RecipientEmailAddress), GETUTCDATE(), DATEADD(DAY, 30, GETUTCDATE())
FROM (VALUES
    ('Neighborhood Book Club', 'BOOK-CLUB-24X7', 'sample-entra-sammy-jordon', 'bookclub.guest@example.com'),
    ('Weekend Hikers', 'HIKE-CREW-88ZQ', 'sample-google-marcus-lee', 'hiker.guest@example.com'),
    ('Family Game Night', 'GAME-NIGHT-5F2K', 'sample-facebook-priya-patel', 'gamenight.guest@example.com')
) AS v(CircleName, Code, CreatedBySubject, RecipientEmailAddress)
INNER JOIN [Meetings].[Circle] AS c ON c.Name = v.CircleName
INNER JOIN [Meetings].[UserIdentity] AS i ON i.Subject = v.CreatedBySubject;

INSERT INTO [Meetings].[EmailVerificationToken] (TokenHash, NormalizedEmailAddress, InvitationCodeId, UserEmailAliasId, ExpiresUtc, UsedUtc, CreatedUtc)
SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', i.Code + ':sample-verification-token'), 2), i.NormalizedRecipientEmailAddress, i.InvitationCodeId, NULL, DATEADD(HOUR, 24, GETUTCDATE()), NULL, GETUTCDATE()
FROM [Meetings].[InvitationCode] AS i
WHERE i.Code IN ('BOOK-CLUB-24X7', 'HIKE-CREW-88ZQ', 'GAME-NIGHT-5F2K');

PRINT '';
PRINT 'Inserting sample events...';
INSERT INTO [Meetings].[Event] (CircleId, Title, Details, StartsUtc, EndsUtc, IsRecurring, RsvpMode, RecurrenceRule, CreatedByUserId, CreatedUtc)
SELECT c.CircleId, v.Title, v.Details, v.StartsUtc, v.EndsUtc, v.IsRecurring, v.RsvpMode, v.RecurrenceRule, i.UserId, GETUTCDATE()
FROM (VALUES
    ('Neighborhood Book Club', 'September Book Discussion', 'Discussing "The Midnight Library" over coffee.', DATEADD(DAY, 7, GETUTCDATE()), DATEADD(MINUTE, 90, DATEADD(DAY, 7, GETUTCDATE())), 0, 0, NULL, 'sample-entra-sammy-jordon'),
    ('Neighborhood Book Club', 'October Book Discussion', 'Book TBD - vote in the group chat.', DATEADD(DAY, 37, GETUTCDATE()), DATEADD(MINUTE, 90, DATEADD(DAY, 37, GETUTCDATE())), 0, 0, NULL, 'sample-entra-sammy-jordon'),
    ('Weekend Hikers', 'Ridge Trail Hike', 'Moderate 6-mile loop, bring water.', DATEADD(DAY, 3, GETUTCDATE()), DATEADD(HOUR, 3, DATEADD(DAY, 3, GETUTCDATE())), 0, 0, NULL, 'sample-google-marcus-lee'),
    ('Weekend Hikers', 'Sunset Ridge Series', 'Weekly sunset hikes all summer.', DATEADD(DAY, 10, GETUTCDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 10, GETUTCDATE())), 1, 1, 'FREQ=WEEKLY;BYDAY=SA', 'sample-google-marcus-lee'),
    ('Weekend Hikers', 'Waterfall Trailhead Meetup', 'Short scenic hike with a picnic after.', DATEADD(DAY, 21, GETUTCDATE()), DATEADD(HOUR, 4, DATEADD(DAY, 21, GETUTCDATE())), 0, 0, NULL, 'sample-facebook-ben-okafor'),
    ('Family Game Night', 'Board Game Bonanza', 'Bring your favorite board game to share.', DATEADD(DAY, 5, GETUTCDATE()), DATEADD(HOUR, 3, DATEADD(DAY, 5, GETUTCDATE())), 0, 0, NULL, 'sample-facebook-priya-patel'),
    ('Family Game Night', 'Monthly Card Night', 'Recurring monthly card games.', DATEADD(DAY, 14, GETUTCDATE()), DATEADD(HOUR, 3, DATEADD(DAY, 14, GETUTCDATE())), 1, 0, 'FREQ=MONTHLY;BYDAY=2FR', 'sample-facebook-priya-patel'),
    ('Family Game Night', 'Kids vs Parents Trivia', 'Trivia night with prizes for the kids.', DATEADD(DAY, 28, GETUTCDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 28, GETUTCDATE())), 0, 0, NULL, 'sample-entra-jordan-smith')
) AS v(CircleName, Title, Details, StartsUtc, EndsUtc, IsRecurring, RsvpMode, RecurrenceRule, CreatedBySubject)
INNER JOIN [Meetings].[Circle] AS c ON c.Name = v.CircleName
INNER JOIN [Meetings].[UserIdentity] AS i ON i.Subject = v.CreatedBySubject;

PRINT '';
PRINT 'Inserting sample RSVPs...';
INSERT INTO [Meetings].[RSVP] (EventId, CircleId, UserId, Status, Notes, OccurrenceDate, RespondedUtc)
SELECT e.EventId, e.CircleId, i.UserId, v.Status, v.Notes, NULL, GETUTCDATE()
FROM (VALUES
    ('September Book Discussion', 'sample-entra-sammy-jordon', 'Accepted', 'Bringing snacks.'),
    ('September Book Discussion', 'sample-google-marcus-lee', 'Accepted', NULL),
    ('September Book Discussion', 'sample-facebook-priya-patel', 'Declined', 'Out of town that week.'),
    ('September Book Discussion', 'sample-google-sofia-rodriguez', 'Pending', NULL),
    ('Ridge Trail Hike', 'sample-google-marcus-lee', 'Accepted', NULL),
    ('Ridge Trail Hike', 'sample-entra-jordan-smith', 'Accepted', 'Bringing trekking poles.'),
    ('Ridge Trail Hike', 'sample-facebook-ben-okafor', 'Declined', 'Recovering from a cold.'),
    ('Board Game Bonanza', 'sample-facebook-priya-patel', 'Accepted', NULL),
    ('Board Game Bonanza', 'sample-entra-sammy-jordon', 'Accepted', 'Bringing Catan.'),
    ('Board Game Bonanza', 'sample-entra-jordan-smith', 'Pending', NULL)
) AS v(EventTitle, UserSubject, Status, Notes)
INNER JOIN [Meetings].[Event] AS e ON e.Title = v.EventTitle
INNER JOIN [Meetings].[UserIdentity] AS i ON i.Subject = v.UserSubject;

PRINT '';
PRINT 'Sample data load complete.';

-- Get User List
SELECT TOP 500 u.UserId, u.DisplayName as UserName, ua.EmailAddress, ua.IsPrimary, ui.Issuer as IdentityIssuer, ui.Subject as IdentitySubject, ui.Provider as IdentityProvider
FROM [Meetings].[User] u 
INNER JOIN [Meetings].[UserIdentity] ui on u.UserId = ui.UserId
INNER JOIN [Meetings].[UserEmailAlias] ua on u.UserId = ua.UserId

-- Get Circle List
SELECT TOP 500 c.CircleId, c.Name as CircleName, c.Description as CircleDesc, u.UserId, u.DisplayName as UserName, cm.Role
FROM [Meetings].[Circle] c 
INNER JOIN [Meetings].[CircleMembership] cm ON c.CircleId = cm.CircleId
INNER JOIN [Meetings].[User] u on cm.UserId = u.UserId

-- Get Event Data
SELECT TOP 500 e.EventId, e.Title as EventTitle,e.StartsUtc as EventStartDate, u.DisplayName as UserName, r.Status as RSVPStatus, r.Notes as RSVPNotes
FROM [Meetings].[Event] e
INNER JOIN [Meetings].[RSVP] r on e.EventId = r.EventId
INNER JOIN [Meetings].[User] u on r.UserId = u.UserId

-- SELECT * From [Meetings].[User]
-- SELECT * FROM [Meetings].[UserIdentity];
-- SELECT * FROM [Meetings].[UserEmailAlias];
-- SELECT * FROM [Meetings].[EmailVerificationToken];
-- SELECT * FROM [Meetings].[Circle]
-- SELECT * FROM [Meetings].[CircleMembership]
-- SELECT * FROM [Meetings].[Event];
-- SELECT * FROM [Meetings].[ReminderLog];
-- SELECT * FROM [Meetings].[RSVP];
-- SELECT * FROM [Meetings].[InvitationCode];

