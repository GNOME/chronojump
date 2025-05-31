using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BLEHelper.Python.Invoker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var regexData = new Regex(@"^Data Changed: [\dA-Za-z\-]+ = (.+)$");

            var fileName = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "..\\..\\..\\..\\BLEHelper.Python\\runner-win.bat";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = "..\\..\\..\\..\\BLEHelper.Python\\runner-linux.sh";
            }
            else
            {
                fileName = "..\\..\\..\\..\\BLEHelper.Python\\runner-mac.sh";
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = "",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardInputEncoding = System.Text.Encoding.UTF8,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (null == process)
                    {
                        return;
                    }
                    process.EnableRaisingEvents = true;
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            //Console.WriteLine($"{e.Data}");
                        }
                    };
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            //Console.WriteLine(e.Data);
                            var m = regexData.Match(e.Data);
                            if (m.Success)
                            {
                                Console.WriteLine(m.Groups[1].Value);
                            }
                        }
                    };
                    process.WaitForExit();
                    process.CancelOutputRead();
                    process.CancelErrorRead();
                    process.EnableRaisingEvents = false;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error Occurred: {ex.Message}");
                return;
            }
        }
    }
}