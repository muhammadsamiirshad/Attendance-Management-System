# Database Migration Guide - RefreshTokens Table

## ⚠️ Important: Application Must Be Stopped

Before running the migration, you must stop the application because the DLL files are currently locked by IIS Express.

---

## Steps to Create and Apply Migration

### Step 1: Stop the Application

**Option A: Using Visual Studio**
1. Click the **Stop** button (red square) in the toolbar
2. Or press **Shift + F5**
3. Wait for IIS Express to fully stop

**Option B: Using Task Manager**
1. Open Task Manager (Ctrl + Shift + Esc)
2. Find "IIS Express Worker Process"
3. Right-click → End Task

**Option C: Using PowerShell**
```powershell
# Find and kill IIS Express process
Get-Process | Where-Object {$_.ProcessName -like "*iisexpress*"} | Stop-Process -Force
```

### Step 2: Create the Migration

Open PowerShell or Command Prompt in the project directory and run:

```powershell
cd "c:\Users\Administrator\Desktop\EAD\AMS\AMS"
dotnet ef migrations add AddRefreshTokensTable
```

**Expected Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

### Step 3: Review the Migration

The migration file will be created in `Migrations/` folder:
```
Migrations/
    [Timestamp]_AddRefreshTokensTable.cs
    [Timestamp]_AddRefreshTokensTable.Designer.cs
```

**Expected Migration Code:**
```csharp
public partial class AddRefreshTokensTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Token = table.Column<string>(nullable: false),
                UserId = table.Column<string>(nullable: false),
                ExpiresAt = table.Column<DateTime>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                RevokedAt = table.Column<DateTime>(nullable: true),
                IsRevoked = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_RefreshTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId",
            table: "RefreshTokens",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RefreshTokens");
    }
}
```

### Step 4: Apply the Migration to Database

```powershell
dotnet ef database update
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20240115123456_AddRefreshTokensTable'.
Done.
```

### Step 5: Verify Migration Applied

```powershell
dotnet ef migrations list
```

**Expected Output:**
```
20240115123456_AddRefreshTokensTable (Applied)
[Other previous migrations]
```

### Step 6: Verify Database Table Created

**Option A: Using SQL Server Management Studio (SSMS)**
1. Open SSMS
2. Connect to your database server
3. Expand Databases → [YourDatabase] → Tables
4. Verify `dbo.RefreshTokens` table exists

**Option B: Using SQL Query**
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'RefreshTokens'
```

**Expected Result:**
```
TABLE_NAME: RefreshTokens
TABLE_TYPE: BASE TABLE
```

### Step 7: Restart the Application

After migration is complete:
1. Press **F5** in Visual Studio to start debugging
2. Or run: `dotnet run`

---

## Alternative: One-Line Migration Command

If you prefer a single command to create and apply:

```powershell
cd "c:\Users\Administrator\Desktop\EAD\AMS\AMS"
dotnet ef migrations add AddRefreshTokensTable && dotnet ef database update
```

---

## Troubleshooting

### Error: "Build failed"

**Cause**: Application still running, DLL files locked

**Fix**:
1. Stop IIS Express completely
2. Delete `bin` and `obj` folders:
   ```powershell
   Remove-Item -Recurse -Force bin, obj
   ```
3. Rebuild:
   ```powershell
   dotnet build
   ```
4. Try migration again

### Error: "Cannot open database"

**Cause**: Database connection string issue

**Fix**:
1. Check `appsettings.json` for correct connection string
2. Ensure SQL Server is running
3. Test connection manually

### Error: "A migration has already been applied"

**Cause**: Migration already exists in database

**Fix**:
Check if table already exists:
```sql
SELECT * FROM __EFMigrationsHistory WHERE MigrationId LIKE '%RefreshToken%'
```

If exists, no action needed. If not:
```powershell
dotnet ef database update
```

### Error: "The object 'RefreshTokens' already exists"

**Cause**: Table was manually created

**Fix**:
1. Drop the table manually:
   ```sql
   DROP TABLE RefreshTokens
   ```
2. Run migration again:
   ```powershell
   dotnet ef database update
   ```

Or mark migration as applied without running:
```powershell
dotnet ef migrations script --from [LastMigrationName] --to AddRefreshTokensTable --output migration.sql
# Review the script
# If table exists, manually add to history:
# INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
# VALUES ('[Timestamp]_AddRefreshTokensTable', '8.0.0')
```

---

## Verification Checklist

After migration, verify:

### 1. Table Structure
```sql
-- Check table exists
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RefreshTokens'

-- Check columns
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'RefreshTokens'
```

**Expected Columns:**
- Id (int, NOT NULL, PRIMARY KEY, IDENTITY)
- Token (nvarchar, NOT NULL)
- UserId (nvarchar(450), NOT NULL)
- ExpiresAt (datetime2, NOT NULL)
- CreatedAt (datetime2, NOT NULL)
- RevokedAt (datetime2, NULL)
- IsRevoked (bit, NOT NULL)

### 2. Foreign Key
```sql
-- Check foreign key to AspNetUsers
SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
WHERE CONSTRAINT_NAME LIKE '%RefreshTokens%'
```

### 3. Index
```sql
-- Check index on UserId
SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID('RefreshTokens') AND name LIKE 'IX_RefreshTokens_UserId'
```

### 4. Test Insert
```sql
-- Test insert (should work)
INSERT INTO RefreshTokens (Token, UserId, ExpiresAt, CreatedAt, IsRevoked)
VALUES ('test-token', '[ValidUserId]', DATEADD(day, 7, GETDATE()), GETDATE(), 0)

-- Verify
SELECT * FROM RefreshTokens

-- Clean up test
DELETE FROM RefreshTokens WHERE Token = 'test-token'
```

---

## Post-Migration Testing

### Test JWT Authentication Flow

1. **Login and Get Token**
   ```powershell
   # PowerShell
   $body = @{
       email = "teacher@test.com"
       password = "Test@123"
   } | ConvertTo-Json

   $response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
       -Method POST `
       -Body $body `
       -ContentType "application/json"

   $token = $response.accessToken
   $refreshToken = $response.refreshToken
   ```

2. **Verify RefreshToken Saved in Database**
   ```sql
   SELECT * FROM RefreshTokens 
   WHERE UserId = '[YourUserId]'
   ORDER BY CreatedAt DESC
   ```

   Should show the newly created refresh token.

3. **Use Refresh Token**
   ```powershell
   $body = @{
       refreshToken = $refreshToken
   } | ConvertTo-Json

   $response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/refresh" `
       -Method POST `
       -Body $body `
       -ContentType "application/json"
   ```

   Should return new access token.

4. **Verify Old Token Revoked**
   ```sql
   SELECT * FROM RefreshTokens 
   WHERE Token = '[OldRefreshToken]'
   ```

   Should show `IsRevoked = 1` and `RevokedAt` set.

---

## Migration Rollback (If Needed)

If you need to undo the migration:

```powershell
# Rollback last migration
dotnet ef database update [PreviousMigrationName]

# Remove migration file
dotnet ef migrations remove
```

**Warning**: This will delete the RefreshTokens table and all data in it!

---

## Summary

✅ **Before Migration**:
- Stop IIS Express / Application
- Backup database (optional but recommended)

✅ **Migration Steps**:
1. `dotnet ef migrations add AddRefreshTokensTable`
2. Review generated migration file
3. `dotnet ef database update`
4. Verify table created

✅ **After Migration**:
- Restart application
- Test JWT login flow
- Verify refresh tokens saved
- Check database for RefreshTokens table

---

## Quick Commands Reference

```powershell
# Stop application (PowerShell)
Get-Process | Where-Object {$_.ProcessName -like "*iisexpress*"} | Stop-Process -Force

# Navigate to project
cd "c:\Users\Administrator\Desktop\EAD\AMS\AMS"

# Create migration
dotnet ef migrations add AddRefreshTokensTable

# Apply migration
dotnet ef database update

# List migrations
dotnet ef migrations list

# Check if applied
dotnet ef migrations list | Select-String "AddRefreshTokensTable"

# Restart application
dotnet run
```

---

## Need Help?

If you encounter issues:

1. **Check Error Message**: Read carefully
2. **Verify Prerequisites**:
   - Application stopped
   - Database accessible
   - EF Core tools installed
3. **Search Error**: Google the specific error message
4. **Check Logs**: Review application logs
5. **Ask for Help**: Provide error details

---

*Last Updated: January 2024*
*Status: Ready to Execute*
