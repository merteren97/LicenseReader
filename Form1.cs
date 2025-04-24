using System.Security.Cryptography;
using System.Text;

namespace LicenseReader
{
    public partial class Form1 : Form
    {
        private static readonly string gizliSifre = "1MertinGizliAnahtari1";
        public Form1()
        {
            InitializeComponent();
        }
        private void btnValidate_Click(object sender, EventArgs e)
        {
            string encryptedText = txtEncryptedInput.Text.Trim();

            try
            {
                string decrypted = Decrypt(encryptedText, gizliSifre);
                string[] parts = decrypted.Split('|');

                if (parts.Length != 2)
                {
                    lblResult.Text = "❌ Geçersiz lisans formatı.";
                    return;
                }

                string decryptedGuid = parts[0];
                DateTime expiryDate = DateTime.Parse(parts[1]);
                string systemGuid = GetMachineGuid();

                if (decryptedGuid != systemGuid)
                {
                    lblResult.Text = "❌ GUID uyuşmuyor.";
                }
                else if (expiryDate < DateTime.Now)
                {
                    lblResult.Text = "❌ Lisans süresi dolmuş.";
                }
                else
                {
                    lblResult.Text = "✅ Lisans geçerli!";
                }
            }
            catch
            {
                lblResult.Text = "❌ Şifre çözülemedi.";
            }
        }
        private string Decrypt(string cipherText, string key)
        {
            byte[] keyBytes = GetFixedSizeKey(key);
            byte[] iv = Encoding.UTF8.GetBytes("1234567890abcdef");

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }
        private byte[] GetFixedSizeKey(string key)
        {
            using SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        private string GetMachineGuid()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
            {
                if (key != null)
                {
                    object value = key.GetValue("MachineGuid");
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}
