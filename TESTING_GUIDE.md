# Quick Testing Guide - Attendance Management System

## 🎯 Quick Start Testing

### 1. Build and Run the Application

```powershell
# In VS Code, press Ctrl+Shift+B to build
# Or run from terminal:
dotnet build
dotnet run
```

The application should start at: `https://localhost:5001` or `http://localhost:5000`

---

## 🧪 Test 1: Auto-Generated Student Numbers

### Steps:
1. **Login as Admin**
   - Navigate to login page
   - Use admin credentials

2. **Create a New Student**
   - Go to: Admin → Manage Students → Create New Student
   - Fill in the form:
     - Email: `test.student1@example.com`
     - First Name: `John`
     - Last Name: `Doe`
     - Phone: `1234567890`
     - Enrollment Date: Select today's date
     - Password: `Test@123`
     - Confirm Password: `Test@123`
   - Click "Create Student"

3. **Verify Auto-Generated Number**
   - After creation, you should be redirected to student details
   - **Expected Student Number**: `STU00001` (or next in sequence)
   - **Note**: You should NOT have entered this manually!

4. **Create Another Student**
   - Repeat steps 2-3 with different email
   - **Expected Student Number**: `STU00002` (incremented)

5. **Edit Student**
   - Go to: Manage Students → Edit (for any student)
   - **Verify**: Student Number field is **read-only** and grayed out
   - **Verify**: Message shows "Student Number cannot be changed"

### ✅ Success Criteria:
- [ ] No manual StudentNumber input on create form
- [ ] Info alert shows "Student Number will be auto-generated"
- [ ] StudentNumber automatically assigned as STU00001, STU00002, etc.
- [ ] StudentNumber is read-only in edit form

---

## 🧪 Test 2: Auto-Generated Teacher Numbers

### Steps:
1. **Create a New Teacher**
   - Go to: Admin → Manage Teachers → Create New Teacher
   - Fill in the form:
     - Email: `test.teacher1@example.com`
     - First Name: `Jane`
     - Last Name: `Smith`
     - Phone: `0987654321`
     - Department: `Computer Science`
     - Hire Date: Select today's date
     - Password: `Test@123`
     - Confirm Password: `Test@123`
   - Click "Create Teacher"

2. **Verify Auto-Generated Number**
   - **Expected Teacher Number**: `TCH00001` (or next in sequence)

3. **Create Another Teacher**
   - Repeat with different email
   - **Expected Teacher Number**: `TCH00002` (incremented)

4. **Edit Teacher**
   - Go to: Manage Teachers → Edit (for any teacher)
   - **Verify**: Teacher Number field is **read-only**

### ✅ Success Criteria:
- [ ] No manual TeacherNumber input on create form
- [ ] Info alert shows "Teacher Number will be auto-generated"
- [ ] TeacherNumber automatically assigned as TCH00001, TCH00002, etc.
- [ ] TeacherNumber is read-only in edit form

---

## 🧪 Test 3: Persistent Login (JWT Token Management)

### Test 3.1: Browser Refresh
1. **Login**
   - Login with any user (admin, teacher, or student)
   - Note the current page/dashboard

2. **Refresh Browser**
   - Press F5 or click refresh
   - **Expected**: You should remain logged in
   - **Expected**: Dashboard/page should load without redirect to login

### Test 3.2: Browser Restart
1. **Login**
   - Login with any user
   - Note you are logged in

2. **Close Browser Completely**
   - Close all browser windows/tabs
   - Wait 5 seconds

3. **Reopen Browser**
   - Open browser and navigate to: `https://localhost:5001`
   - **Expected**: You should STILL be logged in
   - **Expected**: You should see your dashboard, not the login page

### Test 3.3: Developer Tools Cookie Check
1. **Login**
2. **Open Developer Tools** (F12)
3. **Go to Application → Cookies**
4. **Verify the following cookies exist**:
   - `.AspNetCore.Identity.Application` (30 days expiry)
   - `jwt` (HTTP-only, Secure)
   - `refreshToken` (HTTP-only, Secure)

5. **Check Cookie Properties**:
   - All cookies should have `HttpOnly: true`
   - All cookies should have `SameSite: Lax`
   - Cookies should have expiration dates (not session cookies)

### Test 3.4: Token Auto-Refresh
**Note**: This test requires waiting 12 hours or manually expiring the JWT.

1. **Option A - Manual Test (Advanced)**:
   - Reduce JWT expiry to 1 minute in `appsettings.json`
   - Restart application
   - Login and wait 2 minutes
   - Make a request (navigate to any page)
   - **Expected**: Middleware auto-refreshes JWT, user stays logged in

2. **Option B - Production Test**:
   - Login and use the application normally
   - After 12 hours, continue using
   - **Expected**: Auto-refresh happens transparently

### ✅ Success Criteria:
- [ ] User stays logged in after browser refresh
- [ ] User stays logged in after browser restart
- [ ] Cookies are HTTP-only and Secure
- [ ] Cookies have proper expiration dates (not session cookies)
- [ ] JWT auto-refreshes when expired (if refresh token is valid)

---

## 🧪 Test 4: Navigation

### Steps:
1. **Login as Admin**
   - Verify you can access:
     - Dashboard
     - Manage Students
     - Manage Teachers
     - Assign Courses
     - Assign Sections
     - Reports

2. **Login as Teacher**
   - Verify you can access:
     - Dashboard
     - My Courses
     - Mark Attendance
     - View Reports

3. **Login as Student**
   - Verify you can access:
     - Dashboard
     - My Courses
     - My Attendance
     - View Reports

4. **Logout**
   - Click logout from any role
   - **Expected**: Redirected to login page
   - **Expected**: Cookies are cleared
   - **Expected**: Cannot access protected pages

### ✅ Success Criteria:
- [ ] All navigation links work correctly
- [ ] Role-based menus show appropriate items
- [ ] Logout clears session and cookies
- [ ] Protected pages require authentication

---

## 🐛 Troubleshooting

### Issue: Student/Teacher Numbers Not Auto-Generating
**Solution**: 
- Check database - ensure no duplicate numbers exist
- Verify `Repositories.cs` has the auto-generation methods
- Check `AdminController.cs` calls `GenerateNextStudentNumberAsync()` / `GenerateNextTeacherNumberAsync()`

### Issue: Not Staying Logged In After Browser Restart
**Solution**:
- Check `appsettings.json` for JWT and RefreshToken settings
- Verify cookies in browser dev tools have expiration dates (not "Session")
- Check `Program.cs` has `ExpireTimeSpan = TimeSpan.FromDays(30)` for Identity
- Ensure `Middleware/JwtCookieAuthenticationMiddleware.cs` is registered in `Program.cs`

### Issue: Validation Errors on Create Forms
**Solution**:
- Ensure `CreateStudentViewModel` and `CreateTeacherViewModel` do NOT have StudentNumber/TeacherNumber properties
- Rebuild the project: `dotnet build`
- Clear browser cache and cookies

### Issue: "StudentNumber cannot be changed" message not showing
**Solution**:
- Check `EditStudent.cshtml` and `EditTeacher.cshtml` have the `readonly` attribute
- Verify the `<small class="text-muted">` message is present

---

## 📊 Expected Results Summary

### Student Creation:
```
User Input:              System Generates:
-----------             ------------------
Email                   StudentNumber: STU00001
FirstName               Password Hash
LastName                AppUser record
Phone                   Identity integration
EnrollmentDate
Password
```

### Teacher Creation:
```
User Input:              System Generates:
-----------             ------------------
Email                   TeacherNumber: TCH00001
FirstName               Password Hash
LastName                AppUser record
Phone                   Identity integration
Department
HireDate
Password
```

### JWT Flow:
```
Login → JWT (12hr) + RefreshToken (30d) → Browser Restart → Auto-Refresh → Stay Logged In
```

---

## ✅ Final Checklist

Before considering testing complete, verify:

- [ ] ✅ Students created with auto-generated STU##### format
- [ ] ✅ Teachers created with auto-generated TCH##### format
- [ ] ✅ Numbers increment correctly (STU00001, STU00002, etc.)
- [ ] ✅ Edit forms show numbers as read-only
- [ ] ✅ Login persists after browser refresh
- [ ] ✅ Login persists after browser restart
- [ ] ✅ Cookies are HTTP-only and Secure
- [ ] ✅ Navigation works for all roles
- [ ] ✅ Logout clears session properly
- [ ] ✅ No errors in browser console
- [ ] ✅ No errors in application logs

---

## 📝 Test Report Template

After testing, document results:

```
Test Date: ___________
Tester: ___________

Test 1 - Auto-Generated Student Numbers: ✅ / ❌
Notes: _______________________________________

Test 2 - Auto-Generated Teacher Numbers: ✅ / ❌
Notes: _______________________________________

Test 3.1 - Browser Refresh Login: ✅ / ❌
Notes: _______________________________________

Test 3.2 - Browser Restart Login: ✅ / ❌
Notes: _______________________________________

Test 3.3 - Cookie Verification: ✅ / ❌
Notes: _______________________________________

Test 4 - Navigation: ✅ / ❌
Notes: _______________________________________

Overall Status: ✅ All Tests Passed / ❌ Issues Found

Issues Found:
1. _______________________________________
2. _______________________________________
```

---

**Happy Testing! 🚀**

If you encounter any issues, refer to:
- `FINAL_UPDATES_SUMMARY.md` - Complete feature documentation
- `JWT_TOKEN_MANAGEMENT_GUIDE.md` - JWT technical details
