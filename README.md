# Aplikasi Mesin Penjual Otomatis

## Petunjuk Pengaturan
Ikuti langkah-langkah berikut untuk mengatur dan menjalankan Aplikasi Mesin Penjual Otomatis:

### Prasyarat
- .NET 8.0 SDK atau yang lebih baru
- Database PostgreSQL (koneksi sudah dikonfigurasi)

### Langkah Instalasi

1. **Instal Dependensi**
   ```bash
   dotnet restore
   ```

2. **Instal Entity Framework Tools secara Global**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Migrasi Database**
   ```bash
   dotnet ef database update
   ```

4. **Jalankan Aplikasi**
   ```bash
   dotnet run
   ```
   atau
   ```bash
   dotnet watch run
   ```

5. **Akses Aplikasi**
   Buka browser Anda dan akses `https://localhost:5001` atau `http://localhost:5000`

### Informasi Tambahan
- Aplikasi dikonfigurasi untuk menggunakan database PostgreSQL yang dihosting di Supabase
- Entity Framework digunakan untuk akses data dengan Pomelo.EntityFrameworkCore.MySql
- Aplikasi mencakup model untuk Pengguna, Produk, dan Transaksi

### Referensi
- [Tutorial ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-8.0&tabs=visual-studio)
- [Bootstrap](https://getbootstrap.com/docs/5.1/getting-started/introduction/)
- [JQuery Doc](https://releases.jquery.com/jquery/)
- [SupabaseDB](https://supabase.com/)