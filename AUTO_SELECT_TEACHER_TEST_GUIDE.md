# Quick Test Guide: Auto-Select Teacher Feature

## Prerequisites
1. Build and run the application
2. Login as an Admin user
3. Ensure you have at least one course with an assigned teacher

## Test Scenario 1: Create Timetable with Assigned Teacher

**Steps**:
1. Navigate to **Admin → Manage Timetables**
2. Click **Create New Timetable**
3. Select a course that has a teacher assigned
4. **Expected Result**: 
   - Teacher dropdown automatically populates
   - Green success message appears: "✓ [Teacher Name] is assigned to this course"
   - Teacher is pre-selected in the dropdown

## Test Scenario 2: Create Timetable without Assigned Teacher

**Steps**:
1. Navigate to **Admin → Manage Timetables**
2. Click **Create New Timetable**
3. Select a course that does NOT have a teacher assigned
4. **Expected Result**: 
   - Teacher dropdown shows default "Select Teacher..." option
   - Yellow warning message appears: "⚠ No teacher is assigned to this course..."
   - You can manually select a teacher

## Test Scenario 3: Change Course Selection

**Steps**:
1. On Create Timetable page
2. Select first course (with assigned teacher)
3. Wait for auto-selection
4. Change to a different course
5. **Expected Result**: 
   - Teacher dropdown updates automatically
   - Message updates to reflect new course's teacher
   - Loading indicator briefly shows during API call

## Test Scenario 4: Edit Existing Timetable

**Steps**:
1. Navigate to **Admin → Manage Timetables**
2. Click **Edit** on an existing timetable
3. Change the course selection
4. **Expected Result**: 
   - Teacher auto-updates based on new course
   - If no teacher assigned, current selection is kept
   - Informative message shows the status

## Test Scenario 5: Manual Override

**Steps**:
1. On Create/Edit Timetable page
2. Let the system auto-select a teacher
3. Manually change the teacher to a different one
4. **Expected Result**: 
   - Manual selection is respected
   - Form can be submitted with your choice
   - No forced re-selection

## Browser Console Check

1. Open browser Developer Tools (F12)
2. Go to Console tab
3. Perform any of the above tests
4. **Expected Result**: 
   - No JavaScript errors
   - Successful AJAX calls visible (if you filter network requests)

## API Direct Test

**Test the API endpoint directly**:
```
GET http://localhost:5000/api/timetable/get-teacher?courseId=1
```

**Expected Response**:
```json
{
  "success": true,
  "teacherId": 5,
  "teacherName": "John Doe (T12345)",
  "message": "Teacher found successfully"
}
```

Or if no teacher assigned:
```json
{
  "success": false,
  "message": "No teacher is assigned to this course",
  "teacherId": null,
  "teacherName": null
}
```

## Visual Indicators

The feature uses Bootstrap Icons for feedback:
- **⏳ Hourglass**: Loading/fetching data
- **✓ Check**: Success - teacher found and selected
- **⚠ Warning**: No teacher assigned to course
- **✗ Error**: API call failed

## Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| No auto-selection happening | Check browser console for errors; ensure jQuery is loaded |
| API returns 404 | Verify application is running and API route is correct |
| Teacher not in dropdown | Ensure teacher exists and is in ViewBag.Teachers |
| Message not showing | Check that div with id="teacherMessage" exists |

## Performance Notes

- API call is lightweight (single database query)
- Uses AJAX for smooth user experience (no page reload)
- Loading indicator prevents confusion during network delay
- Debouncing not needed as course selection is deliberate user action

## Success Criteria

✅ Teacher auto-selects when course is chosen  
✅ Clear feedback messages displayed  
✅ Manual override still possible  
✅ Works on both Create and Edit pages  
✅ No JavaScript errors in console  
✅ Graceful handling of missing assignments  
✅ API returns proper JSON responses  

## Next Steps After Testing

If all tests pass:
1. Test with real data in production-like environment
2. Gather user feedback on UX
3. Consider adding similar auto-selection for other forms
4. Monitor API performance under load
