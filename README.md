# Aplikasi Vending Machine

Aplikasi web yang mensimulasikan sistem mesin penjual otomatis digital atau Vending Machine. Pengunjung dapat langsung mengakses mesin tanpa login, mengecek saldo, memilih produk, dan melakukan transaksi pembelian layaknya menggunakan mesin penjual otomatis fisik. Transaksi pengunjung dicatat atas nama user default **Anonym**. Login hanya tersedia untuk **SuperAdmin** yang mengelola sistem, user, role, produk, dan konfigurasi SMTP. Sistem mencakup manajemen saldo, katalog produk, serta pelacakan transaksi secara real-time.

## Fitur Utama

### 🔄 Bisnis Proses
- Sistem vending machine digital yang menyediakan alur lengkap: akses pengunjung tanpa login, manajemen saldo, katalog produk, checkout, dan pelacakan transaksi secara real-time dengan pencatatan atas nama user **Anonym**.

### 👣 User Journey
- Pengunjung langsung membuka beranda tanpa login, menambah saldo, memilih produk, dan melakukan checkout. Semua transaksi pengunjung tercatat sebagai user **Anonym**. Menu Transaction disembunyikan untuk pengunjung.
- SuperAdmin login melalui halaman login untuk mengelola user, role, produk, transaksi, dan konfigurasi SMTP.

### ⚙️ Teknologi
- Dibangun dengan stack teknologi modern: ASP.NET Core MVC, Entity Framework Core, PostgreSQL, dengan keamanan menggunakan BCrypt untuk hashing password.

## Keunggulan Sistem

- 🔐 **Autentikasi Aman**: Login khusus SuperAdmin, password di-hash menggunakan BCrypt untuk keamanan maksimal
- 💳 **Saldo Real-Time**: Pembaruan saldo langsung terlihat setelah setiap transaksi
- 📊 **Monitoring Transaksi**: Pantau semua aktivitas transaksi secara real-time
- 📦 **Manajemen Inventaris**: Kelola stok produk secara efisien dan akurat
- 📋 **Audit Log Terpusat**: Catatan lengkap semua aktivitas sistem untuk keperluan audit

## Alur Proses Bisnis

```
1. Pengunjung Akses Aplikasi (tanpa login)
2. Sistem menggunakan user default "Anonym" untuk sesi pengunjung
3. Aksi Pengunjung:
   - Top Up Saldo → Update Saldo Anonym
   - Lihat History
   - Pilih Produk → Validasi Stok & Saldo → Checkout
   - Transaksi Dicatat atas Nama Anonym
4. SuperAdmin Login (hanya role VendingApps-SA)
5. Admin Panel:
   - Kelola User & Role
   - Tambah/Edit/Hapus Produk
   - Monitoring Transaksi
   - Konfigurasi SMTP
6. Semua Aktivitas Dicatat dalam Log Sistem
```

> Catatan: User default `anonym@vending.local` (nama tampilan `Anonym`) dibuat otomatis saat pengunjung pertama kali mengakses aplikasi. User ini tidak memiliki role sehingga tidak dapat login.

## Tech Stack

| Komponen | Teknologi |
|----------|-----------|
| Backend | ASP.NET Core MVC (.NET 8.0) |
| Frontend | Razor Pages/HTML, CSS, JavaScript |
| Database | PostgreSQL (dengan Entity Framework Core) |
| Authentication | Cookie-based authentication |
| Password Hashing | BCrypt.Net-Next |
| ORM | Entity Framework Core |
| Web Framework | ASP.NET Core 8.0 |

## Lisensi
© 2026 [Ganesyk12](https://github.com/Ganesyk12). All Rights Reserved.

Proyek ini dikembangkan sebagai portofolio pribadi.

Dilarang menyalin, memodifikasi, mendistribusikan, atau menggunakan sebagian maupun seluruh kode sumber dan dokumentasi untuk tujuan komersial maupun non-komersial tanpa izin tertulis dari pemilik.

Untuk kerja sama atau penggunaan lebih lanjut, silakan hubungi pemilik melalui GitHub.