-- SQL Query to Check Existing Timetable Entries
-- Run this in your database to see what days have lectures scheduled

SELECT 
    T.Id,
    C.CourseName,
    C.CourseCode,
    CASE T.Day
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END AS DayOfWeek,
    T.StartTime,
    T.EndTime,
    T.Classroom,
    T.IsActive,
    TR.FirstName + ' ' + TR.LastName AS TeacherName
FROM Timetables T
INNER JOIN Courses C ON T.CourseId = C.Id
INNER JOIN Teachers TR ON T.TeacherId = TR.Id
WHERE T.IsActive = 1
ORDER BY T.Day, T.StartTime;

-- If no results, you need to create timetable entries!
-- If results show only Monday-Friday, you need to add Sunday entry for today's testing
