-- ========================================
-- SQL Diagnostic Script for Attendance Issue
-- Run this in SQL Server Management Studio or Azure Data Studio
-- ========================================

PRINT '===== ATTENDANCE SYSTEM DIAGNOSTICS ====='
PRINT ''

-- 1. Check if Students exist
PRINT '1. CHECKING STUDENTS...'
SELECT COUNT(*) AS TotalStudents FROM Students
SELECT TOP 5 Id, StudentNumber, FirstName, LastName, Email FROM Students
PRINT ''

-- 2. Check if Courses exist
PRINT '2. CHECKING COURSES...'
SELECT COUNT(*) AS TotalCourses FROM Courses WHERE IsActive = 1
SELECT TOP 5 Id, CourseCode, CourseName, Department FROM Courses WHERE IsActive = 1
PRINT ''

-- 3. Check if StudentCourseRegistrations exist
PRINT '3. CHECKING STUDENT COURSE REGISTRATIONS...'
SELECT COUNT(*) AS TotalRegistrations FROM StudentCourseRegistrations WHERE IsActive = 1
PRINT ''

-- 4. Detailed view of Student-Course assignments
PRINT '4. STUDENT-COURSE ASSIGNMENTS (Active Only)...'
SELECT 
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS StudentName,
    c.CourseCode,
    c.CourseName,
    scr.RegisteredDate,
    scr.IsActive
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
INNER JOIN Courses c ON scr.CourseId = c.Id
WHERE scr.IsActive = 1
ORDER BY c.CourseCode, s.StudentNumber
PRINT ''

-- 5. Check for Orphaned Registrations (missing Students or Courses)
PRINT '5. CHECKING FOR DATA INTEGRITY ISSUES...'
PRINT 'Orphaned Registrations (Student not found):'
SELECT COUNT(*) FROM StudentCourseRegistrations scr
LEFT JOIN Students s ON scr.StudentId = s.Id
WHERE s.Id IS NULL

PRINT 'Orphaned Registrations (Course not found):'
SELECT COUNT(*) FROM StudentCourseRegistrations scr
LEFT JOIN Courses c ON scr.CourseId = c.Id
WHERE c.Id IS NULL
PRINT ''

-- 6. Students with AppUser data
PRINT '6. CHECKING APPUSER NAVIGATION...'
SELECT 
    s.Id AS StudentId,
    s.StudentNumber,
    s.AppUserId,
    au.UserName,
    au.Email,
    CASE WHEN au.Id IS NULL THEN 'MISSING' ELSE 'OK' END AS AppUserStatus
FROM Students s
LEFT JOIN AspNetUsers au ON s.AppUserId = au.Id
WHERE au.Id IS NULL OR au.Id IS NOT NULL
ORDER BY AppUserStatus DESC, s.StudentNumber
PRINT ''

-- 7. Check Sections and StudentSection assignments
PRINT '7. CHECKING SECTIONS...'
SELECT COUNT(*) AS TotalSections FROM Sections WHERE IsActive = 1
SELECT 
    sec.SectionName,
    COUNT(ss.StudentId) AS StudentCount
FROM Sections sec
LEFT JOIN StudentSections ss ON sec.Id = ss.SectionId AND ss.IsActive = 1
WHERE sec.IsActive = 1
GROUP BY sec.Id, sec.SectionName
ORDER BY sec.SectionName
PRINT ''

-- 8. Check Teachers and Course Assignments
PRINT '8. CHECKING TEACHER-COURSE ASSIGNMENTS...'
SELECT 
    t.TeacherNumber,
    t.FirstName + ' ' + t.LastName AS TeacherName,
    c.CourseCode,
    c.CourseName,
    ca.IsActive
FROM CourseAssignments ca
INNER JOIN Teachers t ON ca.TeacherId = t.Id
INNER JOIN Courses c ON ca.CourseId = c.Id
WHERE ca.IsActive = 1
ORDER BY t.TeacherNumber, c.CourseCode
PRINT ''

-- 9. Check Timetables
PRINT '9. CHECKING TIMETABLES...'
SELECT COUNT(*) AS TotalTimetables FROM Timetables WHERE IsActive = 1
SELECT TOP 5
    c.CourseCode,
    t.FirstName + ' ' + t.LastName AS TeacherName,
    sec.SectionName,
    tt.Day,
    tt.StartTime,
    tt.EndTime,
    tt.Classroom
FROM Timetables tt
INNER JOIN Courses c ON tt.CourseId = c.Id
INNER JOIN Teachers t ON tt.TeacherId = t.Id
INNER JOIN Sections sec ON tt.SectionId = sec.Id
WHERE tt.IsActive = 1
ORDER BY tt.Day, tt.StartTime
PRINT ''

-- 10. Specific test for a course (replace CourseId with actual ID)
PRINT '10. DETAILED CHECK FOR SPECIFIC COURSE (CourseId = 1)...'
DECLARE @TestCourseId INT = 1

SELECT 
    'Course Info' AS InfoType,
    CAST(Id AS VARCHAR) AS Id,
    CourseCode AS Code,
    CourseName AS Name
FROM Courses 
WHERE Id = @TestCourseId

UNION ALL

SELECT 
    'Registered Students' AS InfoType,
    CAST(s.Id AS VARCHAR) AS Id,
    s.StudentNumber AS Code,
    s.FirstName + ' ' + s.LastName AS Name
FROM StudentCourseRegistrations scr
INNER JOIN Students s ON scr.StudentId = s.Id
WHERE scr.CourseId = @TestCourseId AND scr.IsActive = 1

PRINT ''

-- 11. Check Attendance Records
PRINT '11. CHECKING EXISTING ATTENDANCE RECORDS...'
SELECT COUNT(*) AS TotalAttendanceRecords FROM Attendances
SELECT TOP 10
    a.Date,
    c.CourseCode,
    s.StudentNumber,
    a.Status,
    a.MarkedAt
FROM Attendances a
INNER JOIN Courses c ON a.CourseId = c.Id
INNER JOIN Students s ON a.StudentId = s.Id
ORDER BY a.Date DESC, c.CourseCode
PRINT ''

PRINT '===== DIAGNOSTICS COMPLETE ====='
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. If "TotalRegistrations" is 0, you need to assign students to courses'
PRINT '2. If "Orphaned Registrations" > 0, you have data integrity issues'
PRINT '3. If "AppUserStatus" shows MISSING, student accounts are incomplete'
PRINT '4. Check the "STUDENT-COURSE ASSIGNMENTS" section to see who is enrolled where'
PRINT ''
