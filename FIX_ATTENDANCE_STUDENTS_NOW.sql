-- =====================================================
-- IMMEDIATE FIX FOR "NO STUDENTS FOUND" IN ATTENDANCE
-- Date: December 28, 2025
-- =====================================================
-- This script will immediately fix the issue where students
-- don't show up when marking attendance
-- =====================================================

PRINT '========================================='
PRINT 'FIXING ATTENDANCE "NO STUDENTS" ISSUE'
PRINT '========================================='
PRINT ''

-- Step 1: Show current status
PRINT '📊 CURRENT STATUS:'
PRINT ''

SELECT 
    'Total Student Registrations' AS Metric,
    COUNT(*) AS Count
FROM StudentCourseRegistrations

UNION ALL

SELECT 
    'Active Registrations' AS Metric,
    COUNT(*) AS Count
FROM StudentCourseRegistrations
WHERE IsActive = 1

UNION ALL

SELECT 
    'Inactive Registrations (THE PROBLEM!)' AS Metric,
    COUNT(*) AS Count
FROM StudentCourseRegistrations
WHERE IsActive = 0

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- Step 2: Show which students will be fixed
PRINT '👥 STUDENTS THAT WILL BE ACTIVATED:'
PRINT ''

SELECT 
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode,
    c.CourseName,
    CASE WHEN scr.IsActive = 1 THEN 'Active' ELSE '❌ INACTIVE' END AS CurrentStatus
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 0
ORDER BY c.CourseCode, s.StudentNumber

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- Step 3: Apply the fix
PRINT '🔧 APPLYING FIX...'
PRINT ''

DECLARE @InactiveCount INT
SELECT @InactiveCount = COUNT(*) FROM StudentCourseRegistrations WHERE IsActive = 0

IF @InactiveCount > 0
BEGIN
    -- Activate all inactive registrations
    UPDATE StudentCourseRegistrations
    SET IsActive = 1
    WHERE IsActive = 0
    
    PRINT '✅ SUCCESS! Activated ' + CAST(@InactiveCount AS VARCHAR) + ' student course registration(s)'
    PRINT ''
END
ELSE
BEGIN
    PRINT 'ℹ️ All student course registrations are already active.'
    PRINT ''
END

PRINT '-------------------------------------------------------'
PRINT ''

-- Step 4: Verify the fix
PRINT '✓ VERIFICATION - STUDENTS NOW AVAILABLE FOR ATTENDANCE:'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS ActiveStudents,
    STRING_AGG(s.StudentNumber + ': ' + s.FirstName + ' ' + s.LastName, ', ') WITHIN GROUP (ORDER BY s.StudentNumber) AS Students
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
GROUP BY c.Id, c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '========================================='
PRINT '✅ FIX COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'Students should now appear when marking attendance.'
PRINT 'Try marking attendance again - the issue should be resolved.'
PRINT ''
