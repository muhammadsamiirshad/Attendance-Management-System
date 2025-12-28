-- =====================================================
-- COMPREHENSIVE FIX FOR "NO STUDENTS FOUND" IN ATTENDANCE
-- Date: December 28, 2025
-- =====================================================

PRINT '========================================='
PRINT 'ATTENDANCE "NO STUDENTS" DIAGNOSTIC & FIX'
PRINT '========================================='
PRINT ''

-- =====================================================
-- STEP 1: CHECK STUDENT COURSE REGISTRATIONS
-- =====================================================

PRINT '1️⃣ CHECKING STUDENT COURSE REGISTRATIONS STATUS...'
PRINT ''

SELECT 
    c.Id AS CourseId,
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS TotalRegistrations,
    SUM(CASE WHEN scr.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveRegistrations,
    SUM(CASE WHEN scr.IsActive = 0 THEN 1 ELSE 0 END) AS InactiveRegistrations,
    CASE 
        WHEN SUM(CASE WHEN scr.IsActive = 1 THEN 1 ELSE 0 END) = 0 THEN '❌ NO ACTIVE - THIS IS THE PROBLEM!'
        ELSE '✅ Has active students'
    END AS Status
FROM StudentCourseRegistrations scr
INNER JOIN Courses c ON scr.CourseId = c.Id
GROUP BY c.Id, c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- STEP 2: DETAILED VIEW OF INACTIVE REGISTRATIONS
-- =====================================================

PRINT '2️⃣ SHOWING INACTIVE REGISTRATIONS (THE ROOT CAUSE)...'
PRINT ''

SELECT 
    s.Id AS StudentId,
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode,
    c.CourseName,
    scr.RegisteredDate,
    scr.IsActive,
    CASE 
        WHEN scr.IsActive = 0 THEN '❌ INACTIVE - Student will NOT appear in attendance'
        ELSE '✅ Active'
    END AS RegistrationStatus
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 0
ORDER BY c.CourseCode, s.StudentNumber

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- STEP 3: CHECK IF ANY STUDENTS EXIST AT ALL
-- =====================================================

PRINT '3️⃣ CHECKING IF STUDENTS EXIST IN THE SYSTEM...'
PRINT ''

SELECT 
    COUNT(*) AS TotalStudents,
    CASE 
        WHEN COUNT(*) = 0 THEN '❌ NO STUDENTS - Need to create students first!'
        ELSE '✅ Students exist'
    END AS Status
FROM Students

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- STEP 4: CHECK COURSE ASSIGNMENTS (TEACHERS)
-- =====================================================

PRINT '4️⃣ CHECKING COURSE ASSIGNMENTS TO TEACHERS...'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    t.FirstName + ' ' + t.LastName AS TeacherName,
    ca.IsActive,
    CASE 
        WHEN ca.IsActive = 1 THEN '✅ Active'
        ELSE '❌ Inactive'
    END AS AssignmentStatus
FROM CourseAssignments ca
INNER JOIN Courses c ON ca.CourseId = c.Id
INNER JOIN Teachers t ON ca.TeacherId = t.Id
ORDER BY c.CourseCode

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- 🔥 THE FIX: ACTIVATE ALL INACTIVE REGISTRATIONS
-- =====================================================

PRINT ''
PRINT '========================================='
PRINT '🔧 APPLYING FIX...'
PRINT '========================================='
PRINT ''

DECLARE @InactiveCount INT
SELECT @InactiveCount = COUNT(*) FROM StudentCourseRegistrations WHERE IsActive = 0

IF @InactiveCount > 0
BEGIN
    PRINT '⚠️ Found ' + CAST(@InactiveCount AS VARCHAR) + ' inactive registrations'
    PRINT '🔧 Activating all student course registrations...'
    PRINT ''
    
    -- Show what will be activated
    SELECT 
        s.StudentNumber,
        s.FirstName + ' ' + s.LastName AS StudentName,
        c.CourseCode,
        c.CourseName,
        'Will be activated' AS Action
    FROM StudentCourseRegistrations scr
    INNER JOIN Students s ON scr.StudentId = s.Id
    INNER JOIN Courses c ON scr.CourseId = c.Id
    WHERE scr.IsActive = 0
    
    -- APPLY THE FIX
    UPDATE StudentCourseRegistrations
    SET IsActive = 1
    WHERE IsActive = 0
    
    PRINT ''
    PRINT '✅ FIXED! Activated ' + CAST(@InactiveCount AS VARCHAR) + ' registrations'
    PRINT ''
END
ELSE
BEGIN
    PRINT '✅ All registrations are already active - no fix needed'
    PRINT ''
END

PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- STEP 5: VERIFY THE FIX
-- =====================================================

PRINT '5️⃣ VERIFYING THE FIX...'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    COUNT(*) AS TotalActiveStudents,
    CASE 
        WHEN COUNT(*) = 0 THEN '❌ Still no students - Check if students are enrolled at all'
        ELSE '✅ Students will now appear in attendance'
    END AS AttendanceStatus
FROM StudentCourseRegistrations scr
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
GROUP BY c.CourseCode, c.CourseName
ORDER BY c.CourseCode

PRINT ''
PRINT '-------------------------------------------------------'
PRINT ''

-- =====================================================
-- STEP 6: DETAILED ACTIVE STUDENTS LIST
-- =====================================================

PRINT '6️⃣ ACTIVE STUDENTS BY COURSE (WHAT TEACHER SHOULD SEE)...'
PRINT ''

SELECT 
    c.CourseCode,
    c.CourseName,
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    s.Email,
    scr.RegisteredDate,
    '✅ Will appear in attendance marking' AS Status
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
ORDER BY c.CourseCode, s.StudentNumber

PRINT ''
PRINT '========================================='
PRINT '✅ DIAGNOSTIC & FIX COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. Go to the attendance marking page'
PRINT '2. Select a course'
PRINT '3. Students should now appear!'
PRINT ''
PRINT 'If students still do not appear:'
PRINT '- Check if students are actually enrolled (see section 3 above)'
PRINT '- Check if the teacher is assigned to the course (see section 4 above)'
PRINT '- Check application logs for detailed error messages'
PRINT ''
