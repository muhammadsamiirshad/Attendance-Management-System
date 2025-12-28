# Quick Start: Auto-Select Teacher Feature

## 🚀 Get Started in 3 Steps

### Step 1: Run the Application
```bash
dotnet run
```
Navigate to `http://localhost:5000` (or the URL shown in terminal)

### Step 2: Login as Admin
- Username: `admin@ams.com`
- Password: Your admin password

### Step 3: Test the Feature
1. Go to **Admin** → **Manage Timetables**
2. Click **Create New Timetable**
3. Select a course from the dropdown
4. Watch the teacher auto-select! 🎉

---

## 📹 What You'll See

### Scenario A: Course with Assigned Teacher
```
1. Select Course: "CS101 - Introduction to Programming"
   ↓
2. Message appears: "Finding assigned teacher..."
   ↓
3. Teacher dropdown auto-selects: "John Doe (T12345)"
   ↓
4. Success message: "✓ John Doe (T12345) is assigned to this course"
```

### Scenario B: Course without Assigned Teacher
```
1. Select Course: "MATH201 - Calculus II"
   ↓
2. Message appears: "Finding assigned teacher..."
   ↓
3. Warning message: "⚠ No teacher is assigned to this course. Please select a teacher manually."
   ↓
4. You can manually select any teacher from the dropdown
```

---

## 🎨 Visual Indicators

| Icon | Meaning | Color |
|------|---------|-------|
| ⏳ | Searching for teacher... | Blue (Info) |
| ✓ | Teacher found and selected | Green (Success) |
| ⚠ | No teacher assigned | Yellow (Warning) |
| ✗ | Error loading teacher | Red (Danger) |

---

## 💡 Pro Tips

### Tip 1: Course Assignment First
Before creating timetables, ensure teachers are assigned to courses:
1. Go to **Admin** → **Assign Teacher to Course**
2. Select teacher and course
3. Click "Assign"

### Tip 2: Change Course Anytime
- You can change the course selection
- Teacher will automatically update
- No need to reload the page

### Tip 3: Manual Override
- Auto-selection is a suggestion, not a requirement
- You can manually change the teacher if needed
- Useful for substitute teachers or special cases

### Tip 4: Check the Message
- Always read the feedback message
- It tells you why a teacher was (or wasn't) selected
- Helps catch course assignment issues early

---

## 🔧 Troubleshooting

### Problem: Teacher doesn't auto-select
**Solution**: 
1. Check if teacher is assigned to that course
2. Go to **Admin** → **View All Courses**
3. Look for the course and check "Assigned Teacher" column
4. If blank, assign a teacher first

### Problem: "Error loading teacher" message
**Solution**:
1. Open browser Developer Tools (F12)
2. Check Console tab for errors
3. Ensure application is running
4. Try refreshing the page

### Problem: JavaScript not working
**Solution**:
1. Ensure JavaScript is enabled in browser
2. Check for browser extensions blocking scripts
3. Try a different browser (Chrome, Firefox, Edge)

---

## 📱 Mobile/Tablet Support

The feature works on all devices:
- ✅ Desktop browsers
- ✅ Tablets
- ✅ Mobile phones (via responsive design)

---

## 🎯 Use Cases

### Use Case 1: Regular Timetable Creation
Admin creates weekly schedule → Courses auto-populate teachers → Fast setup

### Use Case 2: Semester Planning
Admin plans entire semester → Auto-selection ensures consistency → Fewer errors

### Use Case 3: Schedule Changes
Admin modifies existing timetable → Changes course → Teacher updates automatically

### Use Case 4: New Course Setup
Admin creates timetable for new course → Warning shows no teacher → Admin assigns teacher first

---

## ⚡ Performance

- **Response Time**: < 500ms (typical)
- **No Page Reload**: Smooth AJAX experience
- **Minimal Overhead**: Single database query
- **Efficient**: Uses existing course assignment data

---

## 🎓 For Developers

### API Endpoint
```
GET /api/timetable/get-teacher?courseId={id}
```

### Response Format
```json
{
  "success": true,
  "teacherId": 5,
  "teacherName": "John Doe (T12345)",
  "message": "Teacher found successfully"
}
```

### JavaScript Hook
```javascript
$('#CourseId').on('change', function() {
    // Auto-selection logic triggers here
});
```

---

## 📞 Need Help?

If you encounter issues:

1. **Check Documentation**:
   - [AUTO_SELECT_TEACHER_FEATURE.md](AUTO_SELECT_TEACHER_FEATURE.md) - Technical details
   - [AUTO_SELECT_TEACHER_TEST_GUIDE.md](AUTO_SELECT_TEACHER_TEST_GUIDE.md) - Testing guide

2. **Verify Prerequisites**:
   - Application is running
   - Logged in as Admin
   - Course assignments exist

3. **Browser Console**:
   - Press F12
   - Check for errors
   - Look at Network tab for API calls

---

## ✅ Quick Verification Checklist

Before creating timetables, ensure:

- [ ] Application is running
- [ ] Logged in as Admin user
- [ ] At least one teacher exists in system
- [ ] At least one course exists in system
- [ ] Teacher is assigned to at least one course
- [ ] Browser has JavaScript enabled
- [ ] No console errors when loading the page

---

## 🎉 Success!

You've successfully set up and tested the auto-select teacher feature! 

**Next Steps**:
1. Create timetables for all your courses
2. Verify teacher assignments are correct
3. Let teachers and students view their schedules

---

**Enjoy the streamlined timetable creation process!** 🚀
