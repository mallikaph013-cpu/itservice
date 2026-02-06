# ASI Service Blueprint

## 1. Overview

ASI Service is a comprehensive internal IT support ticketing and company news portal. It allows users to submit IT support requests, tracks the request status through an approval workflow, and provides a work queue for IT staff. A key feature is the text-based news management system, which allows authorized administrators to create, edit, delete, and publish company news and announcements. These updates are displayed to all users on the home page and on a dedicated news management page.

## 2. Project Outline & Features

This section documents the project's structure, design, and implemented features from the initial version to the current one.

### 2.1. Core Architecture

*   **.NET 6 / ASP.NET Core MVC:** Built on the Model-View-Controller pattern.
*   **Entity Framework Core & SQLite:** For database operations and local data storage.
*   **ASP.NET Core Identity:** Manages user authentication and role-based authorization (Admin, ITSupport).
*   **Dependency Injection:** Manages services like `ApplicationDbContext`.

### 2.2. Design & Styling

*   **Bootstrap 5:** Primary CSS framework.
*   **Custom Fonts & Icons:** Uses 'Poppins' from Google Fonts and Bootstrap Icons for a modern UI.
*   **Scoped CSS:** For component-specific styles.
*   **Layout & Navigation:** Consistent navigation via `_Layout.cshtml`.
*   **UI Components:**
    *   **Dynamic News Carousel:** Features the 3 most recent news articles with images on the home and news pages. *Note: The ability to add new images has been removed.*
    *   **Rich Text Editor (CKEditor):** For creating and editing news content. The inline image upload feature within the editor is still present but no longer saves the image to the database record.
    *   **Cards, Tables, Badges, Forms:** Standard UI components for data presentation and interaction.

### 2.3. Implemented Features

*   **News Management (Text-Only):**
    *   The `/News/Index` page serves as a full-featured **News Management Dashboard** for authorized users (Admin, ITSupport).
    *   The system is now purely **text-based**, supporting only Title, Content, and Status.
    *   **CRUD Operations:** Admins can Create, Read, Update, and Delete news articles.
    *   **Status Indicators:** Each news card clearly displays its status ("Published" or "Draft").
    *   **Security:** Actions are secured using `[Bind]` attribute to prevent over-posting attacks.
*   **Support Request Management & Workflow:**
    *   Ticketing system with status tracking and work queue.
*   **Reporting:**
    *   Various reports on support tickets and staff workload.

## 4. Current Change Plan: Remove Image Functionality from News

This section outlines the plan and steps taken for the most recent request.

### 4.1. User Request

The user requested to completely remove the image upload feature ("รูปภาพประกอบ") from the news creation and editing process.

### 4.2. Plan & Execution

1.  **Frontend (Views):**
    *   Modified `Views/News/Create.cshtml` to remove the file input field and the form's `enctype`.
    *   Modified `Views/News/Edit.cshtml` to remove the file input field, the current image display, and the form's `enctype`.
2.  **Backend (Controller):**
    *   Refactored `NewsController.cs` comprehensively.
    *   Removed all logic for handling `ImageFile` and `ImagePath` from the `Create`, `Edit`, and `DeleteConfirmed` actions.
    *   Removed the unused helper methods `UploadFile` and `DeleteFile`.
    *   Removed the `UploadImage` action that was used by CKEditor.
    *   Removed the unused `IWebHostEnvironment` dependency from the constructor.
    *   Secured `Create` and `Edit` actions with the `[Bind]` attribute to protect against over-posting.
3.  **Verification:**
    *   Ran `dotnet build` to confirm the changes resulted in a successful build with no errors.

### 4.3. Result

The news management system is now a purely text-based feature. The user interface for creating and editing news is simpler, and the backend code is cleaner, more secure, and free of unnecessary complexity related to file handling.
