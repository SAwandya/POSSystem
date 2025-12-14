# POSSystem – Run Guide

This document explains how to **build and run the POSSystem WPF application** locally using Visual Studio.

---

## Prerequisites

Make sure the following are installed on your machine:

* **Windows 10 / 11**
* **Visual Studio 2022** (latest version)
* Visual Studio workloads:

  * ✅ **.NET desktop development**
  * ⬜ Data storage and processing (recommended for SQL Server)
* **.NET 8 SDK**
* **SQL Server Express / Developer** (optional – required for database features)

---

## Solution Structure

```
POSSystem.sln
│
├── POSSystem.UI              # WPF desktop application (Startup project)
├── POSSystem.Application     # Business logic & services
├── POSSystem.Domain          # Core domain entities
├── POSSystem.Infrastructure  # Database & external integrations
├── POSSystem.Common          # Shared utilities
└── README.md
```

---

## How to Open the Project

1. Open **File Explorer**
2. Navigate to the project root folder
3. Double-click **`POSSystem.sln`**

⚠️ Do **not** open the folder directly in Visual Studio (avoid *Open Folder* mode).

---

## How to Run the Application

1. In **Solution Explorer**, right-click:

   * `POSSystem.UI`
2. Select **Set as Startup Project**
3. From the top menu, select:

   * **Build → Rebuild Solution**
4. Press **F5** or click **Start** ▶

If the build succeeds, the **WPF application window will launch**.

---

## Common Build Checks

Before running, ensure:

* `POSSystem.UI` target framework is:

  ```
  net8.0-windows
  ```
* `POSSystem.UI.csproj` contains:

  ```xml
  <UseWPF>true</UseWPF>
  <OutputType>WinExe</OutputType>
  ```
* `POSSystem.UI` is marked as the **startup project**

---

## Build Output Location

After a successful build, the executable will be generated at:

```
POSSystem.UI\bin\Debug\net8.0-windows\POSSystem.UI.exe
```

---

## Troubleshooting

### Issue: `.exe` not found

* Run **Build → Clean Solution**
* Then **Build → Rebuild Solution**
* Check the **Output** window for errors

### Issue: Application namespace conflict

Ensure `App.xaml.cs` inherits explicitly from:

```csharp
System.Windows.Application
```

---

## Notes

* This project follows **clean / layered architecture**
* UI layer depends on Application & Infrastructure layers
* Domain layer has **no external dependencies**

---

## Next Steps

* Configure Dependency Injection
* Setup SQL Server & Entity Framework
* Create Login & Sales screens
* Integrate POS hardware (printer, barcode scanner)

---

Happy coding 🚀
