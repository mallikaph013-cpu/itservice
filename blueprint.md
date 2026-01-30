
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
*   **Dynamic Forms:** The IT Support request form uses cascading dropdowns to dynamically show and hide relevant form sections based on user input, providing a streamlined user experience.

### 2.2. Technology Stack

*   **Backend:** ASP.NET Core, C#, Entity Framework Core
*   **Frontend:** Razor Pages, Tailwind CSS, JavaScript, jQuery
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

## 3. Current Task: Final Fix for Cascading Dropdown and JS Errors

### 3.1. Goal

To resolve all outstanding JavaScript errors on the "Create IT Service Request" form, ensuring the cascading dropdowns work flawlessly and all page resources load correctly.

### 3.2. Implementation Steps

1.  **Error Identification (from Console):**
    *   `Uncaught SyntaxError: Unexpected token '+'`: A fatal JavaScript syntax error was identified in the BOM table logic within `Create.cshtml`. This was caused by incorrect use of template literal syntax (`${{...}}`) in a standard string context.
    *   `404 (Not Found)` for `myapp.styles.css` and `jquery.validate.*.js`: These errors were symptomatic of the main syntax error. The fatal JS error halted page rendering, preventing the correct paths for these resources from being injected.

2.  **Corrective Actions in `Create.cshtml`:**
    *   **Fixed Syntax Error:** The incorrect template literal syntax in the BOM table's JavaScript section was removed. The code was corrected to use standard string concatenation (`'...' + variable + '...'`).
    *   **Optimized jQuery Logic:** The `updateForm` function was further refined using jQuery's `.toggle(boolean)` function for showing and hiding form sections. This provides a cleaner, more readable, and more efficient implementation than separate `.show()` and `.hide()` calls.

3.  **Result:**
    *   With the fatal syntax error resolved, the browser can now correctly parse and execute all JavaScript on the page.
    *   The cascading dropdown logic, managed by jQuery, now functions as intended.
    *   The secondary `404 Not Found` errors are resolved as the page can now complete its render cycle and correctly reference the static assets.
