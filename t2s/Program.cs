using System;
using System.IO;
using System.Text;
using System.Threading;

// CREDITS:
//   https://daveaglick.com/posts/capturing-standard-input-in-csharp 

namespace t2s
{
    class Program
    {
        static void Main(string[] args)
        {
            string stdin = null;
            StringBuilder builder = new StringBuilder();

            if((args.Length > 0) && (args[0].ToLowerInvariant() == "--help"))
            {
                Console.WriteLine("t2s converts tabs to '{0}' spaces  (for fixing YAML)", Pad);
                Console.WriteLine("The Environment Variable `PAD` overrides the number of spaces");
                return;
            }

            try
            {

                if (Console.IsInputRedirected)
                {
                    #region "Redirect"
                    using (Stream stream = Console.OpenStandardInput())
                    {
                        byte[] buffer = new byte[1000];
                        int read = -1;
                        while (true)
                        {
                            AutoResetEvent gotInput = new AutoResetEvent(false);
                            Thread inputThread = new Thread(() =>
                            {
                                try
                                {
                                    read = stream.Read(buffer, 0, buffer.Length);
                                    gotInput.Set();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                            })
                            {
                                IsBackground = true
                            };

                            inputThread.Start();

                            // Timeout expired?
                            if (!gotInput.WaitOne(100))
                            {
                                inputThread.Abort();
                                break;
                            }

                            // End of stream?
                            if (read == 0)
                            {
                                stdin = builder.ToString();
                                break;
                            }

                            // Got data
                            builder.Append(Console.InputEncoding.GetString(buffer, 0, read));
                        }
                    }
                    #endregion
                }
                else
                {
                    #region "Read from file"
                    if (args.Length <= 0) throw new FileNotFoundException("No filename given");
                    string fileIn = args[0];
                    if (!File.Exists(fileIn)) throw new FileNotFoundException("File not found", fileIn);
                    var buf = File.ReadAllText(fileIn);
                    builder.Append(buf);
                    buf = null;
                    #endregion
                }

                if (builder.Length <= 0) throw new ArgumentNullException("STDIN", "No data to convert");

                builder = builder.Replace("\t", Padder());

                Console.Out.Write(builder.ToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception: {0}", ex.Message);
            }
        }

        #region "Pad"

        const int Pad_Default = 2;
        static int _pad = Pad_Default;

        static int Pad
        {
            get
            {
                var pads = Environment.GetEnvironmentVariable("PAD");
                if (int.TryParse(pads, out int r))
                {
                    _pad = r;
                }
                else
                {
                    _pad = Pad_Default;
                }
                return _pad;
            }
        }

        static string Padder()
        {
            return " ".PadRight(_pad);
        }

        #endregion

    }
}
