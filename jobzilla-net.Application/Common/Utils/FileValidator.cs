using System.IO;
using System.Linq;

namespace jobzilla_net.Application.Common.Utils;

public static class FileValidator
{
    private static readonly Dictionary<string, byte[]> _fileSignatures = new()
    {
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        { ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // ZIP archive format used by docx
    };

    private static readonly string[] _allowedMimeTypes = new[]
    {
        "image/jpeg",
        "image/png",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public static bool IsValid(Stream fileStream, string fileName, string contentType, long fileLength, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (fileStream == null || fileLength == 0)
        {
            errorMessage = "File is empty.";
            return false;
        }

        if (fileLength > 5 * 1024 * 1024)
        {
            errorMessage = "File exceeds the 5MB limit.";
            return false;
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_fileSignatures.ContainsKey(ext))
        {
            errorMessage = "Invalid file extension.";
            return false;
        }

        if (!_allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            errorMessage = "Invalid MIME type.";
            return false;
        }

        using (var reader = new BinaryReader(fileStream, System.Text.Encoding.Default, leaveOpen: true))
        {
            var signatures = _fileSignatures[ext];
            var headerBytes = reader.ReadBytes(signatures.Length);

            bool match = true;
            for (int i = 0; i < signatures.Length; i++)
            {
                if (headerBytes[i] != signatures[i])
                {
                    match = false;
                    break;
                }
            }

            if (!match)
            {
                errorMessage = "File signature does not match its extension.";
                return false;
            }
        }

        return true;
    }
}
