# Troubleshooting: Attendance Lock Issue

## Problem
Getting "Attendance Locked" or "No lecture scheduled" message when trying to mark attendance.

## Quick Diagnosis

### Step 1: Check the Error Message
When you click "Load Students", what EXACT message do you see?

**Option A**: "No lecture scheduled for this course on [DayName] ([Date])"
- **Cause**: No timetable entry exists for this course on this day
- **Solution**: Go to Step 2

**Option B**: "Attendance marking will be available from [Time]"
- **Cause**: You're trying too early (before 10-minute window)
- **Solution**: Wait until the specified time

**Option C**: "Attendance marking is locked. The window closed at [Time]"
- **Cause**: You're trying too late (after 10-minute grace period)
- **Solution**: Window has closed, contact admin if needed

---

## Step 2: Verify Timetable Entry Exists

### Check in the Database (for Admin):
1. Go to **Timetable** page
2. Look for the course you're trying to mark attendance for
3. Check if there's an entry for the day you selected

### What to Look For:
- ✅ **Course**: Must match exactly
- ✅ **Day**: Must match the day of week you selected
- ✅ **Start Time**: Should be close to current time (within the window)
- ✅ **Active Status**: Must be active (not disabled)
- ✅ **Teacher Assignment**: You must be assigned to this course

**Example**:
```
Course: CS101 - Programming Fundamentals
Day: Saturday
Start Time: 12:30 PM
End Time: 2:00 PM
Status: Active ✓
```

---

## Step 3: Verify Course Assignment

### Check if you're assigned to teach this course:
1. Go to your **Dashboard**
2. Check the "Assigned Courses" or "My Courses" section
3. Verify the course appears in your list

**If the course is NOT in your list**:
- Contact admin to assign you to the course
- You cannot mark attendance for courses you don't teach

---

## Step 4: Check Date and Time

### Current System Information:
```powershell
# Check current date/time on server
Get-Date
```

### Verify:
1. **Date Selected**: What date did you select in the form?
2. **Current Date**: Is it the same as selected date?
3. **Day of Week**: Does it match the timetable entry?

**Common Mistake**:
- Timetable has entry for **Thursday** at 12:30 PM
- You're trying to mark attendance on **Saturday** at 12:30 PM
- **Result**: "No lecture scheduled" (wrong day!)

---

## Step 5: Check Time Window

### Time Window Rules:
For a lecture scheduled at **12:30 PM - 2:00 PM**:

| Current Time | Window Status | Can Mark? |
|--------------|---------------|-----------|
| 12:15 PM | Too Early | ❌ NO |
| 12:20 PM | Window Open | ✅ YES |
| 12:30 PM | Lecture Start | ✅ YES |
| 1:30 PM | During Lecture | ✅ YES |
| 2:00 PM | Lecture Ends | ✅ YES |
| 2:10 PM | Window Closes | ✅ YES (last moment) |
| 2:15 PM | Too Late | ❌ NO (Locked) |

### Check Your Situation:
```
Timetable Entry:
- Start Time: 12:30 PM
- End Time: 2:00 PM

Window Times:
- Opens: 12:20 PM (10 min before)
- Closes: 2:10 PM (10 min after)

Your Current Time: ____:____ PM
Are you within the window? YES / NO
```

---

## Common Scenarios & Solutions

### Scenario 1: "I scheduled a class for TODAY but it says no lecture"
**Problem**: Timetable entries are by DAY OF WEEK, not by date.

**Example of the Issue**:
- Today is **Saturday, December 7, 2025**
- You added a timetable entry for **Monday**
- You try to mark attendance for **Saturday** → NO LECTURE FOUND

**Solution**: 
1. Go to **Timetable > Create**
2. Select the correct **Day of Week** (e.g., Saturday)
3. Set the start/end times
4. Save the entry
5. Try marking attendance again

### Scenario 2: "I'm trying at 12:30 but it's locked"
**Possible Causes**:
1. **Wrong Date Selected**: Check the date picker in the form
2. **No Timetable Entry**: No entry exists for this day
3. **Wrong Day of Week**: Timetable entry is for different day
4. **Time Zone Issue**: Server time vs. your local time mismatch

**Solution**:
```
1. Check the date you selected: ___________
2. Check today's day of week: ___________
3. Check timetable entry day: ___________
4. Do they match? YES / NO
```

### Scenario 3: "It worked yesterday but not today"
**Possible Causes**:
1. Yesterday was a different day of week
2. Timetable entry is not set for today's day
3. Timetable entry was deactivated

**Solution**:
- Check if timetable has an entry for today's day of week
- Verify the entry is still active
- Add a new timetable entry if needed

---

## Quick Fix Checklist

Before asking for help, verify:

- [ ] I am logged in as a **Teacher**
- [ ] The course appears in my **Assigned Courses** list
- [ ] There is a **Timetable Entry** for this course
- [ ] The timetable entry day matches the **day I selected**
- [ ] The timetable entry is **Active** (not disabled)
- [ ] The current time is **within the 20-minute window**
- [ ] I selected the **correct date** in the form
- [ ] The timetable start time is **close to current time**

---

## Still Not Working?

### Collect This Information:
1. **Your Role**: ___________ (Teacher/Admin)
2. **Course Code**: ___________
3. **Course Name**: ___________
4. **Date Selected**: ___________ (e.g., Dec 7, 2025)
5. **Day of Week**: ___________ (e.g., Saturday)
6. **Current Time**: ___________ (e.g., 12:30 PM)
7. **Expected Lecture Time**: ___________ (e.g., 12:30-2:00)
8. **Exact Error Message**: ___________________________________________
9. **Timetable Entry Exists?**: YES / NO / DON'T KNOW
10. **Is Course in Your List?**: YES / NO

### Debug Steps:
1. Take a screenshot of the error message
2. Take a screenshot of the Timetable page
3. Take a screenshot of your Assigned Courses
4. Check the browser console for errors (F12)
5. Note the URL you're trying to access

---

## For Developers: Debug Mode

Add logging to see what's happening:

```csharp
// In ValidateAttendanceWindowAsync method
Console.WriteLine($"CourseId: {courseId}");
Console.WriteLine($"Date: {date}");
Console.WriteLine($"DayOfWeek: {date.DayOfWeek}");
Console.WriteLine($"Lecture found: {lecture != null}");
if (lecture != null)
{
    Console.WriteLine($"Lecture Start: {lecture.StartTime}");
    Console.WriteLine($"Lecture End: {lecture.EndTime}");
}
Console.WriteLine($"Current Time: {DateTime.Now}");
```

Check the console output when you try to load students.

---

## Contact Support

If you've tried everything and it still doesn't work:
- Email: support@example.com
- Include all information from "Still Not Working?" section
- Attach screenshots
- Describe step-by-step what you did

---

**Last Updated**: December 2025
**Version**: 1.0
