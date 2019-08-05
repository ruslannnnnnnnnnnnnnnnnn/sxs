using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Text.RegularExpressions;
using System.IO;


namespace sxs
{
    class ConsoleReduction
    {
        XmlReducter Reduction;

        public void Start()
        {
            Console.WriteLine();
            string input = Console.ReadLine();

            while (input != "break")
            {
                if (!IsAction(input))
                    Console.WriteLine("попробуйте снова.");

                Console.WriteLine();
                input = Console.ReadLine();
            }

            return;
        }

        private string SizeFile(string nameFile)
        {
            if (File.Exists(nameFile))
            {
                long size = new FileInfo(nameFile).Length;

                string[] suf = { "Byt", "KB", "MB", "GB", "TB", "PB", "EB" };
                if (size == 0)
                    return "0" + suf[0];
                long bytes = Math.Abs(size);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);

                return (Math.Sign(size) * num).ToString() + suf[place];
            }
            else throw new Exception("ошибка имени файла!");
        }

        /// <summary>
        /// выбирает действие по заданной команде
        /// </summary>
        /// <param name="str"></param>
        /// <returns>если команда не была выполнина - false</returns>
        private bool IsAction(string str)
        {
            if (!Regex.IsMatch(str, @" --action=(show|reduce|keepone) --filein=(.*\S.*)\.xml.*"))
            {
                Console.WriteLine("ошибка формата!");
                return false;
            }

            string[] inputs = Regex.Split(str, @" --\w+=");

            if (inputs.Length < 4)
            {
                Console.WriteLine("ошибка формата!");
                return false;
            }

            try
            {
                if (Reduction == null || Reduction.Name != inputs[2])
                    Reduction = new XmlReducter(inputs[2]);

                switch (inputs[1])
                {
                    case ("show"):
                        if (Regex.IsMatch(str, @"(.)+ --node=(.*\S.*)"))
                        {
                            LinkedList<string> list = new LinkedList<string>();
                            if (Reduction.Show(inputs[3], list))
                                Console.WriteLine("достигнут лимит!");
                            Show(list);
                        }
                        else throw new Exception("ошибка формата!");
                        break;
                    case ("reduce"):
                        if (Regex.IsMatch(str, @"(.)+ --maxnodes=(.*\S.*) --fileout=(.*\S.*)\.xml"))
                        {
                            Reduction.Reduce(int.Parse(inputs[3]), inputs[4]);
                            Console.WriteLine("file {0} created, size reduce from {1} to {2}",
                                inputs[4], SizeFile(inputs[2]), SizeFile(inputs[4]));
                        }
                        else throw new Exception("ошибка формата!");                          
                        break;
                    case ("keepone"):
                        if (Regex.IsMatch(str, @"(.)+ --node=(.*\S.*) --fileout=(.*\S.*)\.xml"))
                        {
                            Reduction.KeepOne(inputs[3], inputs[4]);
                            Console.WriteLine("file {0} created, size reduce from {1} to {2}",
                                inputs[4], SizeFile(inputs[2]), SizeFile(inputs[4]));
                        }
                        else throw new Exception("ошибка формата!");                    
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private void Show(LinkedList<string> strings)
        {
            foreach (var str in strings)
            {
                Console.WriteLine(" " + str);
            }
        }

    }
}
