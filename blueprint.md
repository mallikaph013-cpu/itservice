
# Blueprint: IT Service Request App

## 1. Purpose & Capabilities

This application is an internal tool for the IT department to manage service requests from various departments. It allows IT staff to track the status of each request, assign tasks, and manage user permissions for accessing different parts of the application. The system has been updated to support a more granular, user-level permission model instead of a role-based one.

---

## 2. Project Outline & Design

### 2.1. Core Features

*   **User Authentication:** Users can log in with predefined credentials.
*   **User-Based Access Control:** The application uses a user-based system to control access to different menus and features, allowing for fine-grained control over individual user permissions.
*   **Permission Management:** A dedicated page for Admins to manage menu access permissions for each user individually, with granular control (Read, Create, Edit, Delete).
*   **Navigation Menu:** A dynamic navigation bar that displays menu items based on the logged-in user's specific permissions.
*   **IT Support Request Tracking:** A feature for creating, viewing, and managing IT support requests. This includes a work queue for IT staff and an approval workflow.
*   **Approval Workflow Management:** Admins can define multi-step approval sequences for different departments.

### 2.2. Technology Stack

*   **Backend:** ASP.NET Core, C#, Entity Framework Core
*   **Frontend:** Razor Pages, Tailwind CSS
*   **Database:** SQLite (for development)

### 2.3. Project Structure

*   **/Controllers:** Handles incoming HTTP requests and application logic (e.g., `UserMenuController`, `ITSupportController`).
*   **/Data:** Contains the `ApplicationDbContext`, migrations, and data seeding logic.
*   **/Models:** Defines the data structures (entities) of the application (e.g., `User`, `Menu`, `UserMenuPermission`, `SupportRequest`).
*   **/ViewModels:** Provides data shapes specifically for the views (e.g., `UserMenuViewModel`).
*   **/Services:** Contains business logic services (e.g., `NavigationService`).
*   **/Views:** Contains the Razor templates for the UI.
*   **/wwwroot:** Static assets (CSS, JS, images).

### 2.4. Visual Design

*   **Layout:** A clean, professional layout using Tailwind CSS.
*   **Navigation:** A top navigation bar for primary menus.
*   **Tables & Forms:** Styled for clarity and ease of use, providing a modern and intuitive user experience.
*   **Permissions UI:** Permissions are managed on a per-user basis, with a clear table layout showing each user and their corresponding access rights for each menu item.

---

## 3. Current Task: Refactor Permissions from Role-Based to User-Based

### 3.1. Goal

To overhaul the application's access control system, moving from a role-based model to a more flexible and granular user-based permission model. This allows administrators to assign permissions directly to individual users.

### 3.2. Implementation Steps

1.  **Create `User` Model:**
    *   Defined a `User.cs` model with properties like `Id`, `Username`, `PasswordHash`, `FirstName`, `LastName`, `Role`.
2.  **Create `UserMenuPermission` Model:**
    *   Created a new `UserMenuPermission.cs` model to link `User` and `Menu` entities directly.
    *   It includes boolean properties for `CanRead`, `CanCreate`, `CanEdit`, `CanDelete`.
3.  **Delete Old `RoleMenuPermission` Model:**
    *   Removed the `RoleMenuPermission.cs` file as it is no longer needed.
4.  **Update `DbContext`:**
    *   Added `DbSet<User>` and `DbSet<UserMenuPermission>`.
    *   Removed `DbSet<RoleMenuPermission>`.
5.  **Refactor Controller:**
    *   Renamed `RoleMenuController.cs` to `UserMenuController.cs`.
    *   Updated the controller's logic to fetch users and their specific permissions.
6.  **Create New ViewModel:**
    *   Created `UserMenuViewModel.cs` to structure the data needed for the new permission management view, including lists of users, menus, and the permissions dictionary.
7.  **Update Views:**
    *   Renamed the `Views/RoleMenu` directory to `Views/UserMenu`.
    *   Rewrote the `Index.cshtml` view to display a table for each user, allowing administrators to set permissions for each menu item.
8.  **Update Navigation Service:**
    *   Modified `NavigationService.cs` to fetch menu items based on the logged-in user's ID and their corresponding `UserMenuPermissions`, instead of their role.
9.  **Database Migration:**
    *   Generated and applied a new EF Core migration (`ChangePermissionToUserLevel`) to reflect the database schema changes. This involved dropping the `RoleMenuPermissions` table and creating the `UserMenuPermissions` table.
