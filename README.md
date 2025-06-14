# asp-api-boilerplate
Asp Net boilerplate API to develop API base applications using PostgreSQL database & JWT Token. 

/* ============================================================
 Documentation is using google translate, I'm sorry if the translation is not accurate. 
============================================================= */  

You can replace the database with MySQL or SQLServer by changing the connection string and adding the EF Core package for the database engine used.

This application requires 2 tables, namely: users and tbsystoken. The structure of the two tables is in the database.txt file

To be able to run the application, first create a database with the database tool, then create the two tables needed with SQL execution.

Register part is finished, you can try it using postman. To change the language please add Accept-Language = id or en according to the language you choose.

Login, Logout & Refresh Token completed.

Untuk mencoba menjalankan aplikasi :

1. Pertama salin project ke lokal, 
2. Pada folder project, dari terminal windows eksekusi 
    - dotnet restore
    untuk mendownload semua paket yang dibutuhkan
3. eksekusi : dotnet watch atau dotnet run
4. Pengetesan bisa dilakukan menggunakan aplkasi Postman




