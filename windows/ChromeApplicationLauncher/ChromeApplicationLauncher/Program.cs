﻿using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChromeApplicationLauncher
{
    class Program
    {
        public static void Main(string[] args)
        {

            JObject data = Read();
            //Console.WriteLine(arguments.ToString());
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = data["exec"].Value<string>(),
                    Arguments = data["args"].Value<string>(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            p.Start();
            while (!p.StandardOutput.EndOfStream)
            {
                Write(p.StandardOutput.ReadLine());
                // do something with line
            }
            p.WaitForExit();
        }

        /** From https://stackoverflow.com/questions/30880709/c-sharp-native-host-with-chrome-native-messaging **/
        public static JObject Read()
        {
            var stdin = Console.OpenStandardInput();
            var length = 0;

            var lengthBytes = new byte[4];
            stdin.Read(lengthBytes, 0, 4);
            length = BitConverter.ToInt32(lengthBytes, 0);

            var buffer = new char[length];
            using (var reader = new StreamReader(stdin))
            {
                while (reader.Peek() >= 0)
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
            }

            return (JObject)JsonConvert.DeserializeObject<JObject>(new string(buffer));
        }

        public static void Write(JToken data)
        {
            var json = new JObject();
            json["data"] = data;

            var bytes = System.Text.Encoding.UTF8.GetBytes(json.ToString(Formatting.None));

            var stdout = Console.OpenStandardOutput();
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }
    }
}
