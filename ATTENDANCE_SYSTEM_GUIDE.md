# Attendance Management System - Complete Guide

## Overview

The AMS Attendance Management System provides a robust, time-locked attendance tracking solution with the following key features:

- **Time-Based Attendance Windows**: Teachers can only mark attendance during specific time windows
- **10-Minute Pre-Lecture Window**: Attendance marking becomes available 10 minutes before each scheduled lecture
- **Automatic Locking**: Attendance is automatically locked after the lecture ends
- **Optional Remarks**: Teachers can add optional notes for any student (sick leave, late arrival, etc.)
- **Real-time Validation**: Prevents attendance marking outside allowed time windows

---

## Key Features

### 1. Time-Based Attendance Lock System

The system enforces strict time windows for attendance marking:

#### Time Window Rules:
- **Window Opens**: 10 minutes before the scheduled lecture start time
- **Window Closes**: When the lecture ends (based on timetable)
- **Before Window**: Teachers receive a message indicating when marking will be available
- **After Window**: Attendance is permanently locked for that session

#### Example Scenario:
```
Lecture Schedule: Monday, 9:00 AM - 10:30 AM

Timeline:
- 8:49 AM ❌ Too early - "Attendance will be available from 8:50 AM"
- 8:50 AM ✅ Window opens - Attendance marking allowed
- 9:00 AM ✅ Lecture starts - Attendance marking still allowed
- 10:30 AM ✅ Lecture ends - Last chance to mark
- 10:31 AM ❌ Locked - "Attendance marking is locked. The lecture ended at 10:30 AM"
```

### 2. Optional Remarks Field

The `Remarks` field is completely optional:
- **Not Required**: Teachers can leave it empty for students with regular attendance
- **Use Cases**:
  - "Sick leave - medical certificate provided"
  - "Late arrival at 9:15 AM"
  - "Excused absence - university event"
  - "Left early - emergency"

### 3. Smart Validation

The system validates:
- ✅ Lecture exists in timetable for selected date
- ✅ Current time is within allowed window
- ✅ Course has registered students
- ✅ No duplicate attendance entries

---

## How to Mark Attendance (Teacher)

### Step 1: Navigate to Attendance Marking
1. Log in as a Teacher
2. Go to **Attendance > Mark Attendance**

### Step 2: Select Course and Date
1. Choose the course from dropdown
2. Select the date (defaults to today)
3. Click **Load Students**

### Step 3: Validate Time Window
The system will check if you're within the allowed time window:

**Success Response:**
```
✅ Attendance Window Open
Available until 10:30 AM
[Student list appears]
```

**Too Early Response:**
```
⏰ Attendance marking will be available from 8:50 AM (10 minutes before the lecture).
```

**Locked Response:**
```
🔒 Attendance marking is locked. The lecture ended at 10:30 AM.
```

### Step 4: Mark Attendance
1. Review the list of students
2. For each student:
   - **Present** (✅): Default selection
   - **Absent** (❌): Select if student is absent
   - **Remarks**: Optionally add notes (e.g., "Late at 9:15", "Sick leave")

3. Quick Actions:
   - **Mark All Present**: Sets all students to present
   - **Mark All Absent**: Sets all students to absent

4. Click **Save Attendance**

### Step 5: Confirmation
- Success: "Attendance marked successfully"
- The data is saved with your name and timestamp

---

## Technical Implementation

### Database Schema

#### Attendance Table
```sql
CREATE TABLE Attendance (
    Id INT PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    Date DATE NOT NULL,
    Status INT NOT NULL,  -- 0=Present, 1=Absent, 2=Late, 3=Excused
    Remarks NVARCHAR(500) NULL,  -- NULLABLE field
    CreatedAt DATETIME NOT NULL,
    CreatedBy NVARCHAR(256) NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
)
```

### Attendance Window Logic

The `ValidateAttendanceWindowAsync` method implements the time-lock logic:

```csharp
public async Task<AttendanceWindowStatus> ValidateAttendanceWindowAsync(int courseId, DateTime date)
{
    // 1. Get the lecture from timetable
    var lecture = await GetUpcomingLectureAsync(courseId, date);
    
    if (lecture == null)
        return new AttendanceWindowStatus { 
            IsAllowed = false, 
            IsLocked = true,
            Message = "No lecture scheduled for this course on the selected date."
        };
    
    // 2. Calculate time windows
    var lectureStartTime = date.Date.Add(lecture.StartTime);
    var lectureEndTime = date.Date.Add(lecture.EndTime);
    var windowStartTime = lectureStartTime.AddMinutes(-10);  // 10 min before
    
    var now = DateTime.Now;
    
    // 3. Check if we're before the window
    if (now < windowStartTime)
        return new AttendanceWindowStatus {
            IsAllowed = false,
            IsLocked = false,
            Message = $"Attendance marking will be available from {windowStartTime:hh:mm tt}"
        };
    
    // 4. Check if we're after the window
    if (now > lectureEndTime)
        return new AttendanceWindowStatus {
            IsAllowed = false,
            IsLocked = true,
            Message = $"Attendance marking is locked. The lecture ended at {lectureEndTime:hh:mm tt}."
        };
    
    // 5. We're in the valid window
    return new AttendanceWindowStatus {
        IsAllowed = true,
        IsLocked = false,
        Message = $"Attendance can be marked until {lectureEndTime:hh:mm tt}."
    };
}
```

### Model Classes

#### AttendanceWindowStatus
```csharp
public class AttendanceWindowStatus
{
    public bool IsAllowed { get; set; }
    public string Message { get; set; }
    public DateTime? LectureStartTime { get; set; }
    public DateTime? WindowStartTime { get; set; }
    public DateTime? LectureEndTime { get; set; }
    public bool IsLocked { get; set; }
}
```

#### StudentAttendanceItem
```csharp
public class StudentAttendanceItem
{
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public bool IsPresent { get; set; } = true;
    public string? Remarks { get; set; }  // NULLABLE - not required
}
```

---

## API Endpoints

### Mark Attendance
```http
POST /Attendance/MarkAttendance
Content-Type: application/x-www-form-urlencoded

CourseId=1
Date=2024-01-15
Students[0].StudentId=1
Students[0].IsPresent=true
Students[0].Remarks=
Students[1].StudentId=2
Students[1].IsPresent=false
Students[1].Remarks=Sick leave
```

**Response (Success):**
```json
{
    "success": true,
    "message": "Attendance marked successfully."
}
```

**Response (Locked):**
```json
{
    "success": false,
    "message": "Attendance marking is locked. The lecture ended at 10:30 AM.",
    "isLocked": true
}
```

### Load Students for Marking
```http
POST /Attendance/LoadStudentsForMarking
Content-Type: application/x-www-form-urlencoded

courseId=1
date=2024-01-15
```

**Response (Success):**
Returns HTML partial with student list

**Response (Locked):**
```json
{
    "success": false,
    "isLocked": true,
    "message": "Attendance marking is locked. The lecture ended at 10:30 AM.",
    "lectureStartTime": "09:00 AM",
    "windowStartTime": "08:50 AM"
}
```

---

## Best Practices

### For Teachers

1. **Mark Attendance Promptly**
   - Mark within the first 10-15 minutes of class
   - Don't wait until the end of the lecture

2. **Use Remarks Wisely**
   - Only add remarks when there's something noteworthy
   - Keep remarks professional and concise
   - Examples:
     - ✅ "Late at 9:15 - traffic"
     - ✅ "Medical certificate provided"
     - ❌ "Always late to class" (unprofessional)

3. **Double-Check Before Saving**
   - Review the attendance list carefully
   - Use "Mark All Present" as a starting point
   - Then mark exceptions

### For Administrators

1. **Ensure Accurate Timetable**
   - All lectures must be in the timetable
   - Correct start and end times are crucial
   - Mark inactive lectures when courses are cancelled

2. **Monitor Attendance Patterns**
   - Use reports to identify students with low attendance
   - Check for unmarked attendance sessions

3. **Handle Special Cases**
   - Make-up classes: Add to timetable
   - Cancelled classes: Mark timetable entry as inactive

---

## Troubleshooting

### "No lecture scheduled for this course on the selected date"

**Cause**: The timetable doesn't have an entry for this course on this day of the week

**Solution**:
1. Go to **Timetable Management**
2. Add a timetable entry for the course
3. Specify day, start time, end time

### "Attendance marking will be available from [time]"

**Cause**: You're trying to mark attendance too early

**Solution**:
- Wait until 10 minutes before the lecture
- Or ask admin to adjust timetable times if incorrect

### "Attendance marking is locked"

**Cause**: The lecture has ended

**Solution**:
- For current day: Attendance cannot be marked (enforced policy)
- Contact administrator if there's a genuine error
- Admin can manually adjust database if absolutely necessary

### "Failed to mark attendance"

**Possible Causes**:
1. Network connectivity issue
2. Session expired - try logging in again
3. Database connection problem
4. Validation error

**Solution**:
1. Check internet connection
2. Refresh page and try again
3. Log out and log back in
4. Contact IT support if persists

---

## Reporting & Analytics

### View Attendance Reports

1. **By Student**: See individual student attendance across all courses
2. **By Course**: View class attendance patterns
3. **Date Range**: Filter by custom date ranges
4. **Export**: Download reports as Excel/PDF

### Attendance Statistics

- **Attendance Percentage**: Calculated per student per course
- **Trend Analysis**: Visual charts showing attendance patterns
- **Alert System**: Notifications for students below threshold

---

## Security & Privacy

1. **Role-Based Access**:
   - Only teachers can mark attendance
   - Only for their assigned courses
   - Students can only view their own attendance

2. **Audit Trail**:
   - All attendance entries include `CreatedBy` and `CreatedAt`
   - Changes are tracked
   - Historical data is preserved

3. **Data Validation**:
   - All inputs are validated server-side
   - SQL injection protection
   - XSS prevention

---

## Future Enhancements

Planned features for upcoming releases:

- [ ] **Biometric Integration**: Fingerprint/facial recognition
- [ ] **Mobile App**: Native iOS/Android apps
- [ ] **QR Code Attendance**: Students scan QR code to mark attendance
- [ ] **Geofencing**: Ensure students are physically on campus
- [ ] **Email Notifications**: Alert students about marked absences
- [ ] **Bulk Operations**: Upload attendance via Excel
- [ ] **Attendance Appeal**: Students can request attendance correction

---

## Support

For technical support or feature requests:
- **Email**: support@ams.edu
- **Documentation**: [Internal Wiki]
- **Help Desk**: Submit a ticket through the portal

---

## Version History

- **v2.0** (Current) - Added time-locked attendance windows
- **v1.5** - Made Remarks field optional
- **v1.0** - Initial release with basic attendance tracking

---

*Last Updated: January 2024*
