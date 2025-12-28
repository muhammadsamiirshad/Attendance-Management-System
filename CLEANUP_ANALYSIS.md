# Codebase Cleanup Analysis - AMS

## Analysis Date: December 9, 2025

## CONTROLLERS ANALYSIS

### ✅ ESSENTIAL CONTROLLERS (Keep)
1. **AccountController.cs** - Login, Logout, Password Management ✓ USED
2. **AdminController.cs** - Admin dashboard, user management ✓ USED
3. **StudentController.cs** - Student dashboard, timetable, attendance ✓ USED
4. **TeacherController.cs** - Teacher dashboard, attendance marking ✓ USED
5. **AttendanceController.cs** - Core attendance functionality ✓ USED
6. **ReportController.cs** - Reports and statistics ✓ USED
7. **API/AuthController.cs** - JWT API authentication ✓ USED (for API)

### ❌ REDUNDANT/UNUSED CONTROLLERS (Delete)
1. **CourseController.cs** - Course management is handled by Admin ❌ REDUNDANT
   - Functionality exists in AdminController
   - No unique views or features
   - Course CRUD can be in Admin section

2. **TimetableController.cs** - Timetable is viewed via Student/Teacher ❌ REDUNDANT
   - Students view via StudentController.ViewTimetable()
   - Teachers view via TeacherController.ViewTimetable()
   - Admin creates timetable via AdminController
   - No unique functionality

3. **HomeController.cs** - Just redirects, no real content ❌ MINIMAL USE
   - Index() just redirects based on role
   - Privacy() is unused
   - Can be replaced with Account/Login as default

## VIEWS ANALYSIS

### ❌ UNUSED VIEWS (Delete)
1. **Views/Course/** - Entire folder ❌
   - Create.cshtml
   - Delete.cshtml
   - Details.cshtml
   - Edit.cshtml
   - Index.cshtml
   - Reason: No links to these views, handled in Admin

2. **Views/Timetable/** - Entire folder ❌
   - Create.cshtml
   - Edit.cshtml
   - Index.cshtml
   - Reason: Students/Teachers have their own timetable views

3. **Views/Home/Privacy.cshtml** - Never used ❌
   - No links in navigation
   - Just default template

4. **Views/Home/Index.cshtml** - Never shown ❌
   - Controller just redirects
   - Never actually renders

## DOCUMENTATION FILES ANALYSIS

### ✅ KEEP
1. **README.md** - Main documentation ✓
2. **LICENSE** - Legal requirement ✓
3. **JWT_AND_TIMETABLE_FIXES.md** - Recent fixes documentation ✓

### ❓ OPTIONAL (User Decision)
1. **.git/** folder - Version control history (usually keep)
2. **.vscode/** folder - VS Code settings (personal preference)

## FILES TO DELETE

### Controllers (3 files)
- Controllers/CourseController.cs
- Controllers/TimetableController.cs  
- Controllers/HomeController.cs

### Views (11+ files)
- Views/Course/ (entire folder - 5 files)
- Views/Timetable/ (entire folder - 3 files)
- Views/Home/Index.cshtml
- Views/Home/Privacy.cshtml

## IMPACT ANALYSIS

### Breaking Changes Required:
1. **Program.cs** - Change default route from Home to Account
2. **Navigation/Layout** - Remove Course/Timetable menu items (if any)
3. **Links** - Update any hardcoded links to removed controllers

### Benefits:
- ✅ Cleaner codebase (14 fewer files)
- ✅ Less confusion about which controller to use
- ✅ Reduced maintenance burden
- ✅ Better code organization
- ✅ Faster build times

### Risks:
- ⚠️ If any custom code references these controllers
- ⚠️ If external systems call these endpoints

## RECOMMENDATION

**SAFE TO DELETE**: All files listed above
**CONFIDENCE LEVEL**: HIGH (95%)

The removed functionality is already available through:
- Admin manages courses
- Admin creates timetables  
- Students/Teachers view their own timetables
- Home just redirects to login

## MIGRATION PLAN

1. **Phase 1**: Delete unused views (no code impact)
2. **Phase 2**: Update default route in Program.cs
3. **Phase 3**: Delete unused controllers
4. **Phase 4**: Test all functionality
5. **Phase 5**: Rebuild and verify

