using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace EnigmaVerse
{
    public partial class Form1 : Form
    {
        private static readonly byte[] IV = new byte[16] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10 };
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "EnigmaVerse " + ProductVersion;
            textBoxKey.MaxLength = 32;
            textBoxKey.UseSystemPasswordChar = true;

            string enigmaVerseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EnigmaVerse");
            if (!Directory.Exists(enigmaVerseFolderPath))
            {
                Directory.CreateDirectory(enigmaVerseFolderPath);
            }
        }

        //UI
        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            string plainText = textBoxInput.Text;
            string key = textBoxKey.Text;
            string encryptedText = Encrypt(plainText, key);
            textBoxOutput.Text = encryptedText;
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            string encryptedText = textBoxInput.Text;
            string key = textBoxKey.Text;
            string decryptedText = Decrypt(encryptedText, key);
            textBoxOutput.Text = decryptedText;
        }

        private void buttonGenKey_Click(object sender, EventArgs e)
        {
            textBoxKey.Clear();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[16];
                rng.GetBytes(keyBytes);
                string randomKey = BitConverter.ToString(keyBytes).Replace("-", "").ToLower();
                textBoxKey.Text = randomKey;
            }
        }

        private void buttonShowKey_Click(object sender, EventArgs e)
        {
            if (textBoxKey.UseSystemPasswordChar == true)
            {
                textBoxKey.UseSystemPasswordChar = false;
                buttonShowKey.Text = "Hide";
            }
            else
            {
                textBoxKey.UseSystemPasswordChar = true;
                buttonShowKey.Text = "Show";
            }
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxInput.Clear();
            textBoxOutput.Clear();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (textBoxKey.Text != "")
            {
                try
                {
                    string enigmaVerseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EnigmaVerse");
                    string fileName = $"EnigmaVerse_Key_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    string filePath = Path.Combine(enigmaVerseFolderPath, fileName);
                    File.WriteAllText(filePath, textBoxKey.Text);
                    MessageBox.Show("File has been saved", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("File could not be saved", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Key is empty", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string enigmaVerseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EnigmaVerse");
            openFileDialog.InitialDirectory = enigmaVerseFolderPath;
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    textBoxKey.Text = fileContent;
                }
                catch
                {
                    MessageBox.Show($"Error loading file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonPath_Click(object sender, EventArgs e)
        {
            string enigmaVerseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EnigmaVerse");
            System.Diagnostics.Process.Start(enigmaVerseFolderPath);
        }

        private void labelTSGames_Click(object sender, EventArgs e)
        {
            string url = "https://tschutscha-games.webador.at/";
            System.Diagnostics.Process.Start(url);
        }

        //Methods

        private string Encrypt(string plainText, string key)
        {
            if (textBoxKey.Text.Length == 32)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = IV;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            else
            {
                MessageBox.Show("The key must be at least 32 characters long", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private string Decrypt(string cipherText, string key)
        {
            if (textBoxKey.Text.Length == 32)
            {
                try
                {
                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = Encoding.UTF8.GetBytes(key);
                        aesAlg.IV = IV;

                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    return srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Decryption error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            else
            {
                MessageBox.Show("The key must be at least 32 characters long", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void textBoxKey_TextChanged(object sender, EventArgs e)
        {
            if (textBoxKey.TextLength == 32)
            {
                textBoxKey.BackColor = Color.Lime;
            }
            else
            {
                textBoxKey.BackColor = Color.Red;
            }
        }
    }
}
