using VendingMachineApp.Models;

namespace VendingMachineApp.Helpers
{
    public static class MessageBuilder
    {
        public static string BuildReceiptEmailBody(UserTransaction transaction)
        {
            var customerName = transaction.User?.UserBalance?.Name ?? "Guest";
            return $@"
                <div style='font-family: sans-serif; color: #333;'>
                    <h2 style='color: #0dcaf0;'>Terima Kasih Telah Berbelanja!</h2>
                    <p>Halo <b>{customerName}</b>,</p>
                    <p>Terlampir adalah struk digital untuk transaksi Anda dengan kode <b>{transaction.TrxCode}</b> pada tanggal {transaction.Date:dd MMMM yyyy HH:mm:ss}.</p>
                    <p>Total belanja Anda: <b style='color: #198754;'>Rp {transaction.TotalAmount:N0}</b></p>
                    <br/>
                    <p>Semoga harimu menyenangkan!</p>
                    <p>Salam hangat,</p>
                    <p><b>Vending Machine App</b></p>
                </div>";
        }

        public static string BuildVerificationEmailBody(string customerName, string verificationCode)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2>Kode Verifikasi Pendaftaran</h2>
                    <p>Halo {customerName},</p>
                    <p>Gunakan kode 6 digit berikut untuk menyelesaikan pendaftaran Anda. Kode ini hanya berlaku selama <strong>1 menit</strong>.</p>
                    <h1 style='color: #0dcaf0; font-size: 32px; letter-spacing: 5px;'>{verificationCode}</h1>
                    <p>Jika Anda tidak merasa mendaftar di Vending App, abaikan email ini.</p>
                </div>";
        }

        public static string BuildTestEmailBody()
        {
            return
                "<h1>Email Berhasil Terkirim!</h1><p>Jika Anda melihat pesan ini, konfigurasi SMTP Anda (SmtpEmails) berhasil bekerja dengan baik.</p>";
        }
    }
}
