# OpenLawOffice

Status: Development

## Installation
### Step 1 - Prerequisites
The below are currently considered the prerequisites and this software is only tested within the Microsoft (R) Visual Studio (R) 2015 IDE with Postgres 9.2 all on Windows 10.

* .NET Framework v4.6
* MVC 5.2.3.0
* Postgresql

### Step 2 - Database
Create a database within your postgresql server.  It doesn't matter what you call it, you can change the name in the web.config discussed later.

### Step 3 - Configure web.config
* Modify your connection string <add name="Postgres": providerName="Npgsql" connectionString="server=...  <-- your Postgresql database information needs to go here
* Modify the elements of the openLawOffice section: <openLawOffice>...</openLawOffice> <-- your configuration settings need to be entered here

### Step 4 - Installation
Launch the site.  You will be directed to the guided installation procedures.  You will create your initial account which will be an administrator account.

### Step 5 - Configuration
After you login, OLO will tell you what it needs you to do, when to do it and how to do it in order to finish your initial configuration.

### Step 6 - Let me know
If you find problems with the installation, file an issue and let me know.
