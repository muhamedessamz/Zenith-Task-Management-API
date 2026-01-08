-- Zenith Database Diagnostic Script
-- Run this to check for data integrity issues

-- 1. Check all projects and their members
SELECT 
    p.Id AS ProjectId,
    p.Title AS ProjectName,
    p.UserId AS OwnerId,
    pm.UserId AS MemberUserId,
    pm.Role AS MemberRole,
    pm.JoinedAt
FROM Projects p
LEFT JOIN ProjectMembers pm ON p.Id = pm.ProjectId
ORDER BY p.Id, pm.UserId;

-- 2. Find projects WITHOUT any members (DATA INTEGRITY ISSUE!)
SELECT 
    p.Id,
    p.Title,
    p.UserId AS OwnerId,
    p.CreatedAt
FROM Projects p
WHERE NOT EXISTS (
    SELECT 1 FROM ProjectMembers pm WHERE pm.ProjectId = p.Id
);

-- 3. Count members per project
SELECT 
    p.Id,
    p.Title,
    COUNT(pm.Id) AS MemberCount
FROM Projects p
LEFT JOIN ProjectMembers pm ON p.Id = pm.ProjectId
GROUP BY p.Id, p.Title
ORDER BY p.Id;

-- 4. Check all users
SELECT Id, Email, DisplayName, FirstName, LastName, CreatedAt
FROM AspNetUsers
ORDER BY CreatedAt DESC;

-- 5. FIX: Add missing ProjectMember records for projects without members
-- (Run this if you find orphaned projects in query #2)
/*
INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedAt)
SELECT p.Id, p.UserId, 'Owner', p.CreatedAt
FROM Projects p
WHERE NOT EXISTS (
    SELECT 1 FROM ProjectMembers pm WHERE pm.ProjectId = p.Id
);
*/
