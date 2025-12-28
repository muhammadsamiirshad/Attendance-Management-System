-- =====================================================
-- QUICK FIX - Activate All Student Registrations
-- Run this FIRST to fix attendance "No Students Found"
-- =====================================================

PRINT '🔧 Activating all student course registrations...'
PRINT ''

-- Show before state
PRINT 'BEFORE:'
SELECT 
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveRegistrations,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveRegistrations,
    COUNT(*) AS TotalRegistrations
FROM StudentCourseRegistrations

-- FIX IT
UPDATE StudentCourseRegistrations
SET IsActive = 1
WHERE IsActive = 0

PRINT ''
PRINT 'AFTER:'
SELECT 
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveRegistrations,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveRegistrations,
    COUNT(*) AS TotalRegistrations
FROM StudentCourseRegistrations

PRINT ''
PRINT '✅ DONE! Now test attendance marking in the application.'
