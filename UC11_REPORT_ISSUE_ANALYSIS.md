# UC11 - Report Issue: Complete Use Case Analysis

**Use Case ID:** UC11  
**Use Case Name:** Report Issue  
**Created By:** LocHD  
**Date Created:** 14/01/2026  
**Primary Actor:** Student  
**Secondary Actors:** Manager  

---

## 1. USE CASE OVERVIEW

### Summary
Allows a Student to submit a formal complaint regarding a specific past order - such as missing items, hygiene issues, or food not matching the description - so that Manager can receive issue notification.

### Dependency
None

---

## 2. PRECONDITIONS

Before the use case can begin, the following conditions must be satisfied:

1. **Student Authentication:** The Student is logged into the Campus Food System
2. **Order History:** The Student has at least one order with a status of "Completed"
3. **Reporting Window Validity:** The time elapsed since the order completion has not exceeded the allowable reporting window (e.g., 24 hours)

---

## 3. POSTCONDITIONS

After successful completion of the use case:

1. **Issue Record Creation:** A new Issue Record is created in the database linked to the specific Order ID
2. **Initial Status:** The status of the issue is set to "Pending Review"
3. **Manager Notification:** The Manager has received an alert/email regarding the issue

---

## 4. MAIN FLOW SEQUENCE

### Step-by-Step Workflow

| Step | Actor | Action | System Response | Input Data | Output Data |
|------|-------|--------|-----------------|-----------|------------|
| 1 | Student | Navigates to "Order History" section | System loads order history page | Student ID (authenticated) | Order history page displayed |
| 2 | System | - | Displays list of Customer's past orders with status "Completed" | Student ID | List of completed orders (orderId, items, price, date, status) |
| 3 | Student | Selects specific order from list | System retrieves order details | Selected orderId | Order selected (highlighted) |
| 4 | System | - | Displays detailed view of selected order | orderId | Order details: items, price, order date, completion date, total amount, status |
| 5 | Student | Navigates to "Report Issue" page (from order detail view) | System validates reporting time window | orderId, current timestamp | Issue Reporting Form displayed OR error message |
| 6 | System | - | Displays Issue Reporting Form requesting required fields | orderId | Form with fields: Issue Type dropdown, Description text area, Photo Upload button |
| 7 | Student | Fills form: selects issue type and enters description | System captures form data | Issue Type (string), Description (string), orderId | Form data stored in memory (not yet persisted) |
| 8 | Student | Clicks "Submit" button | System receives submission request | Form data (Issue Type, Description, orderId) | Submission event triggered |
| 9 | System | - | Validates all required fields are filled and have valid data | Issue Type, Description | Validation result: pass/fail with error details if applicable |
| 10 | System | - | Creates Issue record in database with status "Pending Review" and persists to DB | orderId, Issue Type, Description, current timestamp | Issue created: issueId, status="Pending Review", createdDate |
| 11 | System | - | Sends notification to Manager with issue details | issueId, orderId | Notification created, Manager alerted via email/in-app notification |

### Main Flow Data Model

**Input to Main Flow:**
- Student ID (from authentication)
- Order ID (selected by student)
- Issue Type (selected from dropdown)
- Issue Description (text input)
- Current Date/Time

**Output from Main Flow:**
- New Issue Record (issueId, orderId, status, createdDate)
- Notification to Manager (notificationId, issueId, message)

---

## 5. ALTERNATIVE FLOWS

### Alternative Flow 1: Report with Photo Evidence
**Branches from Step 7 (After issue type and description entry)**

| Step | Actor | Action | System Response | Input Data | Output Data |
|------|-------|--------|-----------------|-----------|------------|
| Alt 1.1 | Student | Chooses to attach a photo to report | System enables photo upload | - | Photo upload dialog opens |
| Alt 1.2 | Student | Selects and uploads image file | System receives image file | Image File (JPG/PNG format, max size TBD) | Image displayed for preview |
| Alt 1.3 | System | - | Uploads image to storage and associates with form | Image File, issueId | Image saved: imagePath returned |
| Alt 1.4 | Flow rejoins | Main sequence continues at Step 8 | - | - | - |

**Alternative Photo Upload Details:**
- **Supported Formats:** JPG, PNG
- **File Validation:** System validates file format before processing
- **Storage:** Image is saved to file storage system
- **Association:** Image path is linked to issue record during creation

---

## 6. EXCEPTION FLOWS

### Exception 1: Reporting Period Expired
**Triggered at Step 5 (During navigation to Report Issue page)**

| Step | Condition | System Response | Output |
|------|-----------|-----------------|--------|
| Exc 1.1 | Order age > 24 hours (or configured reporting window) | System checks ordering completion timestamp against current time | Evaluation: expired or valid |
| Exc 1.2 | If order is EXPIRED | Display "Expired" error message preventing access to report page | Error dialog: "This order is older than 24 hours. Reporting window has expired." |
| Exc 1.3 | - | Terminate use case, return to Order History | Use case ends, no issue created |

**Reporting Window Policy:**
- **Default Window:** 24 hours from order completion
- **Validation Point:** Step 5, before displaying report form
- **Action on Expiration:** Block reporting, display warning, prevent form submission

### Exception 2: Invalid Form Data
**Triggered at Step 9 (Validation phase)**

| Field | Validation Rule | Error Message | Action |
|-------|-----------------|----------------|--------|
| Issue Type | Must be selected (not null) | "Please select an issue type" | Highlight field, prevent submission |
| Description | Must not be empty, min length TBD | "Description is required (minimum X characters)" | Highlight field, prevent submission |
| Multiple Errors | Any required field empty | "Please fill all required fields" | Display all errors, prevent submission |

**Validation Error Handling:**
- User is returned to form with error messages
- Form data is retained (not cleared)
- User can correct and resubmit

### Exception 3: File Upload Error (During Photo Upload)
**Triggered during Alt Flow 1**

| Scenario | Error Condition | System Response | Recovery |
|----------|-----------------|-----------------|----------|
| Invalid Format | File is not JPG or PNG | "Invalid file format. Please upload JPG or PNG files only." | Allow user to reselect file |
| Oversized File | File exceeds size limit | "File is too large. Maximum size is X MB." | Allow user to reselect smaller file |
| Upload Failure | Server error during file save | "Failed to upload image. Please try again." | Show retry button, allow file reselection |

**Resolution:** User can attempt upload again or continue without image (if image is optional)

### Exception 4: Database Error During Issue Creation
**Triggered at Step 10**

| Component | Error Type | System Response | Recovery |
|-----------|-----------|-----------------|----------|
| Issue Table Insert | Database constraint violation or connection error | Show: "An error occurred while creating the issue. Please try again." | Log error, allow user to retry |
| Notification Creation | Notification insert fails | Issue created but manager not notified (log error) | System attempts async notification retry |

**Recovery Strategy:**
- Transaction rollback if both issue and notification fail together
- User-friendly error message
- Option to retry submission

---

## 7. BUSINESS RULES AND VALIDATIONS

### Business Rules

1. **Reporting Window Rule:**
   - An issue can only be reported within 24 hours of order completion
   - Orders older than 24 hours cannot be reported

2. **Issue-Order Relationship:**
   - Each issue must be linked to exactly one order
   - An order must exist and be completed before an issue can be reported

3. **Issue Status Lifecycle:**
   - Initial status: "Open" or "Pending Review"
   - Possible states: Open → In Progress → Resolved → Closed
   - Issues cannot be reported for cancelled or pending orders

4. **Manager Notification:**
   - Manager must be notified immediately upon issue creation
   - Notification will include issue ID, order ID, and issue description
   - Notification is mandatory for all issue submissions

5. **Photo Evidence (If Provided):**
   - Photos are optional unless specified otherwise
   - Photos must be in JPG or PNG format
   - One photo per issue (or TBD limitation)

### Data Validation Rules

| Field | Validation Rule | Type | Required |
|-------|-----------------|------|----------|
| orderId | Must exist in Orders table with status "Completed" | Integer > 0 | Yes |
| Issue Type | Must be from predefined list (enum) | String | Yes |
| Description | Non-empty, min length TBD (suggest 10-500 chars) | String | Yes |
| Photo File | Must be JPG/PNG format, max size TBD | File | No |
| timestamp | Must be within reporting window (≤24 hours from completion) | DateTime | Yes |

### Issue Type Categories
- Missing Items
- Hygiene Issues
- Food Quality/Taste Mismatch
- Incorrect Order
- Damaged Packaging
- Other (with free-text description)

---

## 8. ERROR HANDLING SCENARIOS

### Scenario 1: Student Tries to Report on Expired Order
```
START: Student selects order completed 25 hours ago
STEP 5: System validation triggered
CONDITION: (currentTime - orderCompletionTime) > 24 hours
ACTION: Return error "Reporting window expired"
RESULT: Form not displayed, use case terminates
RECOVERY: User can check order history for other valid orders
```

### Scenario 2: Form Submission with Missing Description
```
START: Student fills Issue Type but leaves Description empty, clicks Submit
STEP 9: Validation triggered
CONDITION: Description field is null or empty
ACTION: Return validation error "Description is required"
RESULT: Form retained with error message, user can edit
RECOVERY: User enters description and resubmits
```

### Scenario 3: Photo Upload with Unsupported Format
```
START: Student uploads .gif file as evidence
STEP Alt 1.3: File validation triggered
CONDITION: File extension not in (jpg, png)
ACTION: Return error "Invalid format. JPG/PNG only."
RESULT: Upload rejected, form continues without image
RECOVERY: User can retry with correct format or skip image
```

### Scenario 4: Network Failure During Issue Persistence
```
START: Form validation passes, system attempts to save issue
STEP 10: Database insert operation fails
CONDITION: Connection timeout or DB error
ACTION: Catch exception, rollback transaction, show error
RESULT: Issue not created, user sees "Try again" option
RECOVERY: User clicks retry, issue creation attempted again
```

### Scenario 5: Manager Notification Delivery Failure
```
START: Issue created successfully, notification service called
STEP 11: Email/notification service fails
CONDITION: Email service unreachable or failure
ACTION: Log error, attempt async retry, notify user of partial success
RESULT: Issue created but manager not immediately notified
RECOVERY: System has async retry mechanism, issue tracked for manual follow-up
```

---

## 9. ACTOR INTERACTIONS

### Student Interactions
1. **Authentication:** Must be logged in (prerequisite)
2. **Navigation:** Accesses Order History and Report Issue pages
3. **Selection:** Chooses a completed order from list
4. **Form Input:** Selects issue type and enters description
5. **Optional Action:** Uploads photo evidence
6. **Submission:** Submits the report
7. **Feedback:** Receives confirmation or error messages

### System Interactions
1. **Validation:** Checks preconditions and form data validity
2. **Persistence:** Creates issue and notification records in database
3. **Notification:** Sends alert to Manager
4. **Storage:** (If photo) Saves image to file system
5. **Error Handling:** Manages exceptions and provides feedback

### Manager Interactions (Secondary)
1. **Notification Receipt:** Receives alert about new issue (asynchronous)
2. **Issue Awareness:** Can view issue details from notification
3. **Not Direct:** Manager doesn't directly interact in this use case flow; receives notification as post-condition

---

## 10. DETAILED DESIGN CLASSES

### Class 1: StudentCoordinator (Control Class)
**Purpose:** Orchestrates the issue reporting flow

**Attributes:**
- `managerInteraction`: Reference to manager notification service

**Operations:**

#### Operation 1: submitIssueRequest
```csharp
public void SubmitIssueRequest(int orderId, string details)
{
    // 1. Find and validate order
    Order order = FindOrderById(orderId);
    if (order == null)
        throw new OrderNotFoundException();
    
    // 2. Validate reporting window
    if (IsReportingWindowExpired(order.CompletionDate))
        throw new ReportingWindowExpiredException();
    
    // 3. Create issue
    Issue newIssue = new Issue();
    newIssue.CreateIssue(orderId, details);
    
    // 4. Notify manager
    managerInteraction.IssueNotification(newIssue.IssueId);
}
```

**Input:** orderId (int), details (string)  
**Output:** None (void)  
**Preconditions:** Order exists and is completed, within reporting window  
**Postconditions:** Issue created, manager notified  
**Operations Used:** Issue.CreateIssue(), ManagerInteraction.IssueNotification()

#### Operation 2: uploadImageFile
```csharp
public void UploadImageFile(int issueId, IFormFile file)
{
    // 1. Validate issue exists
    Issue issue = FindIssueById(issueId);
    if (issue == null)
        throw new IssueNotFoundException();
    
    // 2. Validate file format
    if (!IsValidImageFormat(file))
        throw new InvalidFileFormatException();
    
    // 3. Save file
    string imagePath = SaveFileToStorage(file);
    
    // 4. Update issue with image path
    issue.UpdateIssue(issue.Status, imagePath);
}
```

**Input:** issueId (int), file (IFormFile/File object)  
**Output:** None (void)  
**Preconditions:** Issue exists, file is valid image format  
**Postconditions:** Image saved, issue record updated with image path  
**Operations Used:** Issue.UpdateIssue()

---

### Class 2: Issue (Data Abstraction Class)
**Purpose:** Encapsulates issue data and operations

**Attributes:**
- `issueId`: Integer (Primary Key)
- `orderId`: Integer (Foreign Key to Order)
- `details`: String (Issue description)
- `imagePath`: String (Path to uploaded evidence image, nullable)
- `status`: String (Open, In Progress, Resolved, Closed)
- `createdDate`: DateTime

**Operations:**

#### Operation 1: CreateIssue
```csharp
public void CreateIssue(int targetOrderId, string issueDetails)
{
    orderId = targetOrderId;
    details = issueDetails;
    status = "Open";
    createdDate = DateTime.Now;
    // Issue is then inserted into database
}
```

**Input:** targetOrderId (int), issueDetails (string)  
**Output:** None  
**Preconditions:** Order exists  
**Postconditions:** Issue initialized with Open status  

#### Operation 2: ReadIssue
```csharp
public string ReadIssue()
{
    return $"Status: {status}, Details: {details}";
}
```

**Input:** None  
**Output:** String (issue information)  
**Preconditions:** Issue exists  
**Postconditions:** Issue details returned  

#### Operation 3: UpdateIssue
```csharp
public void UpdateIssue(string newStatus, string newImagePath)
{
    if (newStatus != null)
        status = newStatus;
    
    if (newImagePath != null)
        imagePath = newImagePath;
}
```

**Input:** newStatus (string, nullable), newImagePath (string, nullable)  
**Output:** None  
**Preconditions:** Issue exists  
**Postconditions:** Issue record updated  

#### Operation 4: DeleteIssue
```csharp
public void DeleteIssue()
{
    status = "Deleted";
    // Issue is marked as deleted, not permanently removed
}
```

**Input:** None  
**Output:** None  
**Preconditions:** Issue exists  
**Postconditions:** Issue no longer active  

---

### Class 3: Order (Data Abstraction Class)
**Purpose:** Encapsulates order data (referenced by issues)

**Attributes:**
- `orderId`: Integer (Primary Key)
- `studentId`: Integer (Foreign Key to Student)
- `orderDate`: DateTime
- `completionDate`: DateTime
- `totalAmount`: Float
- `status`: String (Pending, Processing, Completed, Cancelled)

**Operations:**

#### Operation 1: CreateOrder
```csharp
public void CreateOrder(int targetStudentId, float initialAmount)
{
    studentId = targetStudentId;
    totalAmount = initialAmount;
    status = "Pending";
    orderDate = DateTime.Now;
}
```

#### Operation 2: ReadOrder
```csharp
public string ReadOrder()
{
    return $"Status: {status}, Total Amount: {totalAmount}";
}
```

#### Operation 3: UpdateOrder
```csharp
public void UpdateOrder(string newStatus)
{
    if (newStatus != null)
        status = newStatus;
    
    if (newStatus == "Completed")
        completionDate = DateTime.Now;
}
```

#### Operation 4: DeleteOrder
```csharp
public void DeleteOrder()
{
    status = "Cancelled";
}
```

**Key Method for Validation:**
```csharp
public bool IsWithinReportingWindow(int hoursWindow = 24)
{
    TimeSpan elapsed = DateTime.Now - completionDate;
    return elapsed.TotalHours <= hoursWindow;
}
```

---

### Class 4: Notification (Data Abstraction Class)
**Purpose:** Encapsulates notification data sent to Manager

**Attributes:**
- `notificationId`: Integer (Primary Key)
- `issueId`: Integer (Foreign Key to Issue)
- `message`: String (Notification content)
- `isRead`: Boolean (Default: false)
- `createdDate`: DateTime

**Operations:**

#### Operation 1: CreateNotification
```csharp
public void CreateNotification(int targetIssueId, string msg)
{
    issueId = targetIssueId;
    message = msg;
    isRead = false;
    createdDate = DateTime.Now;
}
```

**Input:** targetIssueId (int), msg (string)  
**Output:** None  
**Preconditions:** Issue exists  
**Postconditions:** Notification initialized with unread status  

#### Operation 2: ReadNotification
```csharp
public string ReadNotification()
{
    return $"Message: {message}, Read Status: {isRead}";
}
```

#### Operation 3: UpdateNotification
```csharp
public void UpdateNotification(bool readStatus)
{
    if (readStatus != null)
        isRead = readStatus;
}
```

#### Operation 4: DeleteNotification
```csharp
public void DeleteNotification()
{
    // Logic to permanently remove notification from database
}
```

---

## 11. DATABASE SCHEMA

### Table 1: Issues
```sql
CREATE TABLE Issues (
    IssueId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    Details NVARCHAR(MAX) NOT NULL,
    ImagePath NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME NULL
);

-- Index for foreign key lookups
CREATE INDEX IX_Issues_OrderId ON Issues(OrderId);
CREATE INDEX IX_Issues_Status ON Issues(Status);
CREATE INDEX IX_Issues_CreatedDate ON Issues(CreatedDate);
```

**Constraints:**
- `IssueId` is the single primary key (each issue is unique)
- `OrderId` is a foreign key linking to Orders table
- `Status` is limited to: Open, In Progress, Resolved, Closed, Deleted
- `Details` is a text field for issue description
- `ImagePath` is optional (nullable) for photo evidence
- `CreatedDate` captures when issue was reported

---

### Table 2: Orders
```sql
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL FOREIGN KEY REFERENCES Students(StudentId),
    OrderDate DATETIME NOT NULL,
    CompletionDate DATETIME NULL,
    TotalAmount FLOAT NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
);

-- Index for lookups
CREATE INDEX IX_Orders_StudentId ON Orders(StudentId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CompletionDate ON Orders(CompletionDate);
```

**Constraints:**
- `OrderId` is primary key
- `StudentId` foreign key to Students table
- `Status` limited to: Pending, Processing, Completed, Cancelled
- `CompletionDate` is used to validate reporting window

---

### Table 3: Notifications
```sql
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    IssueId INT NOT NULL FOREIGN KEY REFERENCES Issues(IssueId),
    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ManagerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId)
);

-- Index for manager lookups
CREATE INDEX IX_Notifications_ManagerId ON Notifications(ManagerId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedDate ON Notifications(CreatedDate);
```

**Constraints:**
- `NotificationId` is primary key
- `IssueId` foreign key to Issues table
- `IsRead` tracks whether manager has viewed notification
- `ManagerId` indicates which manager receives notification
- One-to-many relationship: One Issue can generate one Notification

---

## 12. NONFUNCTIONAL REQUIREMENTS

### Usability
1. **Photo Upload:** Must support common formats (JPG, PNG)
2. **Form Design:** Issue reporting form must be intuitive and user-friendly
3. **Error Messages:** Clear, actionable error messages for validation failures
4. **Confirmation:** Success message after issue submission

### Availability
1. **24/7 Access:** Reporting function must be available 24/7, even if cafeteria is closed
2. **Valid Window:** As long as the reporting window is valid (within 24 hours)
3. **System Reliability:** High uptime SLA for issue submission service

### Performance
1. **Response Time:** Form submission should complete within 2-3 seconds
2. **Notification Delivery:** Manager notification should be sent within 5 minutes
3. **Photo Upload:** Photo upload completion within reasonable time (suggest < 30 seconds)

### Security
1. **Authentication:** Student must be authenticated to report issues
2. **Authorization:** Students can only report on their own orders
3. **Data Protection:** Sensitive issue and notification data must be encrypted
4. **File Validation:** Uploaded images must be scanned for malware

### Scalability
1. **Concurrent Users:** System should handle concurrent issue submissions
2. **Database Growth:** Schema designed for large number of issues over time
3. **File Storage:** Scalable solution needed for image storage

---

## 13. OUTSTANDING QUESTIONS/ASSUMPTIONS

### Clarifications Needed
1. **Reporting Window:** Is 24 hours the fixed window? Can it be configured per institution?
2. **Photo Limitations:** Can students upload multiple photos? File size limit?
3. **Issue Type List:** Complete list of predefined issue types?
4. **Manager Assignment:** How are managers assigned to receive notifications?
5. **Follow-up Communication:** Can students follow up on their issue report?
6. **Issue Priority:** Should issues have priority levels (High/Medium/Low)?
7. **SLA:** Service level agreement for manager response time?

### Current Assumptions
- Reporting window: 24 hours from order completion
- One photo per issue maximum
- Photo formats: JPG, PNG only
- All issues start with "Open" status
- Manager receives notification immediately (with async retry)
- Students can view their submitted issues in order history
- No direct communication channel between student and manager in this UC

---

## 14. IMPLEMENTATION MAPPING FOR C# CODE

### Coordinator Implementation: ReportIssueCoordinator
**Location:** `Coordinators/ReportIssueCoordinator.cs`

**Key Methods:**
```csharp
public class ReportIssueCoordinator
{
    private readonly IOrderService _orderService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    
    // Main entry point from controller
    public ReportIssueResult SubmitIssueReport(int studentId, int orderId, 
                                               string issueType, string description, 
                                               IFormFile? photoFile)
    {
        // Validate preconditions
        // Create issue
        // Upload photo if provided
        // Notify manager
        // Return result
    }
    
    // Validate reporting window
    private bool ValidateReportingWindow(Order order)
    {
        return order.IsWithinReportingWindow(24);
    }
}
```

### Service Implementation: IssueService
**Location:** `Services/IssueService.cs`

**Key Methods:**
```csharp
public interface IIssueService
{
    Task<Issue> CreateIssueAsync(int orderId, string details, string issueType);
    Task<Issue> GetIssueByIdAsync(int issueId);
    Task<IEnumerable<Issue>> GetIssuesByOrderAsync(int orderId);
    Task UpdateIssueAsync(int issueId, string status, string imagePath);
}
```

### Repository Implementation: IissueRepository
**Location:** `Data/IIssueRepository.cs` and implementation

```csharp
public interface IIssueRepository
{
    Task AddIssueAsync(Issue issue);
    Task<Issue> GetByIdAsync(int issueId);
    Task<IEnumerable<Issue>> GetByOrderIdAsync(int orderId);
    Task UpdateAsync(Issue issue);
}
```

### Model Implementation: Issue Entity
**Location:** `Models/Issue.cs`

```csharp
public class Issue
{
    public int IssueId { get; set; }
    public int OrderId { get; set; }
    public string Details { get; set; }
    public string IssueType { get; set; }
    public string ImagePath { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    
    // Navigation properties
    public Order Order { get; set; }
    public ICollection<Notification> Notifications { get; set; }
}
```

### Controller Implementation: IssueReportController
**Location:** `Controllers/IssueReportController.cs`

```csharp
[HttpPost("report")]
public async Task<IActionResult> ReportIssue([FromForm] ReportIssueRequest request)
{
    // Use ReportIssueCoordinator to process
    var result = await _reportIssueCoordinator.SubmitIssueReportAsync(
        User.GetStudentId(),
        request.OrderId,
        request.IssueType,
        request.Description,
        request.PhotoFile
    );
    
    if (result.Success)
        return Ok(result);
    else
        return BadRequest(result);
}
```

---

## 15. TESTING STRATEGY

### Unit Test Cases

#### Test 1: Valid Issue Submission
```
Input: Valid orderId, issueType, description
Expected: Issue created, status="Open", notification sent
Assertions: IssueId > 0, Status == "Open", NotificationSent == true
```

#### Test 2: Expired Reporting Window
```
Input: OrderId with completionDate > 24 hours ago
Expected: ReportingWindowExpiredException thrown
Assertions: Exception type, error message contains "24 hours" or "expired"
```

#### Test 3: Missing Description
```
Input: orderId, issueType, description=null/empty
Expected: Validation error
Assertions: ValidationException thrown, error includes "Description"
```

#### Test 4: Invalid Photo Format
```
Input: Valid issue data, photo file with .txt extension
Expected: File validation error
Assertions: InvalidFileFormatException thrown
```

#### Test 5: Photo Upload Success
```
Input: Valid issue data, valid JPG photo
Expected: Issue created, photo saved, imagePath stored
Assertions: Issue.ImagePath is not null, file exists in storage
```

### Integration Test Cases
1. Complete flow: Order → Issue Report → Notification sent
2. Database persistence: Issue record created and retrievable
3. Manager notification delivery
4. File storage and retrieval

---

## SUMMARY

This use case **UC11 - Report Issue** is a critical feature in the Campus Food system that allows students to report quality issues with orders. The implementation requires:

1. **Three main entities:** Order, Issue, Notification
2. **Coordinator pattern:** ReportIssueCoordinator orchestrates the flow
3. **Validation:** Reporting window check (24 hours)
4. **Persistence:** Issue and Notification creation in database
5. **File handling:** Optional photo attachment with format validation
6. **Notification:** Manager alert upon issue creation
7. **Error handling:** Comprehensive exception handling for all failure scenarios

The architecture follows object-oriented design with clear separation between control (Coordinator), data (Entities), and persistence (Repository/Services) layers, making it maintainable and testable.
