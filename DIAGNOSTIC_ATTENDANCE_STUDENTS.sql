-- =====================================================
-- DIAGNOSTIC SCRIPT: Check Attendance Student Issue
-- Date: December 28, 2025
-- =====================================================
-- This script helps diagnose why students don't appear
-- when marking attendance
-- =====================================================

PRINT '========================================='
PRINT '🔍 ATTENDANCE DIAGNOSTIC REPORT'
PRINT '========================================='
PRINT ''

-- =====================================================
-- 1. Check overall student course registrations
-- =====================================================
PRINT '1️⃣ OVERALL REGISTRATION STATUS'
PRINT '---------------------------------------'

DECLARE @TotalReg INT, @ActiveReg INT, @InactiveReg INT

SELECT @TotalReg = COUNT(*) FROM StudentCourseRegistrations
SELECT @ActiveReg = COUNT(*) FROM StudentCourseRegistrations WHERE IsActive = 1
SELECT @InactiveReg = COUNT(*) FROM StudentCourseRegistrations WHERE IsActive = 0

PRINT 'Total Registrations: ' + CAST(@TotalReg AS VARCHAR)
PRINT 'Active Registrations: ' + CAST(@ActiveReg AS VARCHAR)
PRINT 'Inactive Registrations: ' + CAST(@InactiveReg AS VARCHAR)

IF @InactiveReg > 0
BEGIN
    PRINT ''
    PRINT '⚠️  WARNING: Found ' + CAST(@InactiveReg AS VARCHAR) + ' inactive registration(s)!'
    PRINT '⚠️  These students will NOT appear when marking attendance!'
    PRINT ''
END
ELSE
BEGIN
    PRINT ''
    PRINT '✅ All registrations are active!'
    PRINT ''
END

PRINT ''

-- =====================================================
-- 2. Course-by-course breakdown
-- =====================================================
PRINT '2️⃣ REGISTRATION STATUS BY COURSE'
PRINT '---------------------------------------'
PRINT ''

SELECT
    c.Id AS CourseId,
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS TotalEnrolled,
    SUM(CASE WHEN scr.IsActive = 1 THEN 1 ELSE 0 END) AS VisibleInAttendance,
    SUM(CASE WHEN scr.IsActive = 0 THEN 1 ELSE 0 END) AS HiddenFromAttendance,
    CASE
        WHEN SUM(CASE WHEN scr.IsActive = 0 THEN 1 ELSE 0 END) > 0 
        THEN '⚠️  HAS HIDDEN STUDENTS!'
        ELSE '✅ All students visible'
    END AS Status
FROM StudentCourseRegistrations scr
INNER JOIN Courses c ON scr.CourseId = c.Id
GROUP BY c.Id, c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT ''

-- =====================================================
-- 3. Show problematic courses (with inactive students)
-- =====================================================
IF @InactiveReg > 0
BEGIN
    PRINT '3️⃣ COURSES WITH HIDDEN STUDENTS (PROBLEM COURSES)'
    PRINT '---------------------------------------'
    PRINT ''
    
    SELECT
        c.CourseCode,
        c.CourseName,
        s.StudentNumber,
        s.FirstName + ' ' + s.LastName AS StudentName,
        scr.IsActive,
        scr.RegisteredAt,
        CASE 
            WHEN scr.IsActive = 0 THEN '❌ WILL NOT APPEAR IN ATTENDANCE'
            ELSE '✅ Visible'
        END AS Status
    FROM StudentCourseRegistrations scr
    INNER JOIN Courses c ON scr.CourseId = c.Id
    INNER JOIN Students s ON scr.StudentId = s.Id
    WHERE scr.IsActive = 0
    ORDER BY c.CourseCode, s.StudentNumber
    
    PRINT ''
    PRINT ''
END

-- =====================================================
-- 4. Show courses with NO students at all
-- =====================================================
PRINT '4️⃣ COURSES WITH NO STUDENTS ENROLLED'
PRINT '---------------------------------------'
PRINT ''

SELECT
    c.Id,
    c.CourseCode,
    c.CourseName,
    CASE 
        WHEN c.IsActive = 1 THEN 'Active'
        ELSE 'Inactive'
    END AS CourseStatus
FROM Courses c
WHERE c.Id NOT IN (SELECT DISTINCT CourseId FROM StudentCourseRegistrations)
ORDER BY c.CourseCode

PRINT ''
PRINT ''

-- =====================================================
-- 5. Recommended actions
-- =====================================================
PRINT '5️⃣ RECOMMENDED ACTIONS'
PRINT '---------------------------------------'
PRINT ''

IF @InactiveReg > 0
BEGIN
    PRINT '⚠️  ACTION REQUIRED!'
    PRINT ''
    PRINT '   You have ' + CAST(@InactiveReg AS VARCHAR) + ' inactive student registration(s).'
    PRINT '   These students will NOT appear when marking attendance.'
    PRINT ''
    PRINT '   📝 SOLUTION OPTIONS:'
    PRINT ''
    PRINT '   OPTION 1 - Admin Panel (Easiest):'
    PRINT '   ✓ Login as Admin'
    PRINT '   ✓ Go to: Admin → Assign Courses to Students'
    PRINT '   ✓ Click "Fix No Students Found Issue" button'
    PRINT ''
    PRINT '   OPTION 2 - SQL Fix (Quick):'
    PRINT '   ✓ Execute: FIX_ATTENDANCE_STUDENTS_NOW.sql'
    PRINT ''
    PRINT '   OPTION 3 - Manual SQL (Advanced):'
    PRINT '   ✓ Run this command:'
    PRINT '     UPDATE StudentCourseRegistrations SET IsActive = 1 WHERE IsActive = 0'
    PRINT ''
END
ELSE
BEGIN
    PRINT '✅ No issues found with student registrations!'
    PRINT ''
    PRINT '   All student registrations are active.'
    PRINT '   Students should appear normally when marking attendance.'
    PRINT ''
    PRINT '   If you still see "No Students Found":'
    PRINT '   • Make sure you are selecting the correct course'
    PRINT '   • Check that students are actually enrolled (see section 4 above)'
    PRINT '   • Verify the timetable has a lecture scheduled for the selected day'
    PRINT ''
END

PRINT ''
PRINT '========================================='
PRINT '✅ DIAGNOSTIC COMPLETE'
PRINT '========================================='
PRINT ''
PRINT 'Report generated: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT ''
