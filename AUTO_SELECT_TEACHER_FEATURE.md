# Auto-Select Teacher in Timetable Management

## Overview
This feature automatically selects the appropriate teacher when creating or editing a timetable entry based on the selected course. This streamlines the timetable creation process and ensures consistency with course assignments.

## Implementation Details

### 1. API Endpoint Created
**File**: `Controllers/API/TimetableApiController.cs`

- **Endpoint**: `GET /api/timetable/get-teacher`
- **Parameters**: 
  - `courseId` (required): The ID of the selected course
  - `sectionId` (optional): The ID of the selected section
- **Returns**: JSON with teacher information or error message

**Response Format**:
```json
{
  "success": true,
  "teacherId": 5,
  "teacherName": "John Doe (T12345)",
  "message": "Teacher found successfully"
}
```

### 2. Views Updated

#### CreateTimetable.cshtml
- Added ID `teacherSelect` to the teacher dropdown
- Added `teacherMessage` div for displaying status messages
- Added label hint: "(Auto-selected based on course)"
- Implemented JavaScript to:
  - Listen for course/section changes
  - Call API to fetch assigned teacher
  - Auto-populate teacher dropdown
  - Show informative messages (loading, success, warning)

#### EditTimetable.cshtml
- Same enhancements as CreateTimetable
- Preserves existing teacher selection if no assignment found

### 3. User Experience

**When creating a new timetable**:
1. Admin selects a course from the dropdown
2. JavaScript automatically calls the API
3. Shows "Finding assigned teacher..." message
4. If teacher is assigned:
   - Auto-selects the teacher in the dropdown
   - Shows success message: "✓ [Teacher Name] is assigned to this course"
5. If no teacher assigned:
   - Shows warning: "⚠ No teacher is assigned to this course. Please select a teacher manually."
   - Allows manual selection

**When editing an existing timetable**:
- Same auto-selection behavior
- If course changes, teacher updates automatically
- Keeps current selection if no assignment found

### 4. Technical Flow

```
User selects course
    ↓
JavaScript event listener triggers
    ↓
AJAX call to /api/timetable/get-teacher?courseId=X
    ↓
Backend queries CourseAssignments table
    ↓
Returns teacher info (if found)
    ↓
JavaScript updates dropdown and shows message
```

### 5. Database Query
The API queries the `CourseAssignments` table with:
```csharp
var courseAssignment = await _context.CourseAssignments
    .Include(ca => ca.Teacher)
    .Include(ca => ca.Course)
    .Where(ca => ca.CourseId == courseId && ca.IsActive)
    .FirstOrDefaultAsync();
```

## Benefits

1. **Efficiency**: Reduces manual work for admins
2. **Consistency**: Ensures timetables match course assignments
3. **Error Prevention**: Less chance of assigning wrong teacher
4. **User-Friendly**: Clear visual feedback during selection
5. **Flexibility**: Still allows manual override if needed

## Future Enhancements

Potential improvements:
- Section-specific teacher assignments (if same course taught by different teachers in different sections)
- Conflict detection before submission (already exists in backend)
- Bulk timetable creation with auto-teacher assignment
- Teacher workload visualization

## Testing Checklist

- [ ] Select a course with an assigned teacher - should auto-select
- [ ] Select a course without an assigned teacher - should show warning
- [ ] Change course selection - should update teacher automatically
- [ ] Verify on Edit page - should work with existing data
- [ ] Test with invalid course ID - should handle gracefully
- [ ] Check console for JavaScript errors - should be none
- [ ] Verify API response format - should match expected JSON
- [ ] Test manual teacher override - should be possible

## Files Modified/Created

### Created:
- `Controllers/API/TimetableApiController.cs` - New API controller

### Modified:
- `Views/Admin/CreateTimetable.cshtml` - Added auto-select functionality
- `Views/Admin/EditTimetable.cshtml` - Added auto-select functionality

## Notes

- The feature uses jQuery AJAX for API calls (already available in the project)
- Bootstrap Icons used for visual feedback (✓, ⚠, ⏳, ✗)
- The API is RESTful and can be expanded for other timetable operations
- The feature works seamlessly with existing validation and error handling
- No changes required to backend models or database schema
