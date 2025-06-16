# Cloud File Storage System

## Table of Contents

- [Project Overview](#project-overview)
- [Technologies Used](#technologies-used)
- [Architecture](#architecture)
- [Authentication and Authorization](#authentication-and-authorization)
- [File Management](#file-management)
- [Sharing Mechanism](#sharing-mechanism)
- [MVC Application](#mvc-application)
- [Learning Outcomes](#learning-outcomes)

## Project Overview

The Cloud File Storage System is a web application that allows users to securely store, manage, and share files in the cloud. Built with a microservices architecture, it emphasizes modularity and scalability. Key features include:

- User registration and login with secure JWT-based authentication.
- File operations such as uploading, listing, downloading, and deleting.
- Flexible file sharing options: private, public, or shared with specific users.
- A Gateway API that centralizes communication between the frontend and backend services.
- A user-friendly MVC web interface for seamless interaction.

## Technologies Used

- **Backend:**

  - ASP.NET Core for microservices development.
  - SQLite for the Authentication API database.
  - PostgreSQL for the File Metadata API database.
  - MediatR for implementing CQRS.
  - AutoMapper for object mapping.
  - FluentValidation for DTO validation.
  - YARP for the Gateway API reverse proxy.

- **Frontend:**

  - ASP.NET Core MVC for the web application.
  - HTML, CSS, JavaScript, and Bootstrap for the user interface.

- **Authentication:**

  - JWT for secure authentication.
  - BCrypt for password hashing.

- **Version Control:**
  - GitHub for code repository and collaboration.

## Architecture

The system is composed of multiple microservices, each handling a specific function:

1. **Authentication API**: Manages user registration, login, and token refresh using SQLite.
2. **File Metadata API**: Stores file metadata (e.g., name, description) in PostgreSQL.
3. **File Storage API**: Handles file storage and retrieval on the server’s file system.
4. **Gateway API**: Routes requests from the frontend to the appropriate microservice and validates JWT tokens.
5. **MVC Application**: Provides the web interface, communicating with backend services via the Gateway API.

This structure simplifies client-side logic and enhances security by centralizing authentication.

## Authentication and Authorization

Users authenticate through the Authentication API, receiving a JWT token upon login or registration. The token is included in subsequent requests and validated by the Gateway API. File access is restricted based on ownership or sharing permissions, ensuring secure operations.

## File Management

- **Upload**: Users upload files via the MVC application, which are stored by the File Storage API and cataloged by the File Metadata API.
- **List**: Users view their files through the File Metadata API.
- **Download**: Files are retrieved from the File Storage API using file IDs.
- **Delete**: Files are removed from both storage and metadata records.

## Sharing Mechanism

Files can be shared in three ways:

- **Private**: Accessible only to the owner.
- **Public**: Accessible to anyone with the link.
- **Shared with Specific Users**: Owners specify user IDs and permissions (read or edit).

Sharing is managed via the FileShare entity, linking files to users with defined access levels.

## MVC Application

The MVC application offers an intuitive interface with:

- Login and registration pages.
- A file management dashboard for uploading, listing, downloading, and deleting files.
- Modals for editing file details and configuring sharing settings.

It communicates exclusively with the Gateway API, streamlining backend interactions.

## Learning Outcomes

This project provided valuable experience in:

- Designing and implementing microservices architectures.
- Building RESTful APIs with ASP.NET Core.
- Securing applications with JWT authentication.
- Applying the CQRS pattern using MediatR.
- Managing databases with Entity Framework Core.
- Mapping objects with AutoMapper and validating data with FluentValidation.
- Configuring a reverse proxy with YARP.
- Handling file uploads and secure storage.
- Developing user interfaces with ASP.NET Core MVC.
- Practicing effective version control with GitHub.
