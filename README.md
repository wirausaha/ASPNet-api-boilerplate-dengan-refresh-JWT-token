# asp-api-boilerplate
/*===================================================
| Asp Net boilerplate API to develop API base applications using PostgreSQL database & JWT Token. 
| Author : Fajrie R Aradea
|
| Running App for this code avalaible in : http://143.198.221.182:5200
| You can see the list of API's in : http://143.198.221.182:5200/swagger/index.html
| This server is for learning purpose only, please be wise using it
=====================================================*/
You can replace the database with MySQL / SQLServer / PostgreSQL by changing the connection string and adding the EF Core package for the database engine used.

This application requires 2 tables, namely: users and tbsystoken. The structure of the two tables is in the database.txt file, or you can use PostgreSQL-Structure.sql

To be able to run the application, first create a database with the database tool, then create the two tables needed with SQL execution.

Register part is finished, you can try it using postman. To change the language please add Accept-Language = id or en according to the language you choose.

Login, Logout & Refresh Token completed.

To try running the application:

1. First copy the project to local,
2. In the project folder, from the windows terminal execute : dotnet restore 
    to download all the required packages
3. With pgAdmin (if using PostgreSQL): create a database mydb and execute SQL create table in database.txt
4. Edit appsetting.json when needed to change database connection string
5. Edit program.cs if you change database connection string variabel
6. execute without docker: dotnet watch or dotnet run
   with docker please see and edit: docker-compose.yml
7. Testing can be done using the Postman application or frontend app I will upload next






