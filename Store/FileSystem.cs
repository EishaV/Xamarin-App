using System;
using System.IO;
using System.Text;

namespace BackEnd {
  public delegate void ErrDelegte(string msg);
  public delegate void LogDelegte(string log, int c = 0);

  public static class Store {
    private static string FilePath(string name) {
      string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

      if( !Directory.Exists(dir) ) Directory.CreateDirectory(dir);
      return Path.Combine(dir, name);
    }

    public static bool Exist(string name) { return File.Exists(FilePath(name)); }

    public static string[] Filter(string filter) {
      return Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, filter);
    }

    public static byte[] LoadBytes(string name) { return File.ReadAllBytes(FilePath(name)); }
    public static void SaveBytes(string name, byte[] data) { File.WriteAllBytes(FilePath(name), data); }

    public static string LoadText(string name) { return File.ReadAllText(FilePath(name)); }
    public static void SaveText(string name, string text) { File.WriteAllText(FilePath(name), text); }
  }
}
