# Aplikasi Vending Machine

Aplikasi web yang mensimulasikan sistem mesin penjual otomatis digital atau Vending Machine. Pengguna dapat mendaftar, login, mengecek saldo, memilih produk, dan melakukan transaksi pembelian layaknya menggunakan mesin penjual otomatis fisik. Sistem mencakup otentikasi pengguna, manajemen saldo, katalog produk, serta pelacakan transaksi secara real-time.

## Fitur Utama

### 🔄 Bisnis Proses
- Sistem vending machine digital yang menyediakan alur e-commerce lengkap: registrasi pengguna, otentikasi, manajemen saldo, katalog produk, dan pelacakan transaksi secara real-time.

### 👣 User Journey
- Pengalaman pengguna yang intuitif: registrasi, login, penjelajahan produk, manajemen saldo, pembelian produk, dan akses ke riwayat transaksi.

### ⚙️ Teknologi
- Dibangun dengan stack teknologi modern: ASP.NET Core MVC, Entity Framework Core, PostgreSQL, dengan keamanan menggunakan BCrypt untuk hashing password.

## Keunggulan Sistem

- 🔐 **Autentikasi Aman**: Password di-hash menggunakan BCrypt untuk keamanan maksimal
- 💳 **Saldo Real-Time**: Pembaruan saldo langsung terlihat setelah setiap transaksi
- 📊 **Monitoring Transaksi**: Pantau semua aktivitas transaksi secara real-time
- 📦 **Manajemen Inventaris**: Kelola stok produk secara efisien dan akurat
- 📋 **Audit Log Terpusat**: Catatan lengkap semua aktivitas sistem untuk keperluan audit

## Alur Proses Bisnis

```
1. User Akses Aplikasi
2. Cek Status Akun (Sudah Punya Akun? → Ya/Tidak)
3. Jika Belum → Registrasi, Jika Sudah → Login
4. Akses Dashboard / Beranda
5. Aksi Paralel:
   - Top Up Saldo → Update Saldo
   - Lihat History
   - Pilih Produk → Validasi Stok & Saldo → Checkout
   - Admin Panel → Tambah/Edit/Hapus Produk
6. Semua Aktivitas Dicatat dalam Log Sistem
```

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