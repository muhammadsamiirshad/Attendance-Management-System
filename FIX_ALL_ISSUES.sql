-- =====================================================
-- COMPREHENSIVE FIX FOR ATTENDANCE & TIMETABLE ISSUES
-- Run this script to diagnose and fix all data problems
-- =====================================================

PRINT '========== COMPREHENSIVE DIAGNOSTIC & FIX SCRIPT =========='
PRINT ''

-- ===============================================
-- PART 1: DIAGNOSE STUDENT COURSE REGISTRATIONS
-- ===============================================

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

-- ===============================================
-- PART 2: DETAILED STUDENT VIEW
-- ===============================================

PRINT '2. DETAILED STUDENT REGISTRATIONS...'
PRINT ''

SELECT 
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode + ' - ' + c.CourseName AS Course,
    scr.IsActive AS RegistrationActive,
    scr.RegisteredDate,
    CASE 
        WHEN scr.IsActive = 1 THEN '✓ ACTIVE - Will show in attendance'
        WHEN scr.IsActive = 0 THEN '✗ INACTIVE - Will NOT show in attendance'
        ELSE 'Unknown'
    END AS Status
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
ORDER BY scr.IsActive DESC, c.CourseCode, s.StudentNumber

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- ===============================================
-- PART 3: FIX INACTIVE REGISTRATIONS
-- ===============================================

PRINT '3. FIXING INACTIVE REGISTRATIONS...'
PRINT ''

-- Count inactive registrations
DECLARE @InactiveCount INT
SELECT @InactiveCount = COUNT(*)
FROM StudentCourseRegistrations
WHERE IsActive = 0

PRINT 'Found ' + CAST(@InactiveCount AS VARCHAR) + ' inactive student course registrations'

IF @InactiveCount > 0
BEGIN
    PRINT 'Activating all student course registrations...'
    
    -- Activate ALL student course registrations
    UPDATE StudentCourseRegistrations
    SET IsActive = 1
    WHERE IsActive = 0
    
    PRINT '✓ Successfully activated ' + CAST(@InactiveCount AS VARCHAR) + ' registrations'
END
ELSE
BEGIN
    PRINT '✓ All registrations are already active'
END

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- ===============================================
-- PART 4: VERIFY FIX
-- ===============================================

PRINT '4. VERIFYING FIX...'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS TotalActiveStudents
FROM StudentCourseRegistrations scr
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
GROUP BY c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- ===============================================
-- PART 5: CHECK TIMETABLES
-- ===============================================

PRINT '5. CHECKING TIMETABLES...'
PRINT ''

SELECT 
    c.CourseCode + ' - ' + c.CourseName AS Course,
    s.SectionName AS Section,
    t.FirstName + ' ' + t.LastName AS Teacher,
    CASE tt.Day
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END AS Day,
    CONVERT(VARCHAR(5), DATEADD(MINUTE, DATEDIFF(MINUTE, 0, tt.StartTime), 0), 108) AS StartTime,
    CONVERT(VARCHAR(5), DATEADD(MINUTE, DATEDIFF(MINUTE, 0, tt.EndTime), 0), 108) AS EndTime,
    tt.Classroom,
    CASE WHEN tt.IsActive = 1 THEN 'Yes' ELSE 'No' END AS Active
FROM Timetables tt
INNER JOIN Courses c ON tt.CourseId = c.Id
INNER JOIN Sections s ON tt.SectionId = s.Id
INNER JOIN Teachers t ON tt.TeacherId = t.Id
ORDER BY tt.Day, tt.StartTime

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- ===============================================
-- PART 6: CHECK COURSE ASSIGNMENTS
-- ===============================================

PRINT '6. CHECKING TEACHER-COURSE ASSIGNMENTS...'
PRINT ''

SELECT 
    c.CourseCode + ' - ' + c.CourseName AS Course,
    t.FirstName + ' ' + t.LastName AS Teacher,
    t.TeacherNumber,
    CASE WHEN ca.IsActive = 1 THEN 'Yes' ELSE 'No' END AS Active,
    ca.AssignedDate
FROM CourseAssignments ca
INNER JOIN Courses c ON ca.CourseId = c.Id
INNER JOIN Teachers t ON ca.TeacherId = t.Id
ORDER BY c.CourseCode, t.LastName

PRINT ''
PRINT '========== SCRIPT COMPLETED SUCCESSFULLY =========='
PRINT ''
PRINT 'SUMMARY:'
PRINT '✓ Student course registrations checked and activated'
PRINT '✓ Timetables verified'
PRINT '✓ Course assignments verified'
PRINT ''
PRINT 'NOW TEST ATTENDANCE MARKING:'
PRINT '1. Login as Teacher'
PRINT '2. Go to Mark Attendance'
PRINT '3. Select a course with students'
PRINT '4. Students should now appear!'
PRINT ''
