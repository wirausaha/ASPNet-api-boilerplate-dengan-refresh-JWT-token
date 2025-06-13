using System.Text;

namespace AspApi.Utilities
{

  public static class RandomStringGenerator
  {
    private static readonly Random random = new Random();

    public static string GenerateRandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      var result = new StringBuilder(length);

      for (int i = 0; i < length - 10; i++) // Sisakan ruang untuk ticks
      {
        result.Append(chars[random.Next(chars.Length)]);
      }

      // Tambahkan waktu saat ini dalam bentuk ticks (10 karakter terakhir)
      result.Append(DateTime.UtcNow.Ticks.ToString().Substring(0, 10));

      return result.ToString();
    }
  }

}
