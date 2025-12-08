# Quick Reference: NEW Attendance Window Rules

## 🎯 Simple Rule
**Attendance can ONLY be marked for 10 minutes starting from lecture time**

## ⏰ Examples

### Lecture 2:00 PM - 4:00 PM
```
✅ CAN mark: 2:00 PM to 2:10 PM
❌ CANNOT mark: Before 2:00 PM
❌ CANNOT mark: After 2:10 PM
```

### Lecture 12:30 PM - 2:00 PM
```
✅ CAN mark: 12:30 PM to 12:40 PM
❌ CANNOT mark: Before 12:30 PM
❌ CANNOT mark: After 12:40 PM
```

## 📅 Creating Timetable for Testing

**IMPORTANT**: To test today, create timetable with:
- **Day**: Saturday (December 7, 2025)
- **Start Time**: Current time + 2 minutes
- **End Time**: Current time + 92 minutes
- **Course**: Your course
- **Classroom**: Any

## 🧪 Testing Steps

1. **Stop IIS Express** (Shift+F5 in Visual Studio)
2. **Restart Application** (F5)
3. **Create Timetable** for today (Saturday)
4. **Before Lecture Time**: Try marking → Should see "Available from [TIME]"
5. **At Lecture Time**: Try marking → Should load students ✅
6. **After 10 Minutes**: Try marking → Should see "Locked" 🔒

## ✅ What Changed

**OLD**: Could mark 10 min before to 10 min after lecture ENDS
**NEW**: Can ONLY mark from lecture start to 10 min after lecture STARTS

This ensures teachers mark attendance when students are actually present!
