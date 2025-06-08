using System;
using System.IO;
using System.Security.Cryptography;

class FileEncryptionTool
{
    private const int KEY_SIZE = 256;      // AES-256
    private const int SALT_SIZE = 16;      // 128 bits
    private const int IV_SIZE = 16;        // 128 bits
    private const int ITERATIONS = 10000;  // PBKDF2 iterations

    static void Main()
    {
        Console.WriteLine("=== File Encryption Tool ===");
        Console.WriteLine("1. Encrypt File");
        Console.WriteLine("2. Decrypt File");
        Console.Write("Choose an option: ");
        string choice = Console.ReadLine();

        Console.Write("Enter file path: ");
        string filePath = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = ReadPassword(); // Secure password input

        if (choice == "1")
        {
            EncryptFile(filePath, password);
        }
        else if (choice == "2")
        {
            DecryptFile(filePath, password);
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }

    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }

        Console.WriteLine();
        return password;
    }

    static void EncryptFile(string filePath, string password)
    {
        byte[] salt = GenerateSalt();
        byte[] iv = GenerateIV();

        using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
        {
            byte[] key = pdb.GetBytes(KEY_SIZE / 8);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                using (FileStream fsOutput = new FileStream(filePath + ".enc", FileMode.Create))
                {
                    fsOutput.Write(salt, 0, salt.Length); // Save salt at start
                    fsOutput.Write(iv, 0, iv.Length);     // Save IV after salt

                    using (CryptoStream cs = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (FileStream fsInput = new FileStream(filePath, FileMode.Open))
                        {
                            CopyStream(fsInput, cs);
                        }
                    }
                }
            }
        }

        Console.WriteLine("File encrypted successfully!");
    }

    static void DecryptFile(string filePath, string password)
    {
        if (!filePath.EndsWith(".enc"))
        {
            Console.WriteLine("Only .enc files can be decrypted.");
            return;
        }

        using (FileStream fsInput = new FileStream(filePath, FileMode.Open))
        {
            byte[] salt = new byte[SALT_SIZE];
            fsInput.Read(salt, 0, SALT_SIZE);

            byte[] iv = new byte[IV_SIZE];
            fsInput.Read(iv, 0, IV_SIZE);

            using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                byte[] key = pdb.GetBytes(KEY_SIZE / 8);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;

                    using (CryptoStream cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        //string outputFilePath = filePath.Replace(".enc", ".dec", StringComparison.OrdinalIgnoreCase);
                        string outputFilePath = filePath.Replace(".enc", ".dec");

                        using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create))
                        {
                            CopyStream(cs, fsOutput);
                        }
                    }
                }
            }
        }

        Console.WriteLine("File decrypted successfully!");
    }

    static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }

    static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SALT_SIZE];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    static byte[] GenerateIV()
    {
        byte[] iv = new byte[IV_SIZE];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }
        return iv;
    }
}