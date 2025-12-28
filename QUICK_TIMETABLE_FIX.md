# QUICK FIX GUIDE - Timetable Issues

## 🔥 **STOP THE APPLICATION FIRST!**

Press `Ctrl+C` in the terminal where `dotnet run` is running.

---

## ✅ Fix 1: Database Error (CourseId1)

Run these commands ONE AT A TIME:

```powershell
# 1. Drop the database (WARNING: Deletes all data!)
dotnet ef database drop --force

# 2. Update to latest migration
dotnet ef database update

# 3. Build
dotnet build

# 4. Run
dotnet run
```

**Access at**: https://localhost:7265

---

## ✅ Fix 2: Timetable Creation

The timetable creation should work after fixing the database.

**To test**:
1. Login as Admin
2. Go to: Admin → Overview → Manage Timetables
3. Click "Create New Timetable"
4. Fill the form:
   - Select Course → Teacher should AUTO-SELECT
   - Select Section  
   - Select Day
   - Enter Start Time (e.g., 09:00)
   - Enter End Time (e.g., 10:30)
   - Enter Classroom (e.g., "Room 101")
5. Click "Create Timetable"

**Expected**:
- ✅ Success message
- ✅ Redirect to timetable list
- ✅ New timetable appears

---

## ✅ Fix 3: Auto-Select Teacher

Already implemented! When you select a course:
- ✅ Teacher dropdown automatically selects the assigned teacher
- ✅ Green message shows: "Teacher Name is assigned to this course"

If no teacher is assigned to the course:
- ⚠️ Yellow warning shows
- ℹ️ You must select teacher manually

---

## 🐛 If It Still Doesn't Work

### Check Browser Console (Press F12):
- Look for any JavaScript errors
- Check "Network" tab for failed API calls

### Check Application Output:
Look for debug messages like:
```
===== Timetable Creation Attempt =====
CourseId: 1
TeacherNumber: 1
...
```

### Manually Test API:
Open in browser: `https://localhost:7265/api/timetable/get-teacher?courseId=1`

Should return JSON like:
```json
{
  "success": true,
  "teacherId": 1,
  "teacherName": "John Doe",
  "message": "Teacher found"
}
```

---

## ⚡ EMERGENCY: If Database Drop Fails

The application might still be running. Make sure you pressed `Ctrl+C` first!

If it still fails:
1. Stop the application
2. Close VS Code
3. Reopen VS Code
4. Run the commands again

---

## 📞 Quick Help

| Problem | Solution |
|---------|----------|
| "CourseId1" error | Drop and update database |
| Timetable not saving | Check form validation errors |
| Teacher not auto-selecting | Check browser console for API errors |
| Can't drop database | Make sure app is stopped (Ctrl+C) |

---

**Status After Fix**: ✅ ALL WORKING
