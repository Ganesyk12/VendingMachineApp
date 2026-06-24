# Redis Message Broker — Implementasi Guide

## Arsitektur

```
┌──────────────────────────────────────────────────────────────────┐
│  Controller (Account, Transaction)                               │
│    ↓ publish                                               ║    │
│  IRedisService.PublishEmailAsync()                         ║    │
│    ↓                                                       ║    │
│  ┌───────────────── REDIS STREAM (email_queue) ─────────┐  ║    │
│  │  Message { toEmail, subject, htmlMessage,             │  ║    │
│  │            attachmentBytes?, attachmentFileName? }     │  ║    │
│  └───────────────────────────────────────────────────────┘  ║    │
│    ↓ read (consumer group)                                  ║    │
│  EmailBackgroundService (BackgroundService)                 ║    │
│    ↓                                                        ║    │
│  IEmailService.SendEmailAsync() / SendEmailWithAttachmentAsync()│
│    ↓                                                        ║    │
│  SMTP → Email Terkirim                                      ║    │
│    ↓                                                        ║    │
│  XACK (hapus dari pending)                                  ║    │
└──────────────────────────────────────────────────────────────────┘
```

## Komponen

### 1. `Services/RedisOptions.cs`
Konfigurasi koneksi Redis. Read dari environment variable.

| Property | Env Var | Default | Keterangan |
|---|---|---|---|
| `Host` | `REDIS_HOST` | `localhost` | Alamat / IP Redis server |
| `Port` | `REDIS_PORT` | `6379` | Port Redis |
| `Password` | `REDIS_PASSWORD` | `""` (kosong) | Password Redis |
| `EmailStream` | `REDIS_STREAM_EMAIL` | `email_queue` | Nama Redis Stream untuk antrian email |
| `ConsumerGroup` | `REDIS_CONSUMER_GROUP` | `email_workers` | Nama consumer group |
| `ConsumerName` | `REDIS_CONSUMER_NAME` | `worker_{MachineName}` | Nama unik tiap worker |

### 2. `Services/RedisService.cs` (Producer)
- Mengirim pesan ke Redis Stream `email_queue`
- Pesan berisi: `toEmail`, `subject`, `htmlMessage`, `attachmentBytes` (base64, opsional), `attachmentFileName` (opsional)
- Gunakan `IRedisService.PublishEmailAsync()` untuk publish

### 3. `Services/EmailBackgroundService.cs` (Consumer)
- `BackgroundService` yang otomatis jalan saat app start
- Membaca pesan dari Redis Stream via `StreamReadGroupAsync` (consumer group)
- Untuk setiap pesan:
  - Inject `IEmailService` via `IServiceProvider.CreateScope()`
  - Kirim email via SMTP
  - Jika sukses → `StreamAcknowledge` (hapus dari pending)
  - Jika gagal → pesan tetap di pending (siap retry)
- Delay 1 detik jika antrian kosong

## Cara Setup

### Redis Server (Docker di VPS)

```bash
# Pull image redis
docker pull redis:7

# Run container (tanpa password)
docker run -d --name redis -p 6379:6379 redis:7

# Run container (dengan password — RECOMMENDED)
docker run -d --name redis -p 6379:6379 redis:7 redis-server --requirepass "password_anda"

# Set password di container yang sudah running
docker exec -it redis redis-cli
CONFIG SET requirepass "password_anda"
AUTH password_anda

# Verifikasi
docker exec redis redis-cli -a "password_anda" PING
# Output: PONG
```

### Security Group / Firewall (AWS EC2)

```bash
# Cek security group di AWS Console:
# EC2 → Security Groups → inbound rules
# Tambah: Custom TCP | TCP | 6379 | IP_RUMAH_ANDA/32

# Cek ufw (Ubuntu)
sudo ufw status

# Cek port binding container
docker ps --filter name=redis --format "{{.Ports}}"
# Harus: 0.0.0.0:6379->6379/tcp (bukan 127.0.0.1)
```

### Environment Variable (.env)

```env
# Redis Configuration (Message Broker)
REDIS_HOST=ip_vps_anda
REDIS_PORT=6379
REDIS_PASSWORD=redis-admin
REDIS_STREAM_EMAIL=email_queue
REDIS_CONSUMER_GROUP=email_workers
REDIS_CONSUMER_NAME=worker_1
```

> **Catatan**: untuk multi-instance, setiap instance harus punya `REDIS_CONSUMER_NAME` unik.

## Monitoring & Debug

### Cek dari VPS

```bash
# Panjang antrian
docker exec redis redis-cli -a "password" XLEN email_queue

# Baca isi pesan (tanpa consume)
docker exec redis redis-cli -a "password" XREAD COUNT 10 STREAMS email_queue 0

# Status consumer group
docker exec redis redis-cli -a "password" XINFO GROUPS email_queue

# Info consumer pending & idle
docker exec redis redis-cli -a "password" XINFO CONSUMERS email_queue email_workers
```

### Cek dari Local (PowerShell)

```powershell
# Test koneksi
Test-NetConnection -ComputerName IP_VPS -Port 6379
# TcpTestSucceeded harus: True
```

### Cek Log Aplikasi

Output console saat app running:

```
EmailBackgroundService started, listening to stream: email_queue
Processing email message {messageId} for user@email.com
Email {messageId} sent and acknowledged
```

## Struktur Message

```
Stream: email_queue

Field              | Type     | Required | Keterangan
-------------------|----------|----------|-------------------------
toEmail            | string   | ✅       | Alamat email tujuan
subject            | string   | ✅       | Judul email
htmlMessage        | string   | ✅       | Body HTML email
attachmentBytes    | string   | ❌       | File attachment (base64)
attachmentFileName | string   | ❌       | Nama file attachment
```

## Flow Perubahan

### Sebelum Redis (Synchronous)

```
Request masuk → Controller → IEmailService.SendEmailAsync() → nunggu SMTP → response
                                                                  ↑
                                                            User nunggu (lama!)
```

### Setelah Redis (Asynchronous)

```
Request masuk → Controller → IRedisService.PublishEmailAsync() → response (cepat!)
                                                                    ↑
                                                              "Sedang dikirim"
```

## Keuntungan

1. **Non-blocking**: response langsung balik tanpa nunggu SMTP
2. **Reliable**: Redis Stream persistent, ga ilang meski worker restart
3. **Retry otomatis**: pesan gagal tetap di pending sampai sukses
4. **Scalable**: tinggal tambah worker dengan consumer name berbeda
5. **Decoupled**: controller ga perlu tau detail SMTP/koneksi email

## Troubleshooting

| Masalah | Solusi |
|---|---|
| `TCP connect failed` | Cek Security Group inbound rule port 6379 |
| `NOAUTH` error | Set `REDIS_PASSWORD` di `.env` sesuai password Redis |
| `ERR BUSYGROUP` | Aman, ini berarti consumer group sudah ada |
| `ERR The server is running without a config file` | `CONFIG REWRITE` gagal — recreate container dengan `--requirepass` |
| Pesan ga pernah dikirim | Cek `EmailBackgroundService` running di log. Cek `XLEN` stream apakah bertambah saat publish |

## File Reference

| File | Kategori |
|---|---|
| `Services/RedisOptions.cs` | Konfigurasi |
| `Services/IRedisService.cs` | Interface producer |
| `Services/RedisService.cs` | Implementasi producer (publish ke stream) |
| `Services/EmailBackgroundService.cs` | Consumer (background service) |
| `Program.cs` | Dependency injection setup |
| `Controllers/AccountController.cs` | Publisher OTP email + OTP caching via Redis |
| `Controllers/TransactionController.cs` | Publisher receipt email |
