-- =====================================================
-- ATTENDANCE FIX - Activate Student Course Registrations
-- Run this in SQL Server Management Studio or Azure Data Studio
-- =====================================================

PRINT '====== ATTENDANCE DIAGNOSTIC & FIX SCRIPT ======'
PRINT ''

-- Step 1: Check current state
PRINT '1. CHECKING STUDENT COURSE REGISTRATIONS...'
PRINT ''

SELECT 
    c.Id AS CourseId,
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS TotalRegistrations,
    SUM(CASE WHEN scr.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveRegistrations,
    SUM(CASE WHEN scr.IsActive = 0 THEN 1 ELSE 0 END) AS InactiveRegistrations
FROM StudentCourseRegistrations scr
INNER JOIN Courses c ON scr.CourseId = c.Id
GROUP BY c.Id, c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- Step 2: Show detailed view of each registration
PRINT '2. DETAILED VIEW OF REGISTRATIONS...'
PRINT ''

SELECT 
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode,
    c.CourseName,
    scr.IsActive AS RegistrationActive,
    scr.RegisteredDate,
    CASE 
        WHEN scr.IsActive = 1 THEN 'Will appear in attendance'
        ELSE '❌ WILL NOT APPEAR - Needs activation'
    END AS AttendanceStatus
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
ORDER BY c.CourseCode, s.StudentNumber

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- Step 3: THE FIX - Activate ALL student registrations
PRINT '3. ACTIVATING ALL STUDENT COURSE REGISTRATIONS...'
PRINT ''

-- Uncomment the line below to actually run the fix
-- UPDATE StudentCourseRegistrations SET IsActive = 1

-- To see what would be updated without actually updating:
SELECT 
    scr.Id,
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode,
    scr.IsActive AS CurrentStatus,
    'Will be set to ACTIVE' AS NewStatus
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 0

PRINT ''
PRINT '⚠️ TO ACTUALLY FIX THE ISSUE:'
PRINT 'Uncomment this line in the script above:'
PRINT 'UPDATE StudentCourseRegistrations SET IsActive = 1'
PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- Step 4: Verification query (run AFTER uncommenting the UPDATE)
PRINT '4. VERIFICATION - Run this after activating registrations'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    COUNT(scr.StudentId) AS TotalActiveStudents,
    STRING_AGG(s.StudentNumber, ', ') AS StudentNumbers
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
GROUP BY c.Id, c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '====== END OF DIAGNOSTIC ======'
PRINT ''
PRINT 'INSTRUCTIONS:'
PRINT '1. Review the results above'
PRINT '2. If you see InactiveRegistrations > 0, that is the problem'
PRINT '3. Uncomment the UPDATE line in Step 3'
PRINT '4. Run the script again'
PRINT '5. Verify students now appear in Step 4'
PRINT '6. Try marking attendance again'
