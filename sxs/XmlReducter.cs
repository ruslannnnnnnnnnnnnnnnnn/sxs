using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.IO;

namespace sxs
{
    class XmlReducter
    {
        XmlReader Xml;
        public string Name { get; private set; }
        int CountNode;

        public XmlReducter(string fileName)
        {
            if (File.Exists(fileName))
            {
                Name = fileName;
                CountNode = 16;
            }
            else throw new Exception("ошибка имени файла!");
        }

        public XmlReducter(string fileName, int countNode)
        {
            if (File.Exists(fileName))
            {
                Name = fileName;
                CountNode = countNode;
            }
            else throw new Exception("ошибка имени файла!");
        }

        /// <summary>
        /// Записывает в список не более 16 элем. указанной ноды.
        /// </summary>
        /// <param name="nameNode"></param>
        /// <param name="resultList"></param>
        /// <returns>при привышении лимита - true</returns>
        public bool Show(string nameNode, LinkedList<string> resultList)
        {
            bool isLimit = false;

            using (Xml = XmlReader.Create(Name))
            {
                if (Xml.ReadToDescendant(nameNode))
                {
                    if (!Xml.IsEmptyElement) 
                        for (Xml.Read(); !(Xml.Name == nameNode && Xml.NodeType == XmlNodeType.EndElement); Xml.Skip())
                            if (Xml.NodeType == XmlNodeType.Element)
                            {
                                if (isLimit = resultList.Count == CountNode) break;
                                resultList.AddLast(Xml.Name);
                            }
                }
                else throw new Exception("ошибка имени nameNode!");
            }

            return isLimit;
        }

        /// <summary>
        /// Проходит по элементам и записывает в файл.
        /// При достижении лимита дочерних нод переходит к следующиму элементу.
        /// Колличество дочерних элементов опред. через стек.
        /// </summary>
        /// <param name="countNode">Допустимое колличество элементов ноды.</param>
        /// <param name="nameOutFile">Имя файла для записи</param>
        public void Reduce(int countNode, string nameOutFile)
        {
            LinkedList<int> stackIndex = new LinkedList<int>();
            
            using (Xml = XmlReader.Create(Name))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(nameOutFile))
                {
                    Xml.Read();
                    for (int i = 0; !Xml.EOF;)
                    {
                        if (i == countNode)
                        {
                            var withspace = (Xml.NodeType == XmlNodeType.Whitespace) ? Xml.Value : null;
                            while (Xml.NodeType != XmlNodeType.EndElement) Xml.Skip();

                            i = stackIndex.Last.Value;
                            stackIndex.RemoveLast();

                            if (withspace != null)
                                xmlWriter.WriteTabs(stackIndex.Count,
                                    withspace.Substring(withspace.Length - (withspace.Length - 1) / (stackIndex.Count + 1)));

                            xmlWriter.WriteEndElement();
                        }
                        else switch (Xml.NodeType)
                            {
                                case (XmlNodeType.EndElement):
                                    i = stackIndex.Last.Value;
                                    stackIndex.RemoveLast();
                                    xmlWriter.WriteEndElement();
                                    break;
                                case (XmlNodeType.Element):
                                    xmlWriter.WriteStartElement(Xml.Name);
                                    xmlWriter.WriteAttributes(Xml, true);
                                    if (Xml.IsEmptyElement)
                                    {
                                        i++;
                                        xmlWriter.WriteEndElement();
                                    }
                                    else
                                    {
                                        stackIndex.AddLast(i + 1);
                                        i = 0;
                                    }
                                    break;
                                /*case (XmlNodeType.Text):
                                    xmlWriter.WriteString(Xml.Value);
                                    i++;
                                    break;*/
                                case (XmlNodeType.Whitespace):
                                    xmlWriter.WriteWhitespace(Xml.Value);
                                    break;
                                default:
                                    xmlWriter.WriteNode(Xml, true);
                                    continue;
                            }
                        Xml.Read();
                    }                  
                }
            }
        }


        /// <summary>
        /// Проходит к заданному элементу, заносит в стек положения родительских нод.
        /// Используя стек выводит нужные элементы в файл.
        /// </summary>
        /// <param name="nameNode">Имя нужной елемента</param>
        /// <param name="nameOutFile">Имя файла для записи</param>
        public void KeepOne(string nameNode, string nameOutFile)
        {
            LinkedList<int> stackIndex = new LinkedList<int>();
            bool isName = false;

            using (XmlWriter xmlWriter = XmlWriter.Create(nameOutFile))
            {
                using (Xml = XmlReader.Create(Name))
                {              
                    int i = 0;
                    if (Xml.Read() && Xml.NodeType != XmlNodeType.Element)
                    {
                        xmlWriter.WriteNode(Xml, true);
                        i++;
                    }

                    for (; Xml.NodeType != XmlNodeType.Element && !Xml.EOF; i++)
                        if (Xml.NodeType != XmlNodeType.Whitespace)
                        {
                            xmlWriter.WriteWhitespace("\n");
                            xmlWriter.WriteNode(Xml, true);
                        }
                        else Xml.Read();

                    if (Xml.EOF) throw new Exception("ошибка содержимого!");

                    stackIndex.AddLast(i + 1);
                    i = 0;

                    while (!(isName = (Xml.Name == nameNode)) && Xml.Read())
                        if (Xml.NodeType == XmlNodeType.EndElement)
                        {
                            i = stackIndex.Last.Value;
                            stackIndex.RemoveLast();
                        }
                        else if (Xml.NodeType == XmlNodeType.Element)
                        {
                            stackIndex.AddLast(i + 1);
                            i = 0;
                        }
                        else i++;
                }

                if (!isName) throw new Exception("ошибка имени nameNode!");

                using (Xml = XmlReader.Create(Name))
                {
                    Xml.Read();
                    int countTabs = 0, i;

                    foreach (int index in stackIndex)
                    {
                        for (i = index; i != 1; i--, Xml.Skip()) ;
                        for (xmlWriter.WriteWhitespace("\n"), i = 0; i < countTabs; i++, xmlWriter.WriteWhitespace("\t")) ;
                        //xmlWriter.WriteTabs(countTabs);
                        xmlWriter.WriteStartElement(Xml.Name);
                        xmlWriter.WriteAttributes(Xml, true);
                        
                        countTabs++;
                        Xml.Read();
                    }
                    countTabs--;
                    for (; countTabs > -1; countTabs--)
                    {
                        for (xmlWriter.WriteWhitespace("\n"), i = 0; i < countTabs; i++, xmlWriter.WriteWhitespace("\t")) ;
                        //xmlWriter.WriteTabs(countTabs);
                        xmlWriter.WriteEndElement();
                    }
                }
            }
        }
    }
}

