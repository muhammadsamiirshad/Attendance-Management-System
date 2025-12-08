# Debug: Attendance Lock Issue - SOLUTION

## Current Status
- **Date**: December 7, 2025
- **Day**: Sunday
- **Time Window**: 12:30 PM - 2:00 PM
- **Issue**: "Attendance Locked - No lecture scheduled"

## Root Cause
The error message "No lecture scheduled for this course on the selected date" means:
- **There is NO timetable entry for this course on SUNDAY**

## The Problem
The system looks up lectures by **DAY OF WEEK**, not by specific date. 

```csharp
public async Task<Timetable?> GetUpcomingLectureAsync(int courseId, DateTime date)
{
    var dayOfWeek = (DayOfWeekEnum)date.DayOfWeek;  // Converts to Sunday, Monday, etc.
    var timetables = await _timetableRepo.GetTimetableByDayAsync(dayOfWeek);
    
    return timetables
        .Where(t => t.CourseId == courseId && t.IsActive)
        .OrderBy(t => t.StartTime)
        .FirstOrDefault();
}
```

**What this means**: 
- If you select date **12/07/2025 (Sunday)**, it looks for timetable entries with **Day = Sunday**
- If you select date **12/08/2025 (Monday)**, it looks for timetable entries with **Day = Monday**

## ✅ SOLUTION

You need to **create a timetable entry** for the course on the day you want to test.

### Option 1: Create Timetable Entry for Sunday (Today)

1. Go to **Timetable** management page (Admin)
2. Click **Create New Timetable**
3. Fill in the details:
   - **Course**: Select your course (e.g., CS101)
   - **Teacher**: Select yourself
   - **Section**: Select a section
   - **Day**: **Sunday** ⚠️ (This is critical!)
   - **Start Time**: **12:30 PM**
   - **End Time**: **2:00 PM**
   - **Classroom**: Any room (e.g., "Room 101")
   - **Active**: ✅ Checked
4. Click **Save**

### Option 2: Test with a Day That Already Has Timetable Entry

1. Check which days have timetable entries for your course
2. Select a date that matches one of those days
3. Try marking attendance

### How to Check Existing Timetable Entries

**Method 1: Via UI**
1. Go to **Timetable** page
2. Look at the weekly grid
3. Find your course in the schedule
4. Note which **days** it appears on

**Method 2: Via Database** (if you have access)
```sql
SELECT 
    C.CourseName, 
    C.CourseCode,
    T.Day,
    T.StartTime,
    T.EndTime,
    T.Classroom,
    T.IsActive,
    TR.FirstName + ' ' + TR.LastName AS Teacher
FROM Timetables T
JOIN Courses C ON T.CourseId = C.Id
JOIN Teachers TR ON T.TeacherId = TR.Id
WHERE T.IsActive = 1
ORDER BY T.Day, T.StartTime
```

## 🧪 Testing Steps

### Step 1: Create Test Timetable Entry
Create a timetable entry for **TODAY (Sunday)** with:
- Start Time: Current time + 2 minutes
- End Time: Current time + 92 minutes (1.5 hours later)
- Day: **Sunday**

### Step 2: Wait Until Window Opens
The window opens **10 minutes BEFORE** the lecture start time.

Example:
- If lecture starts at **1:00 PM**
- Window opens at **12:50 PM**
- Window closes at **1:10 PM** (10 minutes after 1:00 PM end time)

### Step 3: Try Marking Attendance
1. Go to **Attendance > Mark Attendance**
2. Select your course
3. Select today's date (**12/07/2025**)
4. Click **Load Students**
5. If window is open, you'll see the student list
6. If too early, you'll see: "Attendance marking will be available from [time]"
7. If too late, you'll see: "Attendance marking is locked"

## 🔧 Quick Fix Commands

### Restart the Application (to load new code)
```powershell
# Stop the application (if running)
# Press Ctrl+C in the terminal where it's running

# OR restart IIS Express from Visual Studio
# OR rebuild and run:
cd "c:\Users\Administrator\Desktop\EAD\AMS\AMS"
dotnet build
dotnet run
```

### Check Server Time
```powershell
Get-Date -Format "yyyy-MM-dd HH:mm:ss dddd"
```

## 📋 Verification Checklist

Before testing, verify:
- [x] Timetable entry exists for the **DAY OF WEEK** (not just the date)
- [x] The day matches: If testing on Sunday, timetable must have Day = Sunday
- [x] Start time is set correctly (e.g., 12:30 PM)
- [x] End time is set correctly (e.g., 2:00 PM)
- [x] Timetable entry is **Active** (IsActive = true)
- [x] You are assigned as the teacher for that course
- [x] Current time is within the window (10 min before to 10 min after)
- [x] Application has been restarted (to load new code changes)

## 🎯 Common Mistakes

### Mistake 1: Wrong Day
❌ Creating timetable for Monday but testing on Sunday
✅ Create timetable for Sunday if testing on Sunday

### Mistake 2: Testing Outside Window
❌ Testing at 12:15 PM for 12:30 PM lecture (15 minutes early)
✅ Wait until 12:20 PM (10 minutes before)

### Mistake 3: Not Restarting App
❌ Code changes made but application not restarted
✅ Stop and restart the application

### Mistake 4: Inactive Timetable
❌ Timetable entry has IsActive = false
✅ Ensure IsActive = true

## 🔍 Debug Output

Add this to your `ValidateAttendanceWindowAsync` to debug:

```csharp
public async Task<AttendanceWindowStatus> ValidateAttendanceWindowAsync(int courseId, DateTime date)
{
    var status = new AttendanceWindowStatus();
    
    // DEBUG INFO
    var dayOfWeek = (DayOfWeekEnum)date.DayOfWeek;
    Console.WriteLine($"[DEBUG] Checking attendance for CourseId={courseId}, Date={date:yyyy-MM-dd}, DayOfWeek={dayOfWeek}");
    
    var lecture = await GetUpcomingLectureAsync(courseId, date);
    
    if (lecture == null)
    {
        Console.WriteLine($"[DEBUG] No lecture found for {dayOfWeek}");
        status.IsAllowed = false;
        status.Message = $"No lecture scheduled for this course on {dayOfWeek} ({date:yyyy-MM-dd}).";
        status.IsLocked = true;
        return status;
    }
    
    Console.WriteLine($"[DEBUG] Lecture found: Start={lecture.StartTime}, End={lecture.EndTime}");
    // ... rest of the method
}
```

## 📞 Next Steps

1. **Create a timetable entry for Sunday (today)** with:
   - Day: Sunday
   - Start: 12:30 PM (or current time + 5 minutes)
   - End: 2:00 PM
   - Active: Yes

2. **Restart your application**:
   - Stop the current running instance
   - Rebuild: `dotnet build`
   - Run: `dotnet run`

3. **Try marking attendance again**:
   - Select your course
   - Select today's date (12/07/2025)
   - Click "Load Students"

4. **Check the console output** for debug messages

## 🎓 Understanding the Time Window

```
Lecture: 12:30 PM - 2:00 PM
Window Opens: 12:20 PM (10 min before)
Window Closes: 2:10 PM (10 min after)

Timeline:
12:00 PM ❌ Too early
12:15 PM ❌ Too early  
12:20 PM ✅ Window opens
12:25 PM ✅ Can mark
12:30 PM ✅ Lecture starts - Can mark
12:45 PM ✅ During lecture - Can mark
2:00 PM  ✅ Lecture ends - Can mark
2:05 PM  ✅ Grace period - Can mark
2:10 PM  ✅ Last moment
2:15 PM  ❌ Too late - Locked
```

---

**Status**: Ready for testing once timetable entry is created for Sunday
